using UnityEngine;

[DefaultExecutionOrder(-100)]
public class UIManager : Singleton<UIManager>
{
    [Header("Main UI Panels")]
    [SerializeField] private GameObject _inGamePanel;
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private GameObject _gameClearPanel;
    [SerializeField] private GameObject _pausePanel;

    protected override void Init()
    {

    }

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

    private void OnGameStateChanged(GameStateChangedEvent evt)
    {
        switch (evt.NewState)
        {
            case GameState.Playing:
                if (_pausePanel != null) _pausePanel.SetActive(false);
                if (_inGamePanel != null) _inGamePanel.SetActive(true);
                break;

            case GameState.Paused:
                if (_pausePanel != null) _pausePanel.SetActive(true);
                break;

            case GameState.GameOver:
                if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
                break;

            case GameState.GameClear:
                if (_gameClearPanel != null) _gameClearPanel.SetActive(true);
                break;

            case GameState.Ready:
                HideAllPanels();
                break;
        }
    }

    // 만약 인게임 상태에 따라 UI 변화가 필요하다면
    private void OnInGameStateChanged(InGameStateChangedEvent evt)
    {
        switch (evt.NewState)
        {
            case InGameState.None:
                break;

            case InGameState.StageFailed:
                break;

            case InGameState.StageCleared:
                break;
        }
    }

    public void HideAllPanels()
    {
        if (_inGamePanel != null) _inGamePanel.SetActive(false);
        if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
        if (_gameClearPanel != null) _gameClearPanel.SetActive(false);
        if (_pausePanel != null) _pausePanel.SetActive(false);
    }
}