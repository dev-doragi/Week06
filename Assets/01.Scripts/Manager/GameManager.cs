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

public class GameManager : Singleton<GameManager>
{
    public GameState CurrentState { get; private set; } = GameState.Ready;

    protected override void Init()
    {
        Application.targetFrameRate = 60;
    }

    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState) return;

        CurrentState = newState;

        Time.timeScale = (CurrentState == GameState.Paused) ? 0f : 1f;

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