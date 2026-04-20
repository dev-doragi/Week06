using UnityEngine;

public enum ItemTier
{
    tier1 = 0,
    tier2,
    tier3,
    tier4
}


[CreateAssetMenu(fileName ="NewShopItem", menuName = "Store/Shop Item")]
public class ShopItemData : ScriptableObject
{
    [Header("UI Display")]
    public string displayName;
    public int partKey;
    public ShopItemCategory category;
    [TextArea]
    public string description;
    

    [Header("Economic Info")]
    public int cost;
    public ItemTier tier;
    public bool isLocked;
}
