using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.Events;

// 실습 종류 정의
public enum TutorialCondition { None, CameraMove, PartPlacement }

public enum RequiredPartGroup { Any, Defense, Attack, Custom }

[System.Serializable]
public class TutorialStep
{
    public string SpeakerName;
    [TextArea(3, 5)] public string Message;
    public Sprite PortraitSprite;
    public RectTransform TargetUI;
    public int[] RequiredPartKeys; // optional: if set, only these part keys count for PartPlacement
    public RequiredPartGroup RequiredGroup = RequiredPartGroup.Any;

    [Header("UI Motion")]
    public bool MoveDialogUp = false; // 이 단계에서 다이얼로그 패널을 위로 올릴지
    public float MoveOffset = 300f;   // 위로 이동할 거리 (px)
    public float MoveDuration = 0.5f; // 이동 애니메이션 시간(초)

    [Header("Control Settings")]
    public bool ShouldPause = true;      // 이 단계에서 시간을 멈출 것인가?
    public bool BlockInput = true;       // 이 단계에서 플레이어 조작을 막을 것인가?

    [Header("Action Conditions")]
    public TutorialCondition Condition = TutorialCondition.None;
    public float RequiredAmount = 1.0f;
    [Tooltip("If > 0, dialog will auto-advance after this many seconds (unscaled). Set 0 to disable.")]
    public float AutoAdvanceDelay = 3.0f;

    public string PlacementLabel; // optional: PartPlacement 스텝에서 인스펙터로 표시 이름을 덮어쓸 때 사용

    [Header("Portrait Motion")]
    public bool MovePortraitLeft = false; // 이 단계에서 포트레이트를 왼쪽으로 이동시킬지
    public float PortraitMoveOffset = 150f; // 왼쪽으로 이동할 거리(px)
    public float PortraitMoveDuration = 0.5f; // 이동 애니메이션 시간(초)
    public RectTransform PortraitTarget; // optional: 이동할 RectTransform 위치를 지정하면 그 좌표로 이동
}

[DefaultExecutionOrder(-90)]
public class TutorialManager : Singleton<TutorialManager>
{
    private const float CAMERA_DEFAULT_SIZE = 10f;
    private const float CAMERA_DEFAULT_POS_Z = -10f;
    private const int DEFENSE_PART_MIN = 1;
    private const int DEFENSE_PART_MAX = 3;
    private const int ATTACK_PART_MIN = 4;
    private const int ATTACK_PART_MAX = 6;

    [Header("UI References")]
    [SerializeField] private GameObject _dialogPanel;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _dialogText;
    [SerializeField] private Image _portraitImage;
    [SerializeField] private RectTransform _highlighter;
    [SerializeField] private Button _clickButton;
    [SerializeField] private GameObject _postPanel;

    // 배치 진행도 UI: 튜토리얼 스텝이 PartPlacement 일 때 사용
    [Header("Placement Progress UI")]
    [SerializeField] private GameObject _placementProgressPanel; // 전체 패널(Show/Hide 용)
    [SerializeField] private TextMeshProUGUI _placementProgressText; // "공격 유닛 n개 배치하기 (0/n)" 텍스트

    [Header("Settings")]
    [SerializeField] private float _typingSpeed = 0.05f;
    [SerializeField] private float _cameraResetDuration = 1f;
    [SerializeField] private TutorialStep[] _steps;

    [Header("Highlighter Pulse Effect")]
    [SerializeField] private float _pulseScale = 1.1f;
    [SerializeField] private float _pulseSpeed = 4.0f;

    [Header("Debug & Manual Start")]
    [SerializeField] private bool _startOnAwake = true;

    private int _currentStep = 0;
    private bool _isTyping = false;
    private bool _cancelTyping = false;
    private bool _moveNext = false;
    private bool _isShowing = false;

    private float _currentProgress = 0f;
    private bool _conditionMet = false;
    private bool _isResettingCamera = false;
    private Coroutine _dialogMoveCoroutine = null;
    private Coroutine _pulseCoroutine = null;
    private Coroutine _portraitMoveCoroutine = null;
    private Vector2 _dialogOriginalAnchoredPos = Vector2.zero;
    private Vector2 _highlighterBaseSize = Vector2.zero;
    private Vector2 _clickButtonOriginalAnchoredPos = Vector2.zero;
    private Vector2 _portraitOriginalAnchoredPos = Vector2.zero;
    private bool _portraitOriginalCached = false;
    private Coroutine _dialogCameraCoroutine = null;
    private float _dialogCamOriginalSize = CAMERA_DEFAULT_SIZE;
    private Vector3 _dialogCamOriginalPos = Vector3.zero;

    // 현재 PartPlacement 스텝의 요구 개수와 라벨 캐시
    private int _currentStepRequiredCount = 0;
    private string _currentPlacementLabel = "";

    private void OnEnable()
    {
        if (EventBus.Instance != null)
        {
            EventBus.Instance.Subscribe<StageLoadedEvent>(OnStageLoaded);
            EventBus.Instance.Subscribe<ScrollEvent>(OnCameraZoomed);
            EventBus.Instance.Subscribe<RightClickEvent>(OnCameraDragged);
            EventBus.Instance.Subscribe<PartPlacedEvent>(OnPartPlaced);
            // 튜토리얼: 공격 배치 모드 진입 요청 구독
            EventBus.Instance.Subscribe<AttackPlacementTutorialRequestedEvent>(OnAttackPlacementTutorialRequested);
        }
    }

    // 공격 배치 튜토리얼 요청 처리
    private void OnAttackPlacementTutorialRequested(AttackPlacementTutorialRequestedEvent evt)
    {
        // 이미 다이얼로그가 보이는 중이면 무시
        if (_isShowing) return;
        if (!StageLoadContext.IsTutorial) return;

        StartCoroutine(ShowAttackPlacementTutorialRoutine(evt.PartKey));
    }

    private IEnumerator ShowAttackPlacementTutorialRoutine(int partKey)
    {
        _isShowing = true;
        InputReader.Instance?.SetInputBlocked(true);

        if (_dialogPanel != null)
        {
            _dialogPanel.SetActive(true);
            if (_nameText != null) _nameText.text = "설치 규칙";
            if (_dialogText != null)
                _dialogText.text = "공격 유닛은 공중에 설치할 수 없습니다.\n공격 유닛끼리는 서로 인접해서 배치할 수 없습니다.\n가능한 타일에만 배치하세요.\n(확인 클릭)";
        }

        UnityAction temp = () => { _moveNext = true; };
        if (_clickButton != null) _clickButton.onClick.AddListener(temp);

        _moveNext = false;
        while (!_moveNext) yield return null;

        if (_clickButton != null) _clickButton.onClick.RemoveListener(temp);
        if (_dialogPanel != null) _dialogPanel.SetActive(false);

        InputReader.Instance?.SetInputBlocked(false);
        _moveNext = false;
        _isShowing = false;

        EventBus.Instance?.Publish(new AttackPlacementTutorialEndedEvent());
    }

    private void OnDisable()
    {
        if (EventBus.Instance != null)
        {
            EventBus.Instance.Unsubscribe<StageLoadedEvent>(OnStageLoaded);
            EventBus.Instance.Unsubscribe<ScrollEvent>(OnCameraZoomed);
            EventBus.Instance.Unsubscribe<RightClickEvent>(OnCameraDragged);
            EventBus.Instance.Unsubscribe<PartPlacedEvent>(OnPartPlaced);
            EventBus.Instance.Unsubscribe<AttackPlacementTutorialRequestedEvent>(OnAttackPlacementTutorialRequested);
        }
    }

    private void Start()
    {
        if (_dialogPanel != null) _dialogPanel.SetActive(false);
        if (_highlighter != null) _highlighter.gameObject.SetActive(false);
        if (_placementProgressPanel != null) _placementProgressPanel.SetActive(false);
        if (_placementProgressText != null) _placementProgressText.gameObject.SetActive(false);

        // ClickButton 원위치 저장
        if (_clickButton != null)
        {
            var cbRt = _clickButton.GetComponent<RectTransform>();
            if (cbRt != null) _clickButtonOriginalAnchoredPos = cbRt.anchoredPosition;
        }

        if (_clickButton != null)
        {
            _clickButton.onClick.AddListener(HandlePointerClick);
        }

        if (_startOnAwake)
        {
            TryStartTutorial();
        }
    }

    private void Update()
    {
        if (!_isShowing) return;

        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame)
        {
            if (_isTyping) { _cancelTyping = true; return; }
            if (_steps == null || _currentStep < 0 || _currentStep >= _steps.Length) return;
            if (_steps[_currentStep].Condition == TutorialCondition.None) _moveNext = true;
        }
    }

    private void OnStageLoaded(StageLoadedEvent evt)
    {
        if (_isShowing || _steps == null || _steps.Length == 0) return;
        TryStartTutorial();
    }

    private void TryStartTutorial()
    {
        if (_isShowing) return;
        _isShowing = true;
        StartCoroutine(TutorialSequenceRoutine());
    }

    private void HandlePointerClick()
    {
        if (_steps == null || _currentStep < 0 || _currentStep >= _steps.Length) return;
        if (_isTyping) _cancelTyping = true;
        else if (_steps[_currentStep].Condition == TutorialCondition.None) _moveNext = true;
    }

    private IEnumerator TutorialSequenceRoutine()
    {
        yield return null;
        for (_currentStep = 0; _currentStep < _steps.Length; _currentStep++)
        {
            yield return StartCoroutine(ShowDialogRoutine(_steps[_currentStep]));
        }

        CleanupTutorial();

        if (_postPanel != null)
        {
            _postPanel.SetActive(true);
        }
    }

    private IEnumerator ShowDialogRoutine(TutorialStep step)
    {
        _currentProgress = 0f;
        _conditionMet = false;
        _currentStepRequiredCount = 0;
        _currentPlacementLabel = "";

        Time.timeScale = step.ShouldPause ? 0f : 1f;
        InputReader.Instance?.SetInputBlocked(step.BlockInput);

        if (_dialogPanel != null)
        {
            _dialogPanel.SetActive(true);
            var rt = _dialogPanel.GetComponent<RectTransform>();
            if (rt != null) _dialogOriginalAnchoredPos = rt.anchoredPosition;
        }

        if (_nameText != null) _nameText.text = step.SpeakerName;

        if (step.Condition != TutorialCondition.CameraMove)
        {
            StopDialogCameraCoroutine();
            _dialogCameraCoroutine = StartCoroutine(DialogCameraLockRoutine(0.25f));
        }

        // 다이얼로그 이동
        if (step.MoveDialogUp && _dialogPanel != null)
        {
            if (_dialogMoveCoroutine != null) StopCoroutine(_dialogMoveCoroutine);
            _dialogMoveCoroutine = StartCoroutine(MoveDialogUpRoutine(step.MoveOffset, step.MoveDuration));
        }

        // 포트레이트 설정
        if (_portraitImage != null)
        {
            bool hasPortrait = step.PortraitSprite != null;
            _portraitImage.gameObject.SetActive(hasPortrait);
            if (hasPortrait) _portraitImage.sprite = step.PortraitSprite;

            var portraitRt = _portraitImage.GetComponent<RectTransform>();
            if (portraitRt != null)
            {
                // 최초에만 원위치 캐시
                if (!_portraitOriginalCached)
                {
                    _portraitOriginalAnchoredPos = portraitRt.anchoredPosition;
                    _portraitOriginalCached = true;
                }

                if (step.MovePortraitLeft || step.PortraitTarget != null)
                {
                    StopPortraitMove();
                    Vector2 targetAnchored;
                    if (step.PortraitTarget != null)
                    {
                        targetAnchored = step.PortraitTarget.anchoredPosition;
                    }
                    else
                    {
                        targetAnchored = _portraitOriginalAnchoredPos + new Vector2(-Mathf.Abs(step.PortraitMoveOffset), 0f);
                    }

                    _portraitMoveCoroutine = StartCoroutine(MovePortraitToRoutine(portraitRt, targetAnchored, step.PortraitMoveDuration));
                }
                else
                {
                    // 복귀 애니메이션
                    StopPortraitMove();
                    StartCoroutine(MovePortraitBackRoutine(portraitRt, step.PortraitMoveDuration));
                }
            }
        }

        // 하이라이터 설정 및 펄스 시작
        if (_highlighter != null)
        {
            StopHighlighterPulse();
            if (step.TargetUI != null)
            {
                _highlighter.gameObject.SetActive(true);
                _highlighter.position = step.TargetUI.position;
                _highlighterBaseSize = step.TargetUI.sizeDelta;
                _highlighter.sizeDelta = _highlighterBaseSize;
                StartHighlighterPulse();
            }
            else
            {
                _highlighter.gameObject.SetActive(false);
            }
        }

        // 타이핑 로직
        if (_dialogText != null) _dialogText.text = "";
        _isTyping = true;
        _cancelTyping = false;
        if (!string.IsNullOrEmpty(step.Message))
        {
            foreach (char c in step.Message.ToCharArray())
            {
                if (_cancelTyping) break;
                if (_dialogText != null) _dialogText.text += c;
                yield return new WaitForSecondsRealtime(_typingSpeed);
            }
            if (_dialogText != null) _dialogText.text = step.Message;
        }
        _isTyping = false;
        _moveNext = false;

        // PartPlacement 전용: 진행도 UI 초기화 및 표시
        if (step.Condition == TutorialCondition.PartPlacement)
        {
            // 요구 개수는 RequiredAmount 기준(정수 기대)
            _currentStepRequiredCount = Mathf.Max(1, Mathf.CeilToInt(step.RequiredAmount));

            // 라벨은 인스펙터에서 덮어쓸 수 있도록 우선 사용하고, 없으면 그룹 기준 자동 생성
            if (!string.IsNullOrWhiteSpace(step.PlacementLabel))
            {
                _currentPlacementLabel = step.PlacementLabel;
            }
            else
            {
                switch (step.RequiredGroup)
                {
                    case RequiredPartGroup.Attack:
                        _currentPlacementLabel = "공격 유닛";
                        break;
                    case RequiredPartGroup.Defense:
                        _currentPlacementLabel = "방어 유닛";
                        break;
                    case RequiredPartGroup.Custom:
                        _currentPlacementLabel = "선택 유닛";
                        break;
                    default:
                        _currentPlacementLabel = "유닛";
                        break;
                }
            }

            ShowPlacementProgressPanel(true);
            UpdatePlacementProgressUI();
        }

        // CameraMove 전용: PartPlacement과 동일한 방식으로 진행도 UI 표시
        if (step.Condition == TutorialCondition.CameraMove)
        {
            _currentStepRequiredCount = Mathf.Max(1, Mathf.CeilToInt(step.RequiredAmount));
            if (!string.IsNullOrWhiteSpace(step.PlacementLabel))
            {
                _currentPlacementLabel = step.PlacementLabel;
            }
            else
            {
                _currentPlacementLabel = "카메라 이동";
            }

            ShowPlacementProgressPanel(true);
            UpdatePlacementProgressUI();
        }

        // 실습 조건 처리
        if (step.Condition != TutorialCondition.None)
        {
            Time.timeScale = 1f;
            InputReader.Instance?.SetInputBlocked(false);
            while (!_conditionMet) yield return null;

            if (step.Condition == TutorialCondition.CameraMove)
            {
                _isResettingCamera = true;
                yield return StartCoroutine(ResetCameraRoutine(_cameraResetDuration));
                _isResettingCamera = false;
            }
        }
        else
        {
            if (step.AutoAdvanceDelay > 0f)
            {
                float elapsed = 0f;
                while (!_moveNext && elapsed < step.AutoAdvanceDelay)
                {
                    elapsed += Time.unscaledDeltaTime;
                    yield return null;
                }

                if (!_moveNext) _moveNext = true;
            }
            else
            {
                while (!_moveNext) yield return null;
            }
        }

        StopHighlighterPulse();
        StopDialogCameraCoroutine();

        // Portrait가 이동해있다면 스텝 종료 시 원위치로 복귀시키되 **대기하지 않음**
        if (_portraitImage != null && _portraitOriginalCached)
        {
            var portraitRt = _portraitImage.GetComponent<RectTransform>();
            if (portraitRt != null)
            {
                StopPortraitMove();
                // 비동기 복귀: 완료를 기다리지 않음 -> 입력으로 즉시 다음 스텝 진행 가능
                StartCoroutine(MovePortraitBackRoutine(portraitRt, step.PortraitMoveDuration));
            }
        }

        // PartPlacement UI 숨김
        ShowPlacementProgressPanel(false);

        if (step.MoveDialogUp && _dialogPanel != null)
        {
            if (_dialogMoveCoroutine != null) StopCoroutine(_dialogMoveCoroutine);
            yield return StartCoroutine(MoveDialogDownRoutine(step.MoveDuration));
            _dialogMoveCoroutine = null;
        }
    }

    private void StartHighlighterPulse()
    {
        if (_pulseCoroutine != null) StopCoroutine(_pulseCoroutine);
        _pulseCoroutine = StartCoroutine(HighlighterPulseRoutine());
    }

    private void StartPortraitMove(RectTransform portraitRt, float offset, float duration)
    {
        if (portraitRt == null) return;
        if (_portraitMoveCoroutine != null) StopCoroutine(_portraitMoveCoroutine);
        Vector2 targetAnchored = portraitRt.anchoredPosition + new Vector2(-Mathf.Abs(offset), 0f);
        _portraitMoveCoroutine = StartCoroutine(MovePortraitToRoutine(portraitRt, targetAnchored, duration));
    }

    private void StopPortraitMove()
    {
        if (_portraitMoveCoroutine != null)
        {
            StopCoroutine(_portraitMoveCoroutine);
            _portraitMoveCoroutine = null;
        }
    }

    private void StopHighlighterPulse()
    {
        if (_pulseCoroutine != null)
        {
            StopCoroutine(_pulseCoroutine);
            _pulseCoroutine = null;
        }
    }

    private IEnumerator HighlighterPulseRoutine()
    {
        while (true)
        {
            float t = (Mathf.Sin(Time.unscaledTime * _pulseSpeed) + 1f) / 2f;
            if (_highlighter != null)
            {
                _highlighter.sizeDelta = Vector2.Lerp(_highlighterBaseSize, _highlighterBaseSize * _pulseScale, t);
            }
            yield return null;
        }
    }

    private IEnumerator MovePortraitToRoutine(RectTransform portraitRt, Vector2 targetAnchored, float duration)
    {
        if (portraitRt == null) yield break;

        Vector2 start = portraitRt.anchoredPosition;
        Vector2 target = targetAnchored;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            portraitRt.anchoredPosition = Vector2.Lerp(start, target, t);
            yield return null;
        }

        portraitRt.anchoredPosition = target;
    }

    private IEnumerator MovePortraitBackRoutine(RectTransform portraitRt, float duration)
    {
        if (portraitRt == null) yield break;

        Vector2 current = portraitRt.anchoredPosition;
        Vector2 target = _portraitOriginalAnchoredPos; // use cached original position
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            portraitRt.anchoredPosition = Vector2.Lerp(current, target, t);
            yield return null;
        }

        portraitRt.anchoredPosition = target;
    }

    private void StopDialogCameraCoroutine()
    {
        if (_dialogCameraCoroutine != null)
        {
            StopCoroutine(_dialogCameraCoroutine);
            _dialogCameraCoroutine = null;
        }
    }

    private void CleanupTutorial()
    {
        _isShowing = false;
        StopHighlighterPulse();
        if (_dialogPanel != null) _dialogPanel.SetActive(false);
        if (_highlighter != null) _highlighter.gameObject.SetActive(false);
        if (_placementProgressPanel != null) _placementProgressPanel.SetActive(false);
        if (_placementProgressText != null) _placementProgressText.gameObject.SetActive(false);
        // 포트레이트 위치 복귀
        if (_portraitImage != null && _portraitOriginalCached)
        {
            var portraitRt = _portraitImage.GetComponent<RectTransform>();
            if (portraitRt != null)
            {
                StopPortraitMove();
                StartCoroutine(MovePortraitBackRoutine(portraitRt, 0.25f));
            }
        }
        InputReader.Instance?.SetInputBlocked(false);
        Time.timeScale = 1f;
    }

    private IEnumerator MoveDialogUpRoutine(float offset, float duration)
    {
        if (_dialogPanel == null) yield break;

        var rt = _dialogPanel.GetComponent<RectTransform>();
        Vector2 start = rt.anchoredPosition;
        Vector2 target = start + new Vector2(0f, offset);
        Vector2 buttonOffset = new Vector2(0f, offset);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            rt.anchoredPosition = Vector2.Lerp(start, target, t);

            if (_clickButton != null)
            {
                var cbRt = _clickButton.GetComponent<RectTransform>();
                if (cbRt != null)
                    cbRt.anchoredPosition = Vector2.Lerp(_clickButtonOriginalAnchoredPos, _clickButtonOriginalAnchoredPos + buttonOffset, t);
            }
            yield return null;
        }

        rt.anchoredPosition = target;
        if (_clickButton != null)
        {
            var cbRt = _clickButton.GetComponent<RectTransform>();
            if (cbRt != null)
                cbRt.anchoredPosition = _clickButtonOriginalAnchoredPos + buttonOffset;
        }
    }

    private IEnumerator MoveDialogDownRoutine(float duration)
    {
        if (_dialogPanel == null) yield break;
        var rt = _dialogPanel.GetComponent<RectTransform>();
        Vector2 start = rt.anchoredPosition;
        Vector2 target = _dialogOriginalAnchoredPos;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            rt.anchoredPosition = Vector2.Lerp(start, target, Mathf.Clamp01(elapsed / duration));
            if (_clickButton != null)
            {
                var cbRt = _clickButton.GetComponent<RectTransform>();
                if (cbRt != null)
                    cbRt.anchoredPosition = Vector2.Lerp(_clickButtonOriginalAnchoredPos + new Vector2(0f, start.y - target.y), _clickButtonOriginalAnchoredPos, Mathf.Clamp01(elapsed / duration));
            }
            yield return null;
        }
        rt.anchoredPosition = target;
        if (_clickButton != null)
        {
            var cbRt = _clickButton.GetComponent<RectTransform>();
            if (cbRt != null)
                cbRt.anchoredPosition = _clickButtonOriginalAnchoredPos;
        }
    }

    // --- 실습 이벤트 핸들러 ---
    private void OnCameraZoomed(ScrollEvent e)
    {
        if (!IsCurrentStepCondition(TutorialCondition.CameraMove)) return;

        _currentProgress += 0.2f;
        UpdatePlacementProgressUI();
        CheckCondition();
    }

    private void OnCameraDragged(RightClickEvent e)
    {
        if (!IsCurrentStepCondition(TutorialCondition.CameraMove) || !e.IsStarted) return;

        _currentProgress += 0.1f;
        UpdatePlacementProgressUI();
        CheckCondition();
    }

    private void OnPartPlaced(PartPlacedEvent e)
    {
        if (!IsCurrentStepCondition(TutorialCondition.PartPlacement)) return;

        var step = _steps[_currentStep];
        if (!IsPartKeyMatchingGroup(e.PartKey, step.RequiredGroup, step.RequiredPartKeys)) return;

        _currentProgress += 1.0f;
        UpdatePlacementProgressUI();
        CheckCondition();
    }

    private bool IsCurrentStepCondition(TutorialCondition condition)
    {
        if (!_isShowing || _isResettingCamera) return false;
        if (_steps == null || _currentStep < 0 || _currentStep >= _steps.Length) return false;
        return _steps[_currentStep].Condition == condition;
    }

    private bool IsPartKeyMatchingGroup(int partKey, RequiredPartGroup group, int[] customKeys)
    {
        switch (group)
        {
            case RequiredPartGroup.Any:
                return true;

            case RequiredPartGroup.Defense:
                return partKey >= DEFENSE_PART_MIN && partKey <= DEFENSE_PART_MAX;

            case RequiredPartGroup.Attack:
                return partKey >= ATTACK_PART_MIN && partKey <= ATTACK_PART_MAX;

            case RequiredPartGroup.Custom:
                if (customKeys == null || customKeys.Length == 0) return false;
                foreach (var key in customKeys)
                {
                    if (key == partKey) return true;
                }
                return false;

            default:
                return false;
        }
    }

    private void CheckCondition()
    {
        if (_steps == null || _currentStep < 0 || _currentStep >= _steps.Length) return;
        if (_currentProgress >= _steps[_currentStep].RequiredAmount) _conditionMet = true;
    }

    private IEnumerator ResetCameraRoutine(float duration)
    {
        Camera mainCam = Camera.main;
        if (mainCam == null) yield break;

        float startSize = mainCam.orthographicSize;
        Vector3 startPos = mainCam.transform.position;
        Vector3 targetPos = new Vector3(0f, 0f, CAMERA_DEFAULT_POS_Z);
        float elapsed = 0f;

        InputReader.Instance?.SetInputBlocked(true);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            mainCam.orthographicSize = Mathf.Lerp(startSize, CAMERA_DEFAULT_SIZE, t);
            mainCam.transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        mainCam.orthographicSize = CAMERA_DEFAULT_SIZE;
        mainCam.transform.position = targetPos;
        InputReader.Instance?.SetInputBlocked(false);
        _isResettingCamera = false;
    }

    private IEnumerator DialogCameraLockRoutine(float duration)
    {
        Camera mainCam = Camera.main;
        if (mainCam == null) yield break;

        _dialogCamOriginalSize = mainCam.orthographicSize;
        _dialogCamOriginalPos = mainCam.transform.position;

        float elapsed = 0f;
        Vector3 targetPos = new Vector3(0f, 0f, CAMERA_DEFAULT_POS_Z);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            mainCam.orthographicSize = Mathf.Lerp(_dialogCamOriginalSize, CAMERA_DEFAULT_SIZE, t);
            mainCam.transform.position = Vector3.Lerp(_dialogCamOriginalPos, targetPos, t);
            yield return null;
        }

        mainCam.orthographicSize = CAMERA_DEFAULT_SIZE;
        mainCam.transform.position = targetPos;

        while (true) yield return null;
    }

    // --- Placement progress UI helpers ---
    private void ShowPlacementProgressPanel(bool show)
    {
        if (_placementProgressPanel != null)
            _placementProgressPanel.SetActive(show);
        if (_placementProgressText != null)
            _placementProgressText.gameObject.SetActive(show);
    }

    private void UpdatePlacementProgressUI()
    {
        if (_placementProgressText == null) return;

        // 현재 스텝이 CameraMove이면 (0/n) 형식의 진행도는 표시하지 않음 — 라벨만 표시
        if (_steps != null && _currentStep >= 0 && _currentStep < _steps.Length && _steps[_currentStep].Condition == TutorialCondition.CameraMove)
        {
            if (!string.IsNullOrWhiteSpace(_currentPlacementLabel))
                _placementProgressText.text = _currentPlacementLabel;
            else
                _placementProgressText.text = "";
            return;
        }

        int current = Mathf.FloorToInt(_currentProgress);
        int required = Mathf.Max(1, _currentStepRequiredCount);
        if (!string.IsNullOrWhiteSpace(_currentPlacementLabel))
            _placementProgressText.text = $"{_currentPlacementLabel} ({current}/{required})";
        else
            _placementProgressText.text = $"({current}/{required})";
    }
}