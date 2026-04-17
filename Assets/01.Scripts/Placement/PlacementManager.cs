
using UnityEngine;

public class PlacementManager : Singleton<PlacementManager>
{
    private int _currentMouseCount = 0;

    public int CurrentMouse => _currentMouseCount;

    public void AddMouseCount(int amount)
    {
        _currentMouseCount += amount;
    }

    public void SubtractMouseCount(int amount)
    {
        _currentMouseCount -= amount;
        _currentMouseCount = (int)Mathf.Min(0, _currentMouseCount);
    }

    
}
