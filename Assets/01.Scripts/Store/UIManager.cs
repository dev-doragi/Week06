using UnityEngine;

[DefaultExecutionOrder(-100)]
public class UIManager : Singleton<UIManager>
{
    [Header("Main UI Panels")]
    [SerializeField] private GameObject _inGamePanel;
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private GameObject _gameClearPanel;
    [SerializeField] private GameObject _pausePanel;

    [Header("Clear Panel Elements")]
    [SerializeField] private GameObject _gameClearText;
    [SerializeField] private GameObject _stageClearText;
    [SerializeField] private GameObject _resumeButton;

    private void OnEnable()
    {
        if (EventBus.Instance != null)
        {
            EventBus.Instance.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
            EventBus.Instance.Subscribe<InGameStateChangedEvent>(OnInGameStateChanged);
        }
    }

    private void OnDisable()
    {
        if (EventBus.Instance != null)
        {
            EventBus.Instance.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
            EventBus.Instance.Unsubscribe<InGameStateChangedEvent>(OnInGameStateChanged);
        }
    }

    // 전체 게임 상태 변화에 따른 UI
    private void OnGameStateChanged(GameStateChangedEvent evt)
    {
        switch (evt.NewState)
        {
            case GameState.Playing:
                HideAllPanels();
                if (_inGamePanel != null) _inGamePanel.SetActive(true);
                break;
            case GameState.Paused:
                if (_pausePanel != null) _pausePanel.SetActive(true);
                break;
            case GameState.GameOver:
                if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
                break;
            case GameState.GameClear:
                ShowClearPanel(isAllGameClear: true);
                break;
            case GameState.Ready:
                HideAllPanels();
                break;
        }
    }

    // 인 게임 상태 변화에 따른 UI
    private void OnInGameStateChanged(InGameStateChangedEvent evt)
    {
        if (evt.NewState == InGameState.StageCleared)
        {
            if (GameManager.Instance.CurrentState != GameState.GameClear)
            {
                ShowClearPanel(isAllGameClear: false);
            }
        }
    }

    private void ShowClearPanel(bool isAllGameClear)
    {
        if (_gameClearPanel == null) return;
        _gameClearPanel.SetActive(true);

        if (isAllGameClear)
        {
            if (_gameClearText != null) _gameClearText.SetActive(true);
            if (_stageClearText != null) _stageClearText.SetActive(false);
            if (_resumeButton != null) _resumeButton.SetActive(false);
        }
        else
        {
            if (_gameClearText != null) _gameClearText.SetActive(false);
            if (_stageClearText != null) _stageClearText.SetActive(true);
            if (_resumeButton != null) _resumeButton.SetActive(true);
        }
    }

    public void HideAllPanels()
    {
        if (_inGamePanel != null) _inGamePanel.SetActive(false);
        if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
        if (_gameClearPanel != null) _gameClearPanel.SetActive(false);
        if (_pausePanel != null) _pausePanel.SetActive(false);
    }

    // ----------------------------------------------------------------
    // 버튼 OnClick에 바로 연결할 수 있는 공개 이벤트 핸들러들
    // 인스펙터에서 각 버튼의 OnClick에 이 메서드들을 연결하세요.
    // ----------------------------------------------------------------

    public void OnPauseClicked()
    {
        PauseManager.Instance.TogglePause(true);
    }

    public void OnResumeClicked()
    {
        PauseManager.Instance.TogglePause(false);
    }

    public void OnGoToLobbyClicked()
    {
        SceneLoader.Instance.GoToLobby();
    }

    public void OnRetryClicked()
    {
        VehicleCache.Clear();
        SceneLoader.Instance.ReloadCurrentScene();
    }

    public void OnNextStageClicked()
    {
        // 현재 차량 상태를 캐싱한 뒤 다음 스테이지 로드
        var saver = FindAnyObjectByType<VehicleSaveLoader>();
        if (saver != null)
        {
            saver.SaveCurrentVehicle(StageManager.Instance.CurrentStageIndex);
        }

        HideAllPanels();
        if (_inGamePanel != null) _inGamePanel.SetActive(true);
        StageManager.Instance.LoadNextStage();
    }

    public void OnGoToStageSelectClicked()
    {
        HideAllPanels();
        SceneLoader.Instance.GoToStageSelect();
    }
}