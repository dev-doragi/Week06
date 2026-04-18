using System;
using UnityEngine;

[Serializable]
public class PartPrefabEntry
{
    [SerializeField] private int _key;
    [SerializeField] private GameObject _prefab;

    public int Key => _key;
    public GameObject Prefab => _prefab;

    public bool IsValid()
    {
        return _key > 0 && _prefab != null;
    }
}