using UnityEngine;

public class RatAttackHandler : MonoBehaviour
{
    private RatController _ratController;
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

        if (!_ratController.TryGetAttackStat(out var attackStat))
        {
            Debug.LogError($"{name}: 공격형 스탯이 없어 공격할 수 없습니다.");
            return false;
        }

        if (!CanAttack)
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