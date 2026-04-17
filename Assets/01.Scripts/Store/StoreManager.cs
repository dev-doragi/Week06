
using System;
using System.Collections.Generic;
using UnityEngine;

public enum StoreState
{
    AttackStore = 0,
    BuildStore,
    SupportStore
}

public class StoreManager : Singleton<StoreManager>
{
    [Header ("Rat Datas")]
    [SerializeField] private List<RatData> _ratDataContainer;

    [Header("Store UI Pages")]
    [SerializeField] private GameObject _attackStore;
    [SerializeField] private GameObject _buildStore;
    [SerializeField] private GameObject _supportStore;

    [Header("Spawn Settings")]
    [SerializeField] private Transform _spawnPoint;

    [Header("Current Store State")]
    [SerializeField] private StoreState _currentStore;

    [SerializeField] private GameObject _tempObject;


    // 임시 코스트, 아마 게임 매니저가 가질듯
    private int tempCost = 15;

    public int TempCost => tempCost;

    // Use Init instead of Awake
    protected override void Init()
    {
        Debug.Log("Store System Initialized via Singleton Base");
        // Set default page
        AttackUnitStoreButton(); 
    }

    public void BuyUnit(RatData data)
    {
        // Spawning logic...
        if (tempCost < data.CommonStat.Cost)
        {
            Debug.Log("[Store Manager] : Not Enough of mouses left!");
            return;
        }
        
        // 현재는 임시 데이터 사용중, 후에 데이터 또는 리스트에서 불러와질 예정
        Instantiate(_tempObject, _spawnPoint.position, Quaternion.identity);
    }

    // 구메창 각 상태에 따라 불러올때 사용
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
