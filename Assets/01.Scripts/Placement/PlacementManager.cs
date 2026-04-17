
using TMPro;
using UnityEngine;

public class PlacementManager : Singleton<PlacementManager>
{
    private int _currentMouseCount = 0;

    [SerializeField] private TextMeshProUGUI _countDisplay;
    [SerializeField] private GameObject _mousePrefab;
    [SerializeField] private Transform _spawnLocation;

    public int CurrentMouse => _currentMouseCount;

    protected override void Init()
    {
        // It's safer to pre-warm the pool here or in Start
        PoolManager.Instance.CreatePool(_mousePrefab, 20, 500);
    }

    private void UpdateDisplay()
    {
        if (_countDisplay != null)
        {
            _countDisplay.text = _currentMouseCount.ToString();
        }
    }

    public void AddMouseCount(int amount)
    {
        _currentMouseCount += amount;
        UpdateDisplay();
    }

    public void SubtractMouseCount(int amount)
    {
        _currentMouseCount -= amount;
        
        // FIX: Use Max to ensure the number never goes below 0
        _currentMouseCount = Mathf.Max(0, _currentMouseCount);
        

        
        UpdateDisplay();
    }

    public void SpawnMouseAtPoint(Vector3 position)
    {
        PoolManager.Instance.Spawn(_mousePrefab.name, position, Quaternion.identity);
    }
}
