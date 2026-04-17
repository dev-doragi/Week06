
using System.Collections.Generic;
using UnityEngine;

// 스토어 타입
public enum ShopItemCategory
{
    AttackStore = 0,
    BuildStore,
    SupportStore
}

public class StoreManager : Singleton<StoreManager>
{
    [Header ("Rat Datas")]
    // 모든 쥐 데이터를 여기에 저장
    [SerializeField] private ShopDataBase _database;
    [SerializeField] private GameObject _buttonPrefab;

    // 각 타입에 따른 스토어
    [Header("Store UI Pages")]
    [SerializeField] private GameObject _attackStore;
    [SerializeField] private GameObject _buildStore;
    [SerializeField] private GameObject _supportStore;


    // 구매후 나오는 쥐 위치
    [Header("Spawn Settings")]
    [SerializeField] private Transform _spawnPoint;

    // 현제 열린 스토어 상태
    [Header("Current Store State")]
    [SerializeField] private ShopItemCategory _currentStore;


    [Header("Temp Reference")]
    [SerializeField] private GameObject _tempObject;


    // 임시 코스트, 아마 게임 매니저가 가질듯
    private int tempCost = 15;

    public int TempCost => tempCost;

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
            CreateButton(item, _attackStore.transform);

        foreach (ShopItemData item in _database.buildItems) 
            CreateButton(item, _buildStore.transform);
    }

    private void CreateButton(ShopItemData data, Transform targetPage)
    {
        // 1. Spawn the UI Button Prefab as a child of the specific Store Page
        GameObject newButton = Instantiate(_buttonPrefab, targetPage);
        
        // 2. Pass the ShopItemData into the button's script
        if (newButton.TryGetComponent(out ShopButton script))
        {
            script.Setup(data.ratData); // This fills the name, price, and icon
        }
    }

    // 구매 버튼 클리시 해당 코드 실행
    public void BuyUnit(RatData data)
    {
        // Spawning logic...
        if (tempCost < data.CommonStat.Cost)
        {
            Debug.Log("[Store Manager] : Not Enough of mouses left!");
            return;
        }
        
        // 현재는 임시 데이터 사용중, 후에 데이터 또는 리스트에서 불러와질 예정
        Instantiate(_tempObject, _spawnPoint.position, Quaternion.identity, _spawnPoint.transform);
    }
    #endregion




    #region 구매 스토어 창 버튼
    // 구메창 각 상태에 따라 불러올때 사용
    public void AttackUnitStoreButton()
    {
        _currentStore = ShopItemCategory.AttackStore;
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
        if (_attackStore != null)
        _attackStore.SetActive(targetStore == ShopItemCategory.AttackStore);

        if (_buildStore != null)
        _buildStore.SetActive(targetStore == ShopItemCategory.BuildStore);

        if (_supportStore != null)
        _supportStore.SetActive(targetStore == ShopItemCategory.SupportStore);
    }
    #endregion

    private void RefreshStoreUI()
    {
        SwitchStore(_currentStore);
    }
}
