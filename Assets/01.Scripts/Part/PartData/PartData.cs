using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

[System.Serializable]
public class PartData
{
    // ------------------------------------------------------------
    // Importer가 직접 채우는 최상위 flat field
    // CSV 헤더와 정확히 일치해야 한다.
    // ------------------------------------------------------------
    public int Key;
    public string PartName;
    public PartCategory PartCategory;
    public UnitRoleType UnitRoleType;
    public List<Vector2Int> Shape;
    public float Hp;
    public float DefenseRate;
    public int Cost;
    public string SpriteName;
    public Sprite Icon;

    public float AttackDamage;
    public float AttackSpeed;
    public int AttackRangeRadius;
    public float AttackDistance;
    public AttackTrajectoryType TrajectoryType;
    public float PenetrationRate;

    public float CollisionPower;

    public int SupportRangeRadius;
    public string SupportEffectsRaw;

    // ------------------------------------------------------------
    // 런타임용 래퍼
    // ------------------------------------------------------------
    public PartCommonStatData CommonStat => new PartCommonStatData(Hp, DefenseRate, Cost);

    public PartAttackStatData AttackStat
    {
        get
        {
            if (!CanUseAttack)
            {
                return null;
            }

            // 공격형 스탯 조립
            return new PartAttackStatData(
                AttackDamage,
                AttackSpeed,
                AttackRangeRadius,
                AttackDistance,
                TrajectoryType,
                PenetrationRate);
        }
    }

    public PartDefenseStatData DefenseStat
    {
        get
        {
            if (!CanUseCollision)
            {
                return null;
            }

            // 방어형 스탯 조립
            return new PartDefenseStatData(CollisionPower);
        }
    }

    public PartSupportStatData SupportStat
    {
        get
        {
            if (!CanUseSupport)
            {
                return null;
            }

            // 지원형 스탯 조립
            return new PartSupportStatData(
                SupportRangeRadius,
                ParseSupportEffects(SupportEffectsRaw));
        }
    }

    // ------------------------------------------------------------
    // 최상위 분류
    // ------------------------------------------------------------
    public bool IsUnit => PartCategory == PartCategory.Unit;
    public bool IsBuilding => PartCategory == PartCategory.Building;

    public bool IsAttackUnit => IsUnit && UnitRoleType == UnitRoleType.Attack;
    public bool IsDefenseUnit => IsUnit && UnitRoleType == UnitRoleType.Defense;
    public bool IsSupportUnit => IsUnit && UnitRoleType == UnitRoleType.Support;

    // ------------------------------------------------------------
    // 실제 기능 판정
    // ------------------------------------------------------------
    public bool CanUseAttack => IsAttackUnit;
    public bool CanUseCollision => IsDefenseUnit;
    public bool CanUseSupport => IsSupportUnit;

    public bool IsArcAttack => IsAttackUnit && TrajectoryType == AttackTrajectoryType.Arc;
    public bool IsDirectAttack => IsAttackUnit && TrajectoryType == AttackTrajectoryType.Direct;
    public bool IsAreaAttack => IsAttackUnit && AttackRangeRadius > 0;
    public bool IsSingleTargetAttack => IsAttackUnit && AttackRangeRadius <= 0;

    // ------------------------------------------------------------
    // 기존 호환성용
    // ------------------------------------------------------------
    public bool HasAttackStat => IsAttackUnit;
    public bool HasDefenseStat => IsDefenseUnit;
    public bool HasSupportStat => IsSupportUnit;

