using UnityEngine;

public static class RatDamageCalculator
{
    public static float CalculateEffectiveDefenseRate(float defenseRate, float penetrationRate)
    {
        if (defenseRate < 0f || defenseRate > 1f)
        {
            Debug.LogError($"CalculateEffectiveDefenseRate 실패 - defenseRate는 0~1 사이여야 합니다. 입력값: {defenseRate}");
            return 0f;
        }

        if (penetrationRate < 0f || penetrationRate > 1f)
        {
            Debug.LogError($"CalculateEffectiveDefenseRate 실패 - penetrationRate는 0~1 사이여야 합니다. 입력값: {penetrationRate}");
            return 0f;
        }

        float effectiveDefenseRate = defenseRate * (1f - penetrationRate);
        return Mathf.Clamp01(effectiveDefenseRate);
    }

    public static float CalculateAttackDamage(float attackDamage, float defenseRate, float penetrationRate)
    {
        if (attackDamage < 0f)
        {
            Debug.LogError($"CalculateAttackDamage 실패 - attackDamage는 0 이상이어야 합니다. 입력값: {attackDamage}");
            return 0f;
        }

        float effectiveDefenseRate = CalculateEffectiveDefenseRate(defenseRate, penetrationRate);
        float finalDamage = attackDamage * (1f - effectiveDefenseRate);

        return Mathf.Max(0f, finalDamage);
    }

    public static float CalculateAttackDamage(RatController attacker, RatController target)
    {
        if (attacker == null)
        {
            Debug.LogError("CalculateAttackDamage 실패 - attacker가 Null입니다.");
            return 0f;
        }

        if (target == null)
        {
            Debug.LogError("CalculateAttackDamage 실패 - target이 Null입니다.");
            return 0f;
        }

        if (!attacker.TryGetAttackStat(out var attackStat))
        {
            Debug.LogError($"{attacker.name}: 공격 스탯이 없어 일반 공격 피해를 계산할 수 없습니다.");
            return 0f;
        }

        RatStatModifierRuntime attackerModifier = attacker.GetStatModifierRuntime();
        RatStatModifierRuntime targetModifier = target.GetStatModifierRuntime();

        float attackDamage = GetEffectiveAttackDamage(attackStat, attackerModifier);
        float penetrationRate = GetEffectivePenetrationRate(attackStat, attackerModifier);
        float targetDefenseRate = target.RatStatRuntime.GetEffectiveDefenseRate(targetModifier);

        return CalculateAttackDamage(attackDamage, targetDefenseRate, penetrationRate);
    }

    public static float CalculateCollisionDamage(float attackerCollisionPower, float targetCollisionPower)
    {
        if (attackerCollisionPower < 0f)
        {
            Debug.LogError($"CalculateCollisionDamage 실패 - attackerCollisionPower는 0 이상이어야 합니다. 입력값: {attackerCollisionPower}");
            return 0f;
        }

        if (targetCollisionPower < 0f)
        {
            Debug.LogError($"CalculateCollisionDamage 실패 - targetCollisionPower는 0 이상이어야 합니다. 입력값: {targetCollisionPower}");
            return 0f;
        }

        if (attackerCollisionPower <= targetCollisionPower)
        {
            return 0f;
        }

        return attackerCollisionPower - targetCollisionPower;
    }

    public static float CalculateCollisionDamage(RatController attacker, RatController target)
    {
        if (attacker == null)
        {
            Debug.LogError("CalculateCollisionDamage 실패 - attacker가 Null입니다.");
            return 0f;
        }

        if (target == null)
        {
            Debug.LogError("CalculateCollisionDamage 실패 - target이 Null입니다.");
            return 0f;
        }

        if (!attacker.TryGetDefenseStat(out var attackerDefenseStat))
        {
            Debug.LogError($"{attacker.name}: 충돌 스탯이 없어 충돌 피해를 계산할 수 없습니다.");
            return 0f;
        }

        float attackerCollisionPower = attackerDefenseStat.CollisionPower;
        float targetCollisionPower = 0f;

        if (target.TryGetDefenseStat(out var targetDefenseStat))
        {
            targetCollisionPower = targetDefenseStat.CollisionPower;
        }

        return CalculateCollisionDamage(attackerCollisionPower, targetCollisionPower);
    }

    public static void ApplyAttackDamage(RatController attacker, RatController target)
    {
        if (attacker == null)
        {
            Debug.LogError("ApplyAttackDamage 실패 - attacker가 Null입니다.");
            return;
        }

        if (target == null)
        {
            Debug.LogError("ApplyAttackDamage 실패 - target이 Null입니다.");
            return;
        }

        float damage = CalculateAttackDamage(attacker, target);
        target.ApplyDirectDamage(damage);
    }

    public static void ApplyCollisionDamage(RatController attacker, RatController target)
    {
        if (attacker == null)
        {
            Debug.LogError("ApplyCollisionDamage 실패 - attacker가 Null입니다.");
            return;
        }

        if (target == null)
        {
            Debug.LogError("ApplyCollisionDamage 실패 - target이 Null입니다.");
            return;
        }

        float damage = CalculateCollisionDamage(attacker, target);
        target.ApplyDirectDamage(damage);
    }

    private static float GetEffectiveAttackDamage(PartAttackStatData attackStat, RatStatModifierRuntime modifierRuntime)
    {
        float baseValue = attackStat.AttackDamage;

        if (modifierRuntime == null)
        {
            return baseValue;
        }

        float finalValue = baseValue;
        finalValue += modifierRuntime.AttackDamageFlatBonus;
        finalValue += baseValue * modifierRuntime.AttackDamagePercentBonus;

        return Mathf.Max(0f, finalValue);
    }

    private static float GetEffectivePenetrationRate(PartAttackStatData attackStat, RatStatModifierRuntime modifierRuntime)
    {
        float baseValue = attackStat.PenetrationRate;

        if (modifierRuntime == null)
        {
            return Mathf.Clamp01(baseValue);
        }

        float finalValue = baseValue;
        finalValue += modifierRuntime.PenetrationRateFlatBonus;
        finalValue += baseValue * modifierRuntime.PenetrationRatePercentBonus;

        return Mathf.Clamp01(finalValue);
    }
}