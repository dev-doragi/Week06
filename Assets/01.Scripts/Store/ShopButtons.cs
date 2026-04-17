using UnityEngine;

public class ShopButtons : MonoBehaviour
{
    [SerializeField] GameObject _perfab;

    private TMPro.TextMeshProUGUI _priceText;
    private UnityEngine.UI.Button _myButton;


    void Start()
    {
        // The script finds its own neighbors!
        _priceText = GetComponentInChildren<TMPro.TextMeshProUGUI>();
        _myButton = GetComponent<UnityEngine.UI.Button>();
        
        // You can even add the listener in code so you don't have to 
        // click the "+" in the Inspector 100 times.
        _myButton.onClick.AddListener(HandlePurchase);
    }

    private void HandlePurchase()
    {
        Debug.Log("Button clicked! Checking money...");

        if (_perfab == null)
        {
            Debug.LogWarning("[ShopButtons] : Perfab is Missing!");
            return;
        }

        Instantiate(_perfab);
    }
}
