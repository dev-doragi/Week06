using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlacementManager : Singleton<PlacementManager>
{
    private int _currentMouseCount = 0;

    // Jaein: 활성화된 마우스들의 실시간 참조 리스트
    private List<GameObject> _activeMice = new List<GameObject>();

    [Header("Mouse Spawn Data")]
    [SerializeField] private TextMeshProUGUI _countDisplay;
    [SerializeField] private GameObject _mousePrefab;
    [SerializeField] private Transform _spawnLocation;
    [SerializeField] private int _mouseCount; // Jaein: 디버깅, 첫 스폰할 마우스 개수

    [Header("Mouse Movement Bound Data")]
    [SerializeField] private RectTransform _movementBounds;

    public int CurrentMouse => _currentMouseCount;

    void Start()
    {
        SpawnMouseAtPoint(_mouseCount);
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
        _currentMouseCount += amount;
        SpawnMouseAtPoint(amount);
        UpdateDisplay();
    }

    public void SubtractMouseCount(int amount)
    {
        _currentMouseCount = Mathf.Max(0, _currentMouseCount - amount);
        DespawnMouseAPoint(amount);
        UpdateDisplay();
    }

    public void ResetMouseCount()
    {
        DespawnMouseAPoint(_activeMice.Count);
        _currentMouseCount = 0;
        UpdateDisplay();
    }

    private void SpawnMouseAtPoint(int number)
    {
        for (int i = 0; i < number; i++)
        {
            GameObject obj = PoolManager.Instance.Spawn(_mousePrefab.name, _spawnLocation.position, Quaternion.identity);

            if (obj != null)
            {
                _activeMice.Add(obj);

                if (obj.TryGetComponent(out MouseAgent agent))
                {
                    agent.Setup(_movementBounds);
                }
            }
        }
    }

    private void DespawnMouseAPoint(int number)
    {
        // Jaein: 리스트에서 실제 인스턴스를 꺼내 PoolManager에 반납
        int count = Mathf.Min(number, _activeMice.Count);

        for (int i = 0; i < count; i++)
        {
            int lastIndex = _activeMice.Count - 1;
            GameObject target = _activeMice[lastIndex];

            // Jaein: 프리팹 원본이 아닌, 리스트에 담긴 실제 객체 참조를 전달
            PoolManager.Instance.Despawn(target);

            _activeMice.RemoveAt(lastIndex);
        }
    }
}