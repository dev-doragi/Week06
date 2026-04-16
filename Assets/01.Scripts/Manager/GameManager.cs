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

public class GameManager : Singleton<GameManager>
{
    public GameState CurrentState { get; private set; } = GameState.Ready;

    public static event Action<GameState> OnGameStateChanged;

    protected override void Init()
    {
        // 초기화 로직
        Application.targetFrameRate = 60;
    }

    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState) return;

        CurrentState = newState;
        Time.timeScale = (CurrentState == GameState.Paused) ? 0f : 1f;

        OnGameStateChanged?.Invoke(CurrentState);
    }

    public void StartGame()
    {
        ChangeState(GameState.Playing);
        //StageManager.Instance?.StartCurrentStage();
    }

    public void TogglePause()
    {
        if (CurrentState == GameState.Playing)
            ChangeState(GameState.Paused);
        else if (CurrentState == GameState.Paused)
            ChangeState(GameState.Playing);
    }

    public void GameOver()
    {
        if (CurrentState == GameState.GameOver || CurrentState == GameState.GameClear) return;

        ChangeState(GameState.GameOver);

        // 사운드나 UI 처리는 여기서 직접 하지 않고, 
        // UIManager나 SoundManager가 OnGameStateChanged 이벤트를 듣고 알아서 처리하도록 둡니다.
    }

    public void GameClear()
    {
        if (CurrentState == GameState.GameOver || CurrentState == GameState.GameClear) return;

        ChangeState(GameState.GameClear);
    }

    public void RestartGame()
    {
        ChangeState(GameState.Ready);
        SceneManager.LoadScene("01.MainScene"); // 추후 타이틀 화면으로 변경
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