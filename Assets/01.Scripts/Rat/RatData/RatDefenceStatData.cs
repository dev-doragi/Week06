using UnityEngine;
using System;

[Serializable]
public class RatDefenceStatData
{
    [SerializeField] private float _collisionPower;

    public float CollisionPower => _collisionPower;
}
