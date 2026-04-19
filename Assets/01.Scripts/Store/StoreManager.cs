using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


// 스토어 타입
public enum ShopItemCategory
{
    AttackStore = 0,
    DefenseStore,
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


    [Header("Tab Button Colors")]
    [SerializeField] private Button _attackTabButton;
    [SerializeField] private Button _defenceTabButton;
    [SerializeField] private Button _buildTabButton;
    [SerializeField] private Button _supportTabButton;

    [SerializeField] private Color _selectedColor = Color.white;
    [SerializeField] private Color _deselectedColor = new Color(0.6f, 0.6f, 0.6f, 1f);


    // 구매후 나오는 쥐 위치
    [Header("Spawn Settings")]
    [SerializeField] private Transform _spawnPoint;

    // 현제 열린 스토어 상태
    [Header("Current Store State")]
    [SerializeField] private ShopItemCategory _currentStore = ShopItemCategory.AttackStore;

    [Header("Debug Check")]
    [SerializeField] bool _showDebug = true;



    protected override void Init()
    {

        if (_showDebug) Debug.Log("[StoreManager] : Store System Initialized via Singleton Base");
        // Set default page
        RefreshStoreUI();
    }

    void Start()
    {
        GenerateBuyButtons();
    }


    #region 구매 버튼
    private void GenerateBuyButtons()
    {   
        if (_showDebug) Debug.Log("[StoreManager]: Generating Button");

        ProcessStoreItems(_database.attackItems, _attackStore.transform);
        ProcessStoreItems(_database.defenseItems, _defenceStore.transform);
        ProcessStoreItems(_database.buildItems, _buildStore.transform);
        ProcessStoreItems(_database.supportItems, _supportStore.transform);
    }

    private void ProcessStoreItems(IEnumerable<ShopItemData> items, Transform storeParent)
    {
        ButtonDataList buttonDataList = storeParent.GetComponentInChildren<ButtonDataList>();

        buttonDataList.ResetList();

        foreach (ShopItemData item in items)
        {
            if (item == null) continue;

            ShopButton button = CreateButton(item, storeParent);
            if (button != null) buttonDataList.AddList(button);
        }

        buttonDataList.OrganizeList();
    }

    private ShopButton CreateButton(ShopItemData data, Transform targetPage)
    {
        GameObject newButton = Instantiate(_buttonPrefab, targetPage);

        if (!newButton.TryGetComponent(out ShopButton script))
        {
            Debug.LogWarning("[StoreManager] No ShopButton component found on prefab!");
            return null;
        }

        script.Setup(data, TryGetIconSprite(data.partKey));
        return script;
    }

    private Sprite TryGetIconSprite(int partKey)
    {
        if (_gridManager?.partDic == null)
        {
            Debug.LogWarning("[StoreManager]: partDic is missing");
            return null;
        } 

        if (!_gridManager.partDic.TryGetValue(partKey, out PartData partInfo))
        {
            Debug.LogWarning($"[StoreManager] Key {partKey} not found in GridManager dictionary!");
            return null;
        }

        return partInfo.Icon;
    }

    // 구매 버튼 클리시 해당 코드 실행
    public void SelectUnit(ShopItemData data)
    {
        PlacementManager.Instance.AddMouseCount(data.cost);

        if (_showDebug) Debug.Log("[StoreManager]: Selected part key: " + data.partKey);
        BuildManager.Instance.SelectPart(data.partKey);
    }
    #endregion


    private void UpdateTabColors(ShopItemCategory selected)
    {
        UpdateSingleTab(_attackTabButton,  selected == ShopItemCategory.AttackStore);
        UpdateSingleTab(_defenceTabButton, selected == ShopItemCategory.DefenseStore);
        UpdateSingleTab(_buildTabButton,   selected == ShopItemCategory.BuildStore);
        UpdateSingleTab(_supportTabButton, selected == ShopItemCategory.SupportStore);
    }

    private void UpdateSingleTab(Button button, bool isSelected)
    {
        if (button == null) return;

        // Set the base color
        ColorBlock colors = button.colors;
        colors.normalColor      = isSelected ? _selectedColor : _deselectedColor;
        colors.selectedColor    = isSelected ? _selectedColor : _deselectedColor; // prevents reset on click-away
        colors.highlightedColor = Color.Lerp(colors.normalColor, Color.white, 0.2f); // slight hover brighten
        button.colors = colors;
    }


    #region 구매 스토어 창 버튼
    // 구메창 각 상태에 따라 불러올때 사용
    public void AttackUnitStoreButton()
    {
        _currentStore = ShopItemCategory.AttackStore;
        RefreshStoreUI();
    }

    public void DefenseUnitStoreButton()
    {
        _currentStore = ShopItemCategory.DefenseStore;
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

    private void RefreshStoreUI()
    {
        SwitchStore(_currentStore);
        UpdateTabColors(_currentStore);
    }


    // 사용중인 것 제외하고 다른 상점창 닫음
    private void SwitchStore(ShopItemCategory targetStore)
    {
        RectTransform newContent = null;
        if (_showDebug) Debug.Log("[StoreManager]: Current Store " + targetStore);

        // 불려진 스토어 제외 모두 비활성화
        if (_attackStore != null)
        {
            bool isActive = (targetStore == ShopItemCategory.AttackStore);
            _attackStore.SetActive(isActive);
            if (isActive) newContent = _attackStore.GetComponent<RectTransform>();
        }

        if (_defenceStore != null)
        {
            bool isActive = (targetStore == ShopItemCategory.DefenseStore);
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
        

        // 스크롤 상태 강제로 1 로 리셋
        if (newContent != null && _scrollRect != null)
        {
            _scrollRect.content = newContent;
            _scollbar.value = 1f;
            Canvas.ForceUpdateCanvases();
        }
    }
    #endregion
}

