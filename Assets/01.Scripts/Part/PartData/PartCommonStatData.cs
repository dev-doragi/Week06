using System;
using UnityEngine;

[Serializable]
public class PartCommonStatData
{
    [SerializeField] private float _hp;
    [SerializeField] [Range(0f, 1f)] private float _defenseRate;
    [SerializeField] private int _cost;

    public float Hp => _hp;
    public float DefenseRate => _defenseRate;
    public int Cost => _cost;

    public PartCommonStatData(float hp, float defenseRate, int cost)
    {
        _hp = hp;
        _defenseRate = defenseRate;
        _cost = cost;
    }
}