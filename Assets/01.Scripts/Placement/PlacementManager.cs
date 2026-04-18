
using TMPro;
using UnityEngine;

public class PlacementManager : Singleton<PlacementManager>
{
    private int _currentMouseCount = 15;

    [Header("Mouse Spawn Data")]
    [SerializeField] private TextMeshProUGUI _countDisplay;
    [SerializeField] private GameObject _mousePrefab;
    [SerializeField] private Transform _spawnLocation;
    [SerializeField] private int MaxCount = 500;


    [Header("Mouse Movement Bound Data")]
    [SerializeField] private RectTransform _movementBounds;

    [Header("Mouse Count Data")]
    public int CurrentMouse => _currentMouseCount;

    void Start()
    {
        // It's safer to pre-warm the pool here or in Start
        //PoolManager.Instance.CreatePool(_mousePrefab, _currentMouseCount, MaxCount);

        //SpawnMouseAtPoint(_currentMouseCount);
        UpdateDisplay();
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
        Debug.Log(amount);
        _currentMouseCount += amount;
        SpawnMouseAtPoint(amount);
        UpdateDisplay();
    }

    public void SubtractMouseCount(int amount)
    {
        _currentMouseCount -= amount;

        _currentMouseCount = Mathf.Max(0, _currentMouseCount);

        DespawnMouseAPoint(amount);

        UpdateDisplay();
    }

    public void ResetMouseCount()
    {
        DespawnMouseAPoint(_currentMouseCount);
        _currentMouseCount = 0;
        UpdateDisplay();
    }


    private void SpawnMouseAtPoint(int number)
    {
        for (int i = 0; i < number ;i++)
        {
            GameObject obj = PoolManager.Instance.Spawn(_mousePrefab.name, _spawnLocation.position, Quaternion.identity);

            if (obj.TryGetComponent(out MouseAgent agent))
            {
                agent.Setup(_movementBounds);
            }
        }
    }

    private void DespawnMouseAPoint(int number)
    {
        for (int i = 0; i < number ;i++)
        {
            PoolManager.Instance.Despawn(_mousePrefab);
        }
    }
}
