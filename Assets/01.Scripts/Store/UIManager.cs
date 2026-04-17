
using UnityEngine;


public class UIManager : MonoBehaviour
{
    public static UIManager Instance {get; private set;}


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[UIManager] : Creating UIManager Instance");
            return;
        }
        
        Debug.LogWarning("[UIManager] : Found Multiple UIManager, Destorying Current Instance");
        Destroy(this);
    }

    private void Start()
    {

        RefreshStoreUI();
    }


    private void RefreshStoreUI()
    {
        // Refresh Logic here
    }
}