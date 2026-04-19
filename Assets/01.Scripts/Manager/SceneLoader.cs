using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-99)]
public class SceneLoader : Singleton<SceneLoader>
{
    [Header("Scene Settings")]
    [SerializeField] private string _lobbySceneName = "01.LobbyScene";
    [SerializeField] private string _stageSelectSceneName = "03.StageSelectScene";
    [SerializeField] private string _inGameSceneName = "04.InGameScene";

    public void GoToLobby()
    {
        if (ManagerRegistry.TryGet(out GameManager gameManager))
        {
            gameManager.ChangeState(GameState.Ready);
        }

        SceneManager.LoadScene(_lobbySceneName);
    }

    public void GoToStageSelect()
    {
        SceneManager.LoadScene(_stageSelectSceneName);
    }

    public void EnterInGame()
    {
        SceneManager.LoadScene(_inGameSceneName);
    }

    public void Quit()
    {
        Application.Quit();
    }
}