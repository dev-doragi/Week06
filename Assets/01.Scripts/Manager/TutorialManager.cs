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
    private Vector2 _dialogOriginalAnchoredPos = Vector2.zero;
    private Vector2 _highlighterBaseSize = Vector2.zero;
    private Vector2 _clickButtonOriginalAnchoredPos = Vector2.zero;
    private Coroutine _dialogCameraCoroutine = null;
    private float _dialogCamOriginalSize = CAMERA_DEFAULT_SIZE;
    private Vector3 _dialogCamOriginalPos = Vector3.zero;

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
            while (!_moveNext) yield return null;
        }

        StopHighlighterPulse();
        StopDialogCameraCoroutine();

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
        CheckCondition();
    }

    private void OnCameraDragged(RightClickEvent e)
    {
        if (!IsCurrentStepCondition(TutorialCondition.CameraMove) || !e.IsStarted) return;

        _currentProgress += 0.1f;
        CheckCondition();
    }

    private void OnPartPlaced(PartPlacedEvent e)
    {
        if (!IsCurrentStepCondition(TutorialCondition.PartPlacement)) return;

        var step = _steps[_currentStep];
        if (!IsPartKeyMatchingGroup(e.PartKey, step.RequiredGroup, step.RequiredPartKeys)) return;

        _currentProgress += 1.0f;
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
}