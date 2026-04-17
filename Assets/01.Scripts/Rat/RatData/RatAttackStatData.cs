using UnityEngine;
using System;

public class RatAttackStatData
{
    [SerializeField] private float _attackDamage;
    [SerializeField] private float _attackSpeed;
    [SerializeField] private float _attackRangeRadius;
    [SerializeField] private float _attackDistance;
    [SerializeField] private AttackTrajectoryType _trajectoryType;
    [SerializeField, Range(0f, 1f)] private float _penetrationRate;

    public float AttackDamage => _attackDamage;
    public float AttackSpeed => _attackSpeed;
    public float AttackRangeRadius => _attackRangeRadius;
    public float AttackDistance => _attackDistance;
    public AttackTrajectoryType TrajectoryType => _trajectoryType;
    public float PenetrationRate => _penetrationRate;
}
