using UnityEngine;
using UnityEngine.UI;

public class ShopButton : MonoBehaviour
{    
    public ShopItemData _itemData;
    [SerializeField] private TMPro.TextMeshProUGUI _priceText;
    [SerializeField] private Button _myButton;
    [SerializeField] private Image _shadeimage;
    [SerializeField] private Image _mouseIcon;

    private bool _isLocked = false; // Temp code

    void Start()
    {
        RefreshUI();

        _myButton.onClick.AddListener(HandlePurchase);
       //mouseIcon.SourceImage;
    }

    public void Setup(ShopItemData itemData , Sprite Icon)
    {
        
        _itemData = itemData;
        
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
        Debug.Log("Button clicked! Checking money...");

        if (_itemData == null)
        {
            Debug.LogWarning("[ShopButtons] : Data is Missing!");
            return;
        }

        StoreManager.Instance.SelectUnit(_itemData);
    }
}
