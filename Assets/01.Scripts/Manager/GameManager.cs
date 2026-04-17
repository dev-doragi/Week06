using System;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public void GoToLobby()
    {
        ChangeState(GameState.Ready);
        SceneManager.LoadScene("01.LobbyScene");
    }

    public void GoToStageSelect()
    {
        SceneManager.LoadScene("03.StageSelectScene");
    }

    public void EnterInGame()
    {
        SceneManager.LoadScene("04.InGameScene");
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