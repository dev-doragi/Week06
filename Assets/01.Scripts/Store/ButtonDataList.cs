using System.Collections.Generic;
using UnityEngine;

public class ButtonDataList : MonoBehaviour
{
    [SerializeField] List<ShopButton> _dataList;

    public void AddList(ShopButton shopButton)
    {
        _dataList.Add(shopButton);
    }


    public void DisableByCost(int cost)
    {
        EnableAllButton();

        foreach (ShopButton shopbutton in _dataList)
        {
            if (shopbutton._itemData.cost <= cost)
            {
                shopbutton.gameObject.SetActive(false);
            }
        }
    }

    public void OrganizeList()
    {
        _dataList.Sort((a, b) => a._itemData.cost.CompareTo(b._itemData.cost));
    }

    public void ResetList()
    {
        _dataList.Clear();
    }

    private void EnableAllButton()
    {
        
        foreach (ShopButton shopbutton in _dataList)
        {
            shopbutton.gameObject.SetActive(true);        
        }
    }
}
