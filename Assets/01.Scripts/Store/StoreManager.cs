
using UnityEngine;
using UnityEngine.UI;

// 스토어 타입
public enum ShopItemCategory
{
    AttackStore = 0,
    DefnseStore,
    BuildStore,
    SupportStore
}

public class StoreManager : Singleton<StoreManager>
{
    
    [Header ("Rat Datas")]
    // 모든 쥐 데이터를 여기에 저장
    [SerializeField] private ShopDataBase _database;
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private GameObject _buttonPrefab;

    // 각 타입에 따른 스토어
    [Header("Store UI Pages")]
    [SerializeField] private GameObject _attackStore;
    [SerializeField] private GameObject _defenceStore;
    [SerializeField] private GameObject _buildStore;
    [SerializeField] private GameObject _supportStore;
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private Scrollbar _scollbar;


    // 구매후 나오는 쥐 위치
    [Header("Spawn Settings")]
    [SerializeField] private Transform _spawnPoint;

    // 현제 열린 스토어 상태
    [Header("Current Store State")]
    [SerializeField] private ShopItemCategory _currentStore;



    protected override void Init()
    {
        Debug.Log("Store System Initialized via Singleton Base");
        // Set default page
        AttackUnitStoreButton(); 

        GenerateBuyButtons();
    }



    #region 구매 버튼
    private void GenerateBuyButtons()
    {
        // For each list in the database, spawn buttons in the correct store page
        foreach (ShopItemData item in _database.attackItems) 
            if (item != null) CreateButton(item, _attackStore.transform);

        foreach (ShopItemData item in _database.DefenseItems) 
            if (item != null) CreateButton(item, _defenceStore.transform);

        foreach (ShopItemData item in _database.buildItems) 
            if (item != null) CreateButton(item, _buildStore.transform);

        foreach (ShopItemData item in _database.supportItems) 
            if (item != null) CreateButton(item, _supportStore.transform);
    }

    private void CreateButton(ShopItemData data, Transform targetPage)
    {
        // 1. Spawn the UI Button Prefab as a child of the specific Store Page
        GameObject newButton = Instantiate(_buttonPrefab, targetPage);

        // 2. Pass the ShopItemData into the button's script
        if (newButton.TryGetComponent(out ShopButton script))
        {
            Sprite iconSprite = null;

            // CRITICAL FIX: Use 'partKey', not 'cost', to find the data in the dictionary
            if (_gridManager != null && _gridManager.partDic != null)
            {
                if (_gridManager.partDic.TryGetValue(data.partKey, out PartData partInfo))
                {
                    iconSprite = partInfo.Icon;
                }
                else
                {
                    Debug.LogWarning($"[StoreManager] Key {data.partKey} not found in GridManager dictionary!");
                }
            }

            script.Setup(data, iconSprite); 
        }
    }

    // 구매 버튼 클리시 해당 코드 실행
    public void SelectUnit(ShopItemData data)
    {
        Debug.Log(data.partKey);
        BuildManager.Instance.SelectPart(data.partKey);
    }
    #endregion




    #region 구매 스토어 창 버튼
    // 구메창 각 상태에 따라 불러올때 사용
    public void AttackUnitStoreButton()
    {
        _currentStore = ShopItemCategory.AttackStore;
        RefreshStoreUI();
    }

    public void DefenseUnitStoreButton()
    {
        _currentStore = ShopItemCategory.DefnseStore;
        RefreshStoreUI();
    }

    public void BuildUnitStoreButton() // Renamed from Build for consistency
    {
        _currentStore = ShopItemCategory.BuildStore;
        RefreshStoreUI();
    }

    public void SupportUnitStoreButton()
    {
        _currentStore = ShopItemCategory.SupportStore;
        RefreshStoreUI();
    }




    // 사용중인 것 제외하고 다른 상점창 닫음
    private void SwitchStore(ShopItemCategory targetStore)
    {
        // 1. Identify which transform is the new content
        RectTransform newContent = null;

        if (_attackStore != null)
        {
            bool isActive = (targetStore == ShopItemCategory.AttackStore);
            _attackStore.SetActive(isActive);
            if (isActive) newContent = _attackStore.GetComponent<RectTransform>();
        }

        if (_defenceStore != null)
        {
            bool isActive = (targetStore == ShopItemCategory.DefnseStore);
            _defenceStore.SetActive(isActive);
            if (isActive) newContent = _defenceStore.GetComponent<RectTransform>();
        }

        if (_buildStore != null)
        {
            bool isActive = (targetStore == ShopItemCategory.BuildStore);
            _buildStore.SetActive(isActive);
            if (isActive) newContent = _buildStore.GetComponent<RectTransform>();
        }

        if (_supportStore != null)
        {
            bool isActive = (targetStore == ShopItemCategory.SupportStore);
            _supportStore.SetActive(isActive);
            if (isActive) newContent = _supportStore.GetComponent<RectTransform>();
        }

        // 2. Assign the new content to the ScrollRect
        if (newContent != null && _scrollRect != null)
        {
            _scrollRect.content = newContent;

            // 3. Reset the position to the TOP (1 is top, 0 is bottom)
            _scollbar.value = 1f;
            
            // 4. Force a UI layout update so it doesn't "jump" on the next frame
            Canvas.ForceUpdateCanvases();
        }
    }
    #endregion

    private void RefreshStoreUI()
    {
        SwitchStore(_currentStore);
    }
}
