using System;
using UnityEngine;

public class ShopButtons : MonoBehaviour
{
    public RatData _ratData;

    private TMPro.TextMeshProUGUI _priceText;
    private UnityEngine.UI.Button _myButton;


    void Start()
    {
        // The script finds its own neighbors!
        _priceText = GetComponentInChildren<TMPro.TextMeshProUGUI>();
        _myButton = GetComponent<UnityEngine.UI.Button>();
        
        if (_ratData != null)
        {
            // 2. Setting the text correctly
            // Use the .text property, and convert numbers to strings with .ToString()
            _priceText.text = $"{_ratData.name} : {_ratData.CommonStat.Cost}";
        }

        _myButton.onClick.AddListener(HandlePurchase);
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