    public bool IsValid()
    {
        if (Key <= 0)
        {
            Debug.LogError($"{PartName}: Key가 0 이하입니다.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(PartName))
        {
            Debug.LogError($"Key {Key}: PartName이 비어 있습니다.");
            return false;
        }

        if (PartCategory == PartCategory.None)
        {
            Debug.LogError($"{PartName}: PartCategory가 None입니다.");
            return false;
        }

        if (Shape == null || Shape.Count == 0)
        {
            Debug.LogError($"{PartName}: Shape가 비어 있습니다.");
            return false;
        }

        if (Hp < 0f)
        {
            Debug.LogError($"{PartName}: Hp는 0 이상이어야 합니다. 입력값: {Hp}");
            return false;
        }

        if (DefenseRate < 0f || DefenseRate > 1f)
        {
            Debug.LogError($"{PartName}: DefenseRate는 0~1 사이여야 합니다. 입력값: {DefenseRate}");
            return false;
        }

        if (Cost < 0)
        {
            Debug.LogError($"{PartName}: Cost는 0 이상이어야 합니다. 입력값: {Cost}");
            return false;
        }

        if (IsUnit && UnitRoleType == UnitRoleType.None)
        {
            Debug.LogError($"{PartName}: Unit인데 UnitRoleType이 None입니다.");
            return false;
        }

        if (IsBuilding && UnitRoleType != UnitRoleType.None)
        {
            Debug.LogError($"{PartName}: Building인데 UnitRoleType이 None이 아닙니다.");
            return false;
        }

        if (IsAttackUnit)
        {
            if (!ValidateAttackUnit())
            {
                return false;
            }
        }
        else if (IsDefenseUnit)
        {
            if (!ValidateDefenseUnit())
            {
                return false;
            }
        }
        else if (IsSupportUnit)
        {
            if (!ValidateSupportUnit())
            {
                return false;
            }
        }
        else if (IsBuilding)
        {
            if (!ValidateBuilding())
            {
                return false;
            }
        }

        return true;
    }

    private bool ValidateAttackUnit()
    {
        if (AttackDamage <= 0f)
        {
            Debug.LogError($"{PartName}: Attack 유닛은 AttackDamage가 0보다 커야 합니다.");
            return false;
        }

        if (AttackSpeed <= 0f)
        {
            Debug.LogError($"{PartName}: Attack 유닛은 AttackSpeed가 0보다 커야 합니다.");
            return false;
        }

        if (AttackRangeRadius < 0)
        {
            Debug.LogError($"{PartName}: Attack 유닛의 AttackRangeRadius는 0 이상이어야 합니다.");
            return false;
        }

        if (AttackDistance <= 0f)
        {
            Debug.LogError($"{PartName}: Attack 유닛은 AttackDistance가 0보다 커야 합니다.");
            return false;
        }

        if (TrajectoryType == AttackTrajectoryType.None)
        {
            Debug.LogError($"{PartName}: Attack 유닛은 TrajectoryType이 None일 수 없습니다.");
            return false;
        }

        if (PenetrationRate < 0f || PenetrationRate > 1f)
        {
            Debug.LogError($"{PartName}: Attack 유닛의 PenetrationRate는 0~1 사이여야 합니다.");
            return false;
        }

        if (CollisionPower != 0f)
        {
            Debug.LogError($"{PartName}: Attack 유닛은 CollisionPower를 가질 수 없습니다.");
            return false;
        }

        if (SupportRangeRadius != 0)
        {
            Debug.LogError($"{PartName}: Attack 유닛은 SupportRangeRadius를 가질 수 없습니다.");
            return false;
        }

        if (!string.IsNullOrWhiteSpace(SupportEffectsRaw))
        {
            Debug.LogError($"{PartName}: Attack 유닛은 SupportEffectsRaw를 가질 수 없습니다.");
            return false;
        }

        return true;
    }

    private bool ValidateDefenseUnit()
    {
        if (CollisionPower <= 0f)
        {
            Debug.LogError($"{PartName}: Defense 유닛은 CollisionPower가 0보다 커야 합니다.");
            return false;
        }

        if (AttackDamage != 0f ||
            AttackSpeed != 0f ||
            AttackRangeRadius != 0 ||
            AttackDistance != 0f ||
            TrajectoryType != AttackTrajectoryType.None ||
            PenetrationRate != 0f)
        {
            Debug.LogError($"{PartName}: Defense 유닛은 공격 스탯을 가질 수 없습니다.");
            return false;
        }

        if (SupportRangeRadius != 0)
        {
            Debug.LogError($"{PartName}: Defense 유닛은 SupportRangeRadius를 가질 수 없습니다.");
            return false;
        }

        if (!string.IsNullOrWhiteSpace(SupportEffectsRaw))
        {
            Debug.LogError($"{PartName}: Defense 유닛은 SupportEffectsRaw를 가질 수 없습니다.");
            return false;
        }

        return true;
    }

    private bool ValidateSupportUnit()
    {
        if (SupportRangeRadius <= 0)
        {
            Debug.LogError($"{PartName}: Support 유닛은 SupportRangeRadius가 0보다 커야 합니다.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(SupportEffectsRaw))
        {
            Debug.LogError($"{PartName}: Support 유닛은 SupportEffectsRaw가 비어 있을 수 없습니다.");
            return false;
        }

        List<PartSupportEffectData> effects = ParseSupportEffects(SupportEffectsRaw);
        if (effects.Count == 0)
        {
            Debug.LogError($"{PartName}: Support 유닛은 유효한 지원 효과를 최소 1개 이상 가져야 합니다.");
            return false;
        }

        if (AttackDamage != 0f ||
            AttackSpeed != 0f ||
            AttackRangeRadius != 0 ||
            AttackDistance != 0f ||
            TrajectoryType != AttackTrajectoryType.None ||
            PenetrationRate != 0f)
        {
            Debug.LogError($"{PartName}: Support 유닛은 공격 스탯을 가질 수 없습니다.");
            return false;
        }

        if (CollisionPower != 0f)
        {
            Debug.LogError($"{PartName}: Support 유닛은 CollisionPower를 가질 수 없습니다.");
            return false;
        }

        return true;
    }

    private bool ValidateBuilding()
    {
        if (AttackDamage != 0f ||
            AttackSpeed != 0f ||
            AttackRangeRadius != 0 ||
            AttackDistance != 0f ||
            TrajectoryType != AttackTrajectoryType.None ||
            PenetrationRate != 0f)
        {
            Debug.LogError($"{PartName}: Building은 공격 스탯을 가질 수 없습니다.");
            return false;
        }

        if (CollisionPower != 0f)
        {
            Debug.LogError($"{PartName}: Building은 CollisionPower를 가질 수 없습니다.");
            return false;
        }

        if (SupportRangeRadius != 0)
        {
            Debug.LogError($"{PartName}: Building은 SupportRangeRadius를 가질 수 없습니다.");
            return false;
        }

        if (!string.IsNullOrWhiteSpace(SupportEffectsRaw))
        {
            Debug.LogError($"{PartName}: Building은 SupportEffectsRaw를 가질 수 없습니다.");
            return false;
        }

        return true;
    }

    private List<PartSupportEffectData> ParseSupportEffects(string raw)
    {
        List<PartSupportEffectData> effects = new List<PartSupportEffectData>();

        if (string.IsNullOrWhiteSpace(raw))
        {
            return effects;
        }

        string[] entries = raw.Split('|', StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < entries.Length; i++)
        {
            string entry = entries[i].Trim();
            if (string.IsNullOrWhiteSpace(entry))
            {
                continue;
            }

            string[] parts = entry.Split(':');
            if (parts.Length != 4)
            {
                Debug.LogWarning($"{PartName}: SupportEffectsRaw 파싱 실패 - '{entry}'는 4개 항목 형식이 아닙니다.");
                continue;
            }

            try
            {
                SupportTargetRoleType targetRoleType =
                    (SupportTargetRoleType)Enum.Parse(typeof(SupportTargetRoleType), parts[0], true);

                SupportStatType targetStatType =
                    (SupportStatType)Enum.Parse(typeof(SupportStatType), parts[1], true);

                ModifierType modifierType =
                    (ModifierType)Enum.Parse(typeof(ModifierType), parts[2], true);

                float value = float.Parse(parts[3], CultureInfo.InvariantCulture);

                effects.Add(new PartSupportEffectData(
                    targetRoleType,
                    targetStatType,
                    modifierType,
                    value));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{PartName}: SupportEffectsRaw 파싱 실패 - '{entry}' / {e.Message}");
            }
        }

        return effects;
    }
}