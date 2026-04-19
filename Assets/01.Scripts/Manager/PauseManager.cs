using UnityEngine;

public class PauseManager : Singleton<PauseManager>
{
    private bool _isPaused = false;

    private void OnEnable()
    {
        if (EventBus.Instance != null)
        {
            EventBus.Instance.Subscribe<PausePressedEvent>(OnPausePressed);
            EventBus.Instance.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
        }
    }

    private void OnDisable()
    {
        if (EventBus.Instance != null)
        {
            EventBus.Instance.Unsubscribe<PausePressedEvent>(OnPausePressed);
            EventBus.Instance.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
        }
    }

    private void OnPausePressed(PausePressedEvent evt)
    {
        GameState currentState = GameManager.Instance.CurrentState;

        if (currentState == GameState.Playing)
        {
            TogglePause(true);
        }
        else if (currentState == GameState.Paused)
        {
            TogglePause(false);
        }
    }

    private void OnGameStateChanged(GameStateChangedEvent evt)
    {
        if (evt.NewState != GameState.Paused)
        {
            _isPaused = false;
        }
    }

    public void TogglePause(bool pause)
    {
        _isPaused = pause;
        GameManager.Instance.ChangeState(_isPaused ? GameState.Paused : GameState.Playing);

        // TODO : 이벤트 처리
        EventBus.Instance.Publish(new PausePressedEvent
        {
        });
    }
}