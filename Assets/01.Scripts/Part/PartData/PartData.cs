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
    public bool CanUseAttack =>
        IsAttackUnit &&
        AttackDamage > 0f &&
        AttackSpeed > 0f &&
        AttackDistance > 0f &&
        TrajectoryType != AttackTrajectoryType.None;

    public bool CanUseCollision =>
        IsDefenseUnit &&
        CollisionPower > 0f;

    public bool CanUseSupport =>
        IsSupportUnit &&
        SupportRangeRadius > 0 &&
        !string.IsNullOrWhiteSpace(SupportEffectsRaw) &&
        ParseSupportEffects(SupportEffectsRaw).Count > 0;

    public bool IsArcAttack =>
        CanUseAttack &&
        TrajectoryType == AttackTrajectoryType.Arc;

    public bool IsDirectAttack =>
        CanUseAttack &&
        TrajectoryType == AttackTrajectoryType.Direct;

    public bool IsAreaAttack =>
        CanUseAttack &&
        AttackRangeRadius > 0;

    public bool IsSingleTargetAttack =>
        CanUseAttack &&
        AttackRangeRadius <= 0;

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

        if (IsUnit && UnitRoleType == UnitRoleType.None)
        {
            Debug.LogError($"{PartName}: Unit인데 UnitRoleType이 None입니다.");
            return false;
        }

        if (IsBuilding && UnitRoleType != UnitRoleType.None)
        {
            Debug.LogWarning($"{PartName}: Building인데 UnitRoleType이 None이 아닙니다.");
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

        if (IsAttackUnit)
        {
            if (AttackDamage < 0f)
            {
                Debug.LogError($"{PartName}: AttackDamage는 0 이상이어야 합니다. 입력값: {AttackDamage}");
                return false;
            }

            if (AttackSpeed < 0f)
            {
                Debug.LogError($"{PartName}: AttackSpeed는 0 이상이어야 합니다. 입력값: {AttackSpeed}");
                return false;
            }

            if (AttackRangeRadius < 0)
            {
                Debug.LogError($"{PartName}: AttackRangeRadius는 0 이상이어야 합니다. 입력값: {AttackRangeRadius}");
                return false;
            }

            if (AttackDistance < 0f)
            {
                Debug.LogError($"{PartName}: AttackDistance는 0 이상이어야 합니다. 입력값: {AttackDistance}");
                return false;
            }

            if (PenetrationRate < 0f || PenetrationRate > 1f)
            {
                Debug.LogError($"{PartName}: PenetrationRate는 0~1 사이여야 합니다. 입력값: {PenetrationRate}");
                return false;
            }
        }

        if (IsDefenseUnit && CollisionPower < 0f)
        {
            Debug.LogError($"{PartName}: CollisionPower는 0 이상이어야 합니다. 입력값: {CollisionPower}");
            return false;
        }

        if (IsSupportUnit && SupportRangeRadius < 0)
        {
            Debug.LogError($"{PartName}: SupportRangeRadius는 0 이상이어야 합니다. 입력값: {SupportRangeRadius}");
            return false;
        }

        ValidateBehaviorCombination();
        return true;
    }

    private void ValidateBehaviorCombination()
    {
        if (IsAttackUnit && !CanUseAttack)
        {
            Debug.LogWarning($"{PartName}: Attack 유닛이지만 실제 공격 가능한 스탯 조합이 아닙니다.");
        }

        if (IsDefenseUnit && !CanUseCollision)
        {
            Debug.LogWarning($"{PartName}: Defense 유닛이지만 실제 충돌 가능한 스탯 조합이 아닙니다.");
        }

        if (IsSupportUnit && !CanUseSupport)
        {
            Debug.LogWarning($"{PartName}: Support 유닛이지만 실제 지원 가능한 스탯 조합이 아닙니다.");
        }

        if (IsBuilding && (AttackDamage > 0f || CollisionPower > 0f || !string.IsNullOrWhiteSpace(SupportEffectsRaw)))
        {
            Debug.LogWarning($"{PartName}: Building인데 전용 유닛 스탯 값이 설정되어 있습니다.");
        }
    }

    private List<PartSupportEffectData> ParseSupportEffects(string raw)
    {
        List<PartSupportEffectData> effects = new List<PartSupportEffectData>();

        // 빈 문자열이면 빈 리스트 반환
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
                // 대상 역할 파싱
                SupportTargetRoleType targetRoleType =
                    (SupportTargetRoleType)Enum.Parse(typeof(SupportTargetRoleType), parts[0], true);

                // 대상 스탯 파싱
                SupportStatType targetStatType =
                    (SupportStatType)Enum.Parse(typeof(SupportStatType), parts[1], true);

                // 증가 방식 파싱
                ModifierType modifierType =
                    (ModifierType)Enum.Parse(typeof(ModifierType), parts[2], true);

                // 증가 수치 파싱
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