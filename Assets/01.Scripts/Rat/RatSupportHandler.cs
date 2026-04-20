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

    public void ProcessSupport()
    {
        if (_ratController == null)
        {
            Debug.LogError($"{name}: ProcessSupport 실패 - RatController가 Null입니다.");
            return;
        }

        if (!CanProvideSupport())
        {
            return;
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

        PartData partData = _ratController.PartData;
        if (partData == null)
        {
            Debug.LogError($"{name}: CanProvideSupport 실패 - PartData가 없습니다.");
            return false;
        }

        // 주요 라인: 이제 지원 제공자는 지원형 유닛 + 지원형 빌딩을 모두 포함한다.
        if (!partData.CanUseSupport)
        {
            return false;
        }

        if (!IsAltarSupportAvailable())
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

        if (!_ratController.TryGetSupportStat(out _))
        {
            Debug.LogError($"{name}: Support provider인데 SupportStat을 가져오지 못했습니다.");
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

    private bool IsAltarSupportAvailable()
    {
        if (_ratController == null)
        {
            return false;
        }

        if (!_ratController.IsBuilding())
        {
            return true;
        }

        PartData partData = _ratController.PartData;
        if (partData == null)
        {
            return false;
        }

        if (!partData.IsAltar)
        {
            return true;
        }

        AltarConnector altarConnector = GetComponent<AltarConnector>();
        if (altarConnector == null)
        {
            Debug.LogWarning($"{name}: 제단인데 AltarConnector가 없습니다.");
            return false;
        }

        return altarConnector.IsAltarActive;
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

        // 주요 라인: 현재 지원 수혜 대상은 기존과 동일하게 Unit만 허용한다.
        if (!target.IsUnit())
        {
            return false;
        }

        if (target.IsBuilding())
        {
            return false;
        }

        if (!(target.IsAttackUnit() || target.IsDefenseUnit()))
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

        IReadOnlyList<Vector2Int> sourceCells = _ratController.GetOccupiedCells();
        IReadOnlyList<Vector2Int> targetCells = target.GetOccupiedCells();

        if (sourceCells == null)
        {
            Debug.LogError($"{name}: CanSupportTarget 실패 - sourceCells가 Null입니다.");
            return false;
        }

        if (targetCells == null)
        {
            Debug.LogError($"{target.name}: CanSupportTarget 실패 - targetCells가 Null입니다.");
            return false;
        }

        return GridRangeUtility.IsWithinCellRadius(sourceCells, targetCells, supportStat.SupportRangeRadius);
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
                return target.IsAttackUnit() || target.IsDefenseUnit();

            case SupportTargetRoleType.Attack:
                return target.IsAttackUnit();

            case SupportTargetRoleType.Defense:
                return target.IsDefenseUnit();

            default:
                return false;
        }
    }
}