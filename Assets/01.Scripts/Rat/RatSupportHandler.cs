using System.Collections.Generic;
using UnityEngine;

public class RatSupportHandler : MonoBehaviour
{
    [SerializeField] private bool _useAutoSupport = true;

    private RatController _ratController;

    public bool UseAutoSupport => _useAutoSupport;

    private void Awake()
    {
        _ratController = GetComponent<RatController>();
        if (_ratController == null)
        {
            Debug.LogError($"{name}: RatSupportHandler에 RatController가 없습니다.");
        }
    }

    public bool CanProvideSupport()
    {
        if (_ratController == null)
        {
            Debug.LogError($"{name}: CanProvideSupport 실패 - RatController가 Null입니다.");
            return false;
        }

        if (!_useAutoSupport)
        {
            return false;
        }

        if (!_ratController.IsUnit())
        {
            return false;
        }

        if (!_ratController.CanUseSupport())
        {
            return false;
        }

        if (_ratController.RatStatRuntime == null)
        {
            Debug.LogError($"{name}: CanProvideSupport 실패 - RatStatRuntime이 없습니다.");
            return false;
        }

        if (_ratController.RatStatRuntime.IsDead)
        {
            return false;
        }

        return true;
    }

    public bool TryGetSupportStat(out PartSupportStatData supportStat)
    {
        supportStat = null;

        if (_ratController == null)
        {
            Debug.LogError($"{name}: TryGetSupportStat 실패 - RatController가 Null입니다.");
            return false;
        }

        return _ratController.TryGetSupportStat(out supportStat);
    }

    public bool CanSupportTarget(RatController target)
    {
        if (_ratController == null)
        {
            Debug.LogError($"{name}: CanSupportTarget 실패 - RatController가 Null입니다.");
            return false;
        }

        if (target == null)
        {
            Debug.LogError($"{name}: CanSupportTarget 실패 - target이 Null입니다.");
            return false;
        }

        if (!CanProvideSupport())
        {
            return false;
        }

        if (target == _ratController)
        {
            return false;
        }

        if (_ratController.IsEnemy(target))
        {
            return false;
        }

        if (!target.IsUnit())
        {
            return false;
        }

        if (target.IsBuilding())
        {
            return false;
        }

        if (target.RatStatRuntime == null)
        {
            Debug.LogError($"{target.name}: RatStatRuntime이 없어 지원 대상이 될 수 없습니다.");
            return false;
        }

        if (target.RatStatRuntime.IsDead)
        {
            return false;
        }

        if (!TryGetSupportStat(out var supportStat))
        {
            return false;
        }

        float distance = Vector2.Distance(transform.position, target.transform.position);
        return distance <= supportStat.SupportRangeRadius;
    }

    public IReadOnlyList<PartSupportEffectData> GetApplicableEffects(RatController target)
    {
        List<PartSupportEffectData> result = new List<PartSupportEffectData>();

        if (target == null)
        {
            Debug.LogError($"{name}: GetApplicableEffects 실패 - target이 Null입니다.");
            return result;
        }

        if (!CanSupportTarget(target))
        {
            return result;
        }

        if (!TryGetSupportStat(out var supportStat))
        {
            return result;
        }

        IReadOnlyList<PartSupportEffectData> effects = supportStat.Effects;
        if (effects == null)
        {
            return result;
        }

        for (int i = 0; i < effects.Count; i++)
        {
            PartSupportEffectData effect = effects[i];
            if (effect == null)
            {
                continue;
            }

            if (!CanApplyEffectToTarget(target, effect))
            {
                continue;
            }

            result.Add(effect);
        }

        return result;
    }

    private bool CanApplyEffectToTarget(RatController target, PartSupportEffectData effect)
    {
        if (target == null || effect == null)
        {
            return false;
        }

        switch (effect.TargetRoleType)
        {
            case SupportTargetRoleType.All:
                return target.IsUnit();

            case SupportTargetRoleType.Attack:
                return target.IsAttackUnit();

            case SupportTargetRoleType.Defense:
                return target.IsDefenseUnit();

            default:
                return false;
        }
    }
}