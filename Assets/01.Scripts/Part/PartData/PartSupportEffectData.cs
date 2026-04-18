using System;
using UnityEngine;

[Serializable]
public class PartSupportEffectData
{
    [SerializeField] private SupportTargetRoleType _targetRoleType;
    [SerializeField] private SupportStatType _targetStatType;
    [SerializeField] private ModifierType _modifierType;
    [SerializeField] private float _value;

    public SupportTargetRoleType TargetRoleType => _targetRoleType;
    public SupportStatType TargetStatType => _targetStatType;
    public ModifierType ModifierType => _modifierType;
    public float Value => _value;

    public PartSupportEffectData(
        SupportTargetRoleType targetRoleType,
        SupportStatType targetStatType,
        ModifierType modifierType,
        float value)
    {
        // 지원 대상 역할 저장
        _targetRoleType = targetRoleType;
        // 지원할 대상 스탯 저장
        _targetStatType = targetStatType;
        // 증가 방식 저장
        _modifierType = modifierType;
        // 증가 수치 저장
        _value = value;
    }
}