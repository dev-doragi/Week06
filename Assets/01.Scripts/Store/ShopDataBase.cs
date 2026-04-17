using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ShopDatabase", menuName = "Store/Database")]
public class ShopDataBase : ScriptableObject
{
    public List<ShopItemData> attackItems;
    public List<ShopItemData> buildItems;
    public List<ShopItemData> supportItems;
}
