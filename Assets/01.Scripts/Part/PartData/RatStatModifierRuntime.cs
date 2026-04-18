using System.Collections.Generic;
using UnityEngine;

public class RatStatModifierRuntime : MonoBehaviour
{
    [SerializeField] private bool _useAutoRecalculate = true;

    private RatController _ratController;

    private float _attackDamageFlatBonus;
    private float _attackSpeedFlatBonus;
    private float _penetrationRateFlatBonus;
    private float _defenseRateFlatBonus;

    private float _attackDamagePercentBonus;
    private float _attackSpeedPercentBonus;
    private float _penetrationRatePercentBonus;
    private float _defenseRatePercentBonus;

    public float AttackDamageFlatBonus => _attackDamageFlatBonus;
    public float AttackSpeedFlatBonus => _attackSpeedFlatBonus;
    public float PenetrationRateFlatBonus => _penetrationRateFlatBonus;
    public float DefenseRateFlatBonus => _defenseRateFlatBonus;

    public float AttackDamagePercentBonus => _attackDamagePercentBonus;
    public float AttackSpeedPercentBonus => _attackSpeedPercentBonus;
    public float PenetrationRatePercentBonus => _penetrationRatePercentBonus;
    public float DefenseRatePercentBonus => _defenseRatePercentBonus;

    private void Awake()
    {
        _ratController = GetComponent<RatController>();
        if (_ratController == null)
        {
            Debug.LogError($"{name}: RatStatModifierRuntime에 RatController가 없습니다.");
        }
    }

    private void Update()
    {
        if (!_useAutoRecalculate)
        {
            return;
        }

        RecalculateSupportModifiers();
    }

    public void ResetModifiers()
    {
        _attackDamageFlatBonus = 0f;
        _attackSpeedFlatBonus = 0f;
        _penetrationRateFlatBonus = 0f;
        _defenseRateFlatBonus = 0f;

        _attackDamagePercentBonus = 0f;
        _attackSpeedPercentBonus = 0f;
        _penetrationRatePercentBonus = 0f;
        _defenseRatePercentBonus = 0f;
    }

    public void RecalculateSupportModifiers()
    {
        if (_ratController == null)
        {
            Debug.LogError($"{name}: RecalculateSupportModifiers 실패 - RatController가 Null입니다.");
            return;
        }

        ResetModifiers();

        if (!_ratController.IsUnit())
        {
            return;
        }

        if (_ratController.IsBuilding())
        {
            return;
        }

        if (_ratController.RatStatRuntime == null)
        {
            Debug.LogError($"{name}: RecalculateSupportModifiers 실패 - RatStatRuntime이 없습니다.");
            return;
        }

        if (_ratController.RatStatRuntime.IsDead)
        {
            return;
        }

        RatSupportHandler[] supporters = FindObjectsByType<RatSupportHandler>(FindObjectsSortMode.None);
        if (supporters == null || supporters.Length == 0)
        {
            return;
        }

        for (int i = 0; i < supporters.Length; i++)
        {
            RatSupportHandler supporter = supporters[i];
            if (supporter == null)
            {
                continue;
            }

            if (!supporter.CanSupportTarget(_ratController))
            {
                continue;
            }

            IReadOnlyList<PartSupportEffectData> effects = supporter.GetApplicableEffects(_ratController);
            if (effects == null || effects.Count == 0)
            {
                continue;
            }

            for (int j = 0; j < effects.Count; j++)
            {
                PartSupportEffectData effect = effects[j];
                if (effect == null)
                {
                    continue;
                }

                ApplyModifier(effect.TargetStatType, effect.ModifierType, effect.Value);
            }
        }
    }

    public void ApplyModifier(SupportStatType statType, ModifierType modifierType, float value)
    {
        if (modifierType == ModifierType.None)
        {
            Debug.LogError($"{name}: ApplyModifier 실패 - ModifierType이 None입니다.");
            return;
        }

        switch (statType)
        {
            case SupportStatType.AttackDamage:
                ApplyAttackDamageModifier(modifierType, value);
                break;

            case SupportStatType.AttackSpeed:
                ApplyAttackSpeedModifier(modifierType, value);
                break;

            case SupportStatType.PenetrationRate:
                ApplyPenetrationRateModifier(modifierType, value);
                break;

            case SupportStatType.DefenseRate:
                ApplyDefenseRateModifier(modifierType, value);
                break;

            default:
                Debug.LogError($"{name}: ApplyModifier 실패 - 지원하지 않는 SupportStatType입니다. 입력값: {statType}");
                break;
        }
    }

    private void ApplyAttackDamageModifier(ModifierType modifierType, float value)
    {
        if (modifierType == ModifierType.Flat)
        {
            _attackDamageFlatBonus += value;
        }
        else if (modifierType == ModifierType.Percent)
        {
            _attackDamagePercentBonus += value;
        }
    }

    private void ApplyAttackSpeedModifier(ModifierType modifierType, float value)
    {
        if (modifierType == ModifierType.Flat)
        {
            _attackSpeedFlatBonus += value;
        }
        else if (modifierType == ModifierType.Percent)
        {
            _attackSpeedPercentBonus += value;
        }
    }

    private void ApplyPenetrationRateModifier(ModifierType modifierType, float value)
    {
        if (modifierType == ModifierType.Flat)
        {
            _penetrationRateFlatBonus += value;
        }
        else if (modifierType == ModifierType.Percent)
        {
            _penetrationRatePercentBonus += value;
        }
    }

    private void ApplyDefenseRateModifier(ModifierType modifierType, float value)
    {
        if (modifierType == ModifierType.Flat)
        {
            _defenseRateFlatBonus += value;
        }
        else if (modifierType == ModifierType.Percent)
        {
            _defenseRatePercentBonus += value;
        }
    }
}