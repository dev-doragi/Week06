using UnityEngine;


[CreateAssetMenu(fileName ="NewShopItem", menuName = "Store/Shop Item")]
public class ShopItemData : ScriptableObject
{
    [Header("UI Display")]
    public string displayName;
    public int partKey;
    public ShopItemCategory category;
    

    [Header("Economic Info")]
    public int cost;
    public bool isLocked;
}
