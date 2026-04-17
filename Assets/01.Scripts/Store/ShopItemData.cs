using UnityEngine;

[CreateAssetMenu(fileName ="NewShopItem", menuName = "Store/Shop Item")]
public class ShopItemData : ScriptableObject
{
    [Header("UI Display")]
    public string displayName;
    public Sprite icon;
    public ShopItemCategory category;
    
    [Header("Economic Info")]
    public int cost;
    public bool isLocked;

    [Header("The Link")]
    public RatData ratData; // This links the "Product" to the "Warrior"
    public GameObject _prefab;
}
