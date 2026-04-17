using UnityEngine;
using UnityEngine.UI;

public class ShopButton : MonoBehaviour
{
    public RatData _ratData;

    private TMPro.TextMeshProUGUI _priceText;
    private Button _myButton;
    private Image _image;

    private bool _isLocked = false; // Temp code

    void Start()
    {
        // The script finds its own neighbors!
        _priceText = GetComponentInChildren<TMPro.TextMeshProUGUI>();
        _myButton = GetComponent<Button>();
        _image = GetComponent<Image>();
        
        RefreshUI();

        _myButton.onClick.AddListener(HandlePurchase);
    }

    public void Setup(RatData ratData)
    {
        _ratData = ratData;
    }

    private void RefreshUI()
    {
        if (_ratData == null) {return;}

        if (_isLocked)
        {
            _priceText.text = "LOCKED";
            _myButton.interactable = false;
            _image.color = Color.black;
        }
        else
        {
            _priceText.text = $"{_ratData.name} : {_ratData.CommonStat.Cost}";
            _myButton.interactable = true;
            _image.color = Color.white;
        }
    }

    private void HandlePurchase()
    {
        Debug.Log("Button clicked! Checking money...");

        if (_ratData == null)
        {
            Debug.LogWarning("[ShopButtons] : Perfab is Missing!");
            return;
        }

        StoreManager.Instance.BuyUnit(_ratData);
    }
}
