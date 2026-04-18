using System;
using UnityEngine;

public enum GameState
{
    Ready,
    Playing,
    Paused,
    GameOver,
    GameClear
}

public struct GameStateChangedEvent
{
    public GameState NewState;
}

public class GameManager : Singleton<GameManager>
{
    public GameState CurrentState { get; private set; } = GameState.Ready;

    public static event Action<GameState> OnGameStateChanged;

    protected override void Init()
    {
        Application.targetFrameRate = 60;
    }

    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState) return;

        CurrentState = newState;

        Time.timeScale = (CurrentState == GameState.Paused) ? 0f : 1f;

        OnGameStateChanged?.Invoke(CurrentState);

        EventBus.Instance.Publish(new GameStateChangedEvent { NewState = CurrentState });
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}