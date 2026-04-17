using System;
using UnityEngine;

[Serializable]
public class PartCommonStatData
{
    [SerializeField] private float _health;
    [SerializeField][Range(0f, 1f)] private float _defenseRate;
    [SerializeField] private int _cost;

    public float Health => _health;
    public float DefenseRate => _defenseRate;
    public int Cost => _cost;

    public PartCommonStatData(float health, float defenseRate, int cost)
    {
        _health = health;
        _defenseRate = defenseRate;
        _cost = cost;
    }
}