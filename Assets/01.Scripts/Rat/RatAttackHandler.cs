using UnityEngine;

public class RatAttackHandler : MonoBehaviour
{
    private RatController _ratController;
    private RatTargetFinder _ratTargetFinder;
    private float _lastAttackTime;

    public bool CanAttack
    {
        get
        {
            if (_ratController == null)
            {
                Debug.LogError($"{name}: CanAttack 확인 실패 - RatController가 Null입니다.");
                return false;
            }

            if (!_ratController.TryGetAttackStat(out var attackStat))
            {
                return false;
            }

            return Time.time >= _lastAttackTime + GetAttackInterval(attackStat.AttackSpeed);
        }
    }

    private void Awake()
    {
        _ratController = GetComponent<RatController>();
        if (_ratController == null)
        {
            Debug.LogError($"{name}: RatAttackHandler에 RatController가 없습니다.");
        }

        _ratTargetFinder = GetComponent<RatTargetFinder>();
        if (_ratTargetFinder == null)
        {
            Debug.LogError($"{name}: RatAttackHandler에 RatTargetFinder가 없습니다.");
        }
    }

    public bool TryAttackNearestEnemy()
    {
        if (_ratTargetFinder == null)
        {
            Debug.LogError($"{name}: TryAttackNearestEnemy 실패 - RatTargetFinder가 Null입니다.");
            return false;
        }

        RatController target = _ratTargetFinder.FindNearestEnemy();
        if (target == null) return false;

        return TryAttack(target);
    }

    public bool TryAttack(RatController target)
    {
        if (_ratController == null)
        {
            Debug.LogError($"{name}: TryAttack 실패 - RatController가 Null입니다.");
            return false;
        }

        if (target == null)
        {
            Debug.LogError($"{name}: TryAttack 실패 - target이 Null입니다.");
            return false;
        }

        if (!_ratController.IsEnemy(target)) return false;

        if (!_ratController.TryGetAttackStat(out var attackStat))
        {
            Debug.LogError($"{name}: 공격형 스탯이 없어 공격할 수 없습니다.");
            return false;
        }

        if (target.RatStatRuntime == null)
        {
            Debug.LogError($"{target.name}: RatStatRuntime이 없어 공격 대상이 될 수 없습니다.");
            return false;
        }

        if (target.RatStatRuntime.IsDead)
        {
            return false;
        }

        if (!CanAttack)
        {
            return false;
        }

        if (_ratTargetFinder != null && !_ratTargetFinder.IsTargetWithinSearchRadius(target))
        {
            return false;
        }

        if (!IsTargetInAttackDistance(target, attackStat.AttackDistance))
        {
            return false;
        }

        RatDamageCalculator.ApplyAttackDamage(_ratController, target);
        _lastAttackTime = Time.time;

        return true;
    }

    public bool IsTargetInAttackDistance(RatController target, float attackDistance)
    {
        if (target == null)
        {
            Debug.LogError($"{name}: IsTargetInAttackDistance 실패 - target이 Null입니다.");
            return false;
        }

        if (attackDistance < 0f)
        {
            Debug.LogError($"{name}: IsTargetInAttackDistance 실패 - attackDistance는 0 이상이어야 합니다. 입력값: {attackDistance}");
            return false;
        }

        float distance = Vector2.Distance(transform.position, target.transform.position);
        return distance <= attackDistance;
    }

    public float GetAttackRangeRadius()
    {
        if (_ratController == null)
        {
            Debug.LogError($"{name}: GetAttackRangeRadius 실패 - RatController가 Null입니다.");
            return 0;
        }

        if (!_ratController.TryGetAttackStat(out var attackStat))
        {
            Debug.LogError($"{name}: GetAttackRangeRadius 실패 - 공격형 스탯이 없습니다.");
            return 0;
        }

        return attackStat.AttackRangeRadius;
    }

    private float GetAttackInterval(float attackSpeed)
    {
        if (attackSpeed <= 0f)
        {
            Debug.LogError($"{name}: GetAttackInterval 실패 - attackSpeed는 0보다 커야 합니다. 입력값: {attackSpeed}");
            return float.MaxValue;
        }

        return 1f / attackSpeed;
    }
}