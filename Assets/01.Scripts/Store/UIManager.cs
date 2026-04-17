
using UnityEngine;

public enum StoreState
{
    AttackStore = 0,
    BuildStore,
    SupportStore
}

public class UIManager : MonoBehaviour
{
    public static UIManager Instance {get; private set;}

    [Header("Store Pages")]
    [SerializeField] private GameObject _attackStore;
    [SerializeField] private GameObject _buildStore;
    [SerializeField] private GameObject _supportStore;

    [Header("Current Store State")]
    [SerializeField] private StoreState _currentStore;
    
    public Transform _spawnPoint;

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
        _currentStore = StoreState.BuildStore;
        if (_spawnPoint == null)
        {
            Debug.LogWarning("[UIManager]: Spawn Point is Missing! spawning at [0,0]");
        }

        RefreshStoreUI();
    }

    // 버튼에 아래의 function 사용
    public void AttackUnitStoreButton()
    {
        _currentStore = StoreState.AttackStore;
        RefreshStoreUI();
    }

    public void BuildUnitStoreButton() // Renamed from Build for consistency
    {
        _currentStore = StoreState.BuildStore;
        RefreshStoreUI();
    }

    public void SupportUnitStoreButton()
    {
        _currentStore = StoreState.SupportStore;
        RefreshStoreUI();
    }

    private void RefreshStoreUI()
    {
        SwitchStore(_currentStore);
    }

    // 사용중인 것 제외하고 다른 상점창 닫음
    private void SwitchStore(StoreState targetStore)
    {
        // 모든 스토어 일시적으로 disable
        if (_attackStore != null) _attackStore.SetActive(false);
        if (_buildStore != null) _buildStore.SetActive(false);
        if (_supportStore != null) _supportStore.SetActive(false);

        // 사용중인 스토어 페이지 활성화
        switch (targetStore)
        {
            case StoreState.AttackStore:
                if (_attackStore != null) _attackStore.SetActive(true);
                break;
            case StoreState.BuildStore:
                if (_buildStore != null) _buildStore.SetActive(true);
                break;
            case StoreState.SupportStore:
                if (_supportStore != null) _supportStore.SetActive(true);
                break;
        }
    }
}