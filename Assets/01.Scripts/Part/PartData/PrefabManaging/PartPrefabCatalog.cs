using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "PartPrefabCatalog",
    menuName = "Game Data/Part Prefab Catalog")]
public class PartPrefabCatalog : ScriptableObject
{
    [SerializeField] private List<PartPrefabEntry> _entries = new();

    private Dictionary<int, GameObject> _prefabByKey;

    public IReadOnlyList<PartPrefabEntry> Entries => _entries;

    public void Initialize()
    {
        if (_prefabByKey != null)
        {
            return;
        }

        _prefabByKey = new Dictionary<int, GameObject>();

        for (int i = 0; i < _entries.Count; i++)
        {
            PartPrefabEntry entry = _entries[i];
            if (entry == null)
            {
                Debug.LogWarning($"{name}: PartPrefabEntry[{i}]가 null입니다.");
                continue;
            }

            if (!entry.IsValid())
            {
                Debug.LogWarning($"{name}: 유효하지 않은 엔트리입니다. index={i}");
                continue;
            }

            if (_prefabByKey.ContainsKey(entry.Key))
            {
                Debug.LogWarning($"{name}: 중복 key가 있습니다. key={entry.Key}, 마지막 값으로 덮어씁니다.");
            }

            _prefabByKey[entry.Key] = entry.Prefab;
        }
    }

    public bool TryGetPrefab(int key, out GameObject prefab)
    {
        Initialize();
        return _prefabByKey.TryGetValue(key, out prefab);
    }

    public GameObject GetPrefabOrNull(int key)
    {
        Initialize();

        if (_prefabByKey.TryGetValue(key, out GameObject prefab))
        {
            return prefab;
        }

        return null;
    }

    public bool ContainsKey(int key)
    {
        Initialize();
        return _prefabByKey.ContainsKey(key);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        _prefabByKey = null;
    }
#endif
}