using UnityEngine;

public abstract class BaseAttackPerformer : MonoBehaviour, IAttackPerformer
{
    public abstract bool TryPerformAttack(RatController attacker, RatController target);

    protected bool ValidateAttackContext(RatController attacker, RatController target)
    {
        if (attacker == null)
        {
            Debug.LogError($"{name}: ValidateAttackContext 실패 - attacker가 Null입니다.");
            return false;
        }

        if (target == null)
        {
            Debug.LogError($"{name}: ValidateAttackContext 실패 - target이 Null입니다.");
            return false;
        }

        if (!attacker.IsAttackUnit())
        {
            Debug.LogError($"{attacker.name}: 공격형 유닛이 아닌데 공격 실행기를 사용하려고 했습니다.");
            return false;
        }

        if (!attacker.TryGetAttackStat(out _))
        {
            Debug.LogError($"{attacker.name}: 공격형 유닛인데 AttackStat을 가져오지 못했습니다.");
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

        return true;
    }
}
