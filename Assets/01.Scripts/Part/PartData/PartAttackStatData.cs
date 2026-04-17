using System;
using UnityEngine;

[Serializable]
public class PartAttackStatData
{
    [SerializeField] private float _attackDamage;
    [SerializeField] private float _attackSpeed;
    [SerializeField] private int _attackRangeRadius;
    [SerializeField] private float _attackDistance;
    [SerializeField] private AttackTrajectoryType _trajectoryType;
    [SerializeField][Range(0f, 1f)] private float _penetrationRate;

    public float AttackDamage => _attackDamage;
    public float AttackSpeed => _attackSpeed;
    public int AttackRangeRadius => _attackRangeRadius;
    public float AttackDistance => _attackDistance;
    public AttackTrajectoryType TrajectoryType => _trajectoryType;
    public float PenetrationRate => _penetrationRate;

    public PartAttackStatData(
        float attackDamage,
        float attackSpeed,
        int attackRangeRadius,
        float attackDistance,
        AttackTrajectoryType trajectoryType,
        float penetrationRate)
    {
        _attackDamage = attackDamage;
        _attackSpeed = attackSpeed;
        _attackRangeRadius = attackRangeRadius;
        _attackDistance = attackDistance;
        _trajectoryType = trajectoryType;
        _penetrationRate = penetrationRate;
    }
}