using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PartData
{
    // ------------------------------------------------------------------
    // Importer가 직접 채우는 CSV 매핑용 최상위 필드
    // BasePartSpreadsheetImporter는 이 필드명과 CSV 헤더명을 1:1로 매칭한다.
    // ------------------------------------------------------------------
    public int Key;
    public string PartName;
    public PartType PartType;
    public List<Vector2Int> Shape;
    public float health;
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

    // ------------------------------------------------------------------
    // 사용 편의를 위한 계산형 래퍼
    // Importer는 이 프로퍼티를 건드리지 않고, 게임 로직이 사용한다.
    // ------------------------------------------------------------------
    public PartCommonStatData CommonStat => new PartCommonStatData(health, DefenseRate, Cost);

    public PartAttackStatData AttackStat
    {
        get
        {
            if (!HasAttackStat)
            {
                return null;
            }

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
            if (!HasDefenseStat)
            {
                return null;
            }

            return new PartDefenseStatData(CollisionPower);
        }
    }

    public bool HasAttackStat => PartType == PartType.Attack;
    public bool HasDefenseStat => PartType == PartType.Defense;

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

        if (PartType == PartType.None)
        {
            Debug.LogError($"{PartName}: PartType이 None입니다.");
            return false;
        }

        if (Shape == null || Shape.Count == 0)
        {
            Debug.LogError($"{PartName}: Shape가 비어 있습니다.");
            return false;
        }

        if (health < 0f)
        {
            Debug.LogError($"{PartName}: health는 0 이상이어야 합니다. 입력값: {health}");
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

        if (HasAttackStat)
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

        if (HasDefenseStat)
        {
            if (CollisionPower < 0f)
            {
                Debug.LogError($"{PartName}: CollisionPower는 0 이상이어야 합니다. 입력값: {CollisionPower}");
                return false;
            }
        }

        return true;
    }
}