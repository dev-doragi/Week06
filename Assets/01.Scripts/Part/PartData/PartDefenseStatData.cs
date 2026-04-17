using System;
using UnityEngine;

[Serializable]
public class PartDefenseStatData
{
    [SerializeField] private float _collisionPower;

    public float CollisionPower => _collisionPower;

    public PartDefenseStatData(float collisionPower)
    {
        _collisionPower = collisionPower;
    }
}