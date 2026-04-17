using System;
using UnityEngine;

[Serializable]
public class RatCommonStatData
{
    [SerializeField] private float _hp;
    [SerializeField][Range(0f, 1f)] private float _defenceRate;
    [SerializeField] private int _cost;

    public float Hp => _hp;
    public float DefenceRate => _defenceRate;
    public int Cost => _cost;
}