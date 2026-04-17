
using System.Collections.Generic;
using UnityEngine;

// 스토어 타입
public enum StoreState
{
    AttackStore = 0,
    BuildStore,
    SupportStore
}

public class StoreManager : Singleton<StoreManager>
{
    [Header ("Rat Datas")]
    // 모든 쥐 데이터를 여기에 저장
    [SerializeField] private List<RatData> _ratDataContainer;
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
    [SerializeField] private StoreState _currentStore;


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
        foreach (RatData data in _ratDataContainer)
        {
            // 1. Determine which parent this data belongs to
            Transform targetParent = data.RatType switch
            {
                RatType.Attack => _attackStore.transform,
                RatType.Defence => _buildStore.transform,
                RatType.Wheel => _buildStore.transform,
                RatType.White => _buildStore.transform,
                //RatType.SupportStore => _supportStore,
                _ => _attackStore.transform
            };

            // 2. Spawn and Parent it
            GameObject newBtn = Instantiate(_buttonPrefab, targetParent);
            
            // 3. Inject the data
            if (newBtn.TryGetComponent(out ShopButton script))
            {
                script.Setup(data);
            }
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

    // 사용중인 것 제외하고 다른 상점창 닫음
    private void SwitchStore(StoreState targetStore)
    {
        if (_attackStore != null)
        _attackStore.SetActive(targetStore == StoreState.AttackStore);

        if (_buildStore != null)
        _buildStore.SetActive(targetStore == StoreState.BuildStore);

        if (_supportStore != null)
        _supportStore.SetActive(targetStore == StoreState.SupportStore);
    }
    #endregion

    private void RefreshStoreUI()
    {
        SwitchStore(_currentStore);
    }
}
