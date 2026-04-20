
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;
public class ShopButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{    
    public RunShopItemData _itemData;
    [SerializeField] private TMPro.TextMeshProUGUI _priceText;
    [SerializeField] private Button _myButton;
    [SerializeField] private Image _shadeimage;
    [SerializeField] private Image _mouseIcon;
    private bool _isLocked = false; // Temp code

    private void OnEnable()
    {
        EventBus.Instance.Subscribe<PartPlacedEvent>(OnPartPlaced);
    }

    private void OnDisable()
    {
        EventBus.Instance.Unsubscribe<PartPlacedEvent>(OnPartPlaced);
    }

    void Start()
    {
        RefreshUI();

        _myButton.onClick.AddListener(HandlePurchase);
       //mouseIcon.SourceImage;
    }

    public void Setup(ShopItemData itemData , Sprite Icon)
    {
        
        _itemData = new RunShopItemData(itemData);
        
        if (Icon != null)
        {
            _mouseIcon.sprite = Icon;
        }

        RefreshUI();
        
        // Setup Button Listener if not already set in Inspector
        _myButton.onClick.RemoveAllListeners();
        _myButton.onClick.AddListener(HandlePurchase);
    }

    private void RefreshUI()
    {
        if (_itemData == null) {return;}

        if (_isLocked)
        {
            _priceText.text = "LOCKED";
            _myButton.interactable = false;
        }
        
        else
        {
            _priceText.text = $"{_itemData.displayName} : {_itemData.cost}";
            _myButton.interactable = true;
        }
    }

    private void HandlePurchase()
    {
        if (_itemData == null)
        {
            Debug.LogWarning("[ShopButtons] : Data is Missing!");
            return;
        }

        StoreManager.Instance.SelectUnit(_itemData);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        string name, description, status = "";
       
        if (!GridManager.instance.partDic.TryGetValue(_itemData.partKey, out PartData _partData))
            return;

        name = $"{_partData.PartName}";

        description = $"{_partData.Description}";
        if(_partData.UnitRoleType == UnitRoleType.None || _partData.UnitRoleType == UnitRoleType.Support)
        {

            status = $"건물 체력: {_partData.Hp}\n";
            if (_partData.SupportStat != null)
            {
                foreach (var sup in _partData.SupportStat.Effects)
                {

                    status += sup.Description + "\n";
                }
            }
                

            
        }
        else if (_partData.UnitRoleType == UnitRoleType.Attack)
        {
            status = $"건물 체력: {_partData.Hp}\n공격력: {_partData.AttackDamage}\n공격 속도: {_partData.AttackSpeed}";
        }
        else if (_partData.UnitRoleType == UnitRoleType.Defense)
        {
            status = $"건물 체력: {_partData.Hp}\n방어력: {_partData.DefenseRate * 100}\n충돌 데미지: {_partData.CollisionPower}\n";
        }

        StoreManager.Instance.Hover(true, name, description, status);

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StoreManager.Instance.Hover(false);
    }

    private void OnPartPlaced(PartPlacedEvent e)
    {
        if(e.PartKey == 10001 && e.PartKey == _itemData.partKey)
        {
            _itemData.cost += 90;
            RefreshUI();
        }

    }
}


public class RunShopItemData
{
    public string displayName;
    public int partKey;
    public ShopItemCategory category;
    public string description;
    public int cost;
    public ItemTier tier;
    public bool isLocked;
    public RunShopItemData(ShopItemData dataSO)
    {
        displayName = dataSO.displayName;
        partKey = dataSO.partKey;
        category = dataSO.category;
        description = dataSO.description;
        cost = dataSO.cost;
        tier = dataSO.tier;
        isLocked = dataSO.isLocked;
    }
}