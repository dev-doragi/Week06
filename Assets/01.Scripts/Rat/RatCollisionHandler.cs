using UnityEngine;

public class RatCollisionHandler : MonoBehaviour
{
    private RatController _ratController;

    private void Awake()
    {
        _ratController = GetComponent<RatController>();
        if (_ratController == null)
        {
            Debug.LogError($"{name}: RatCollisionHandler에 RatController가 없습니다.");
        }
    }

    public bool TryCollide(RatController target)
    {
        if (_ratController == null)
        {
            Debug.LogError($"{name}: TryCollide 실패 - RatController가 Null입니다.");
            return false;
        }

        if (target == null)
        {
            Debug.LogError($"{name}: TryCollide 실패 - target이 Null입니다.");
            return false;
        }

        if (!_ratController.IsDefenseUnit())
        {
            return false;
        }

        // 주요 라인: wheel은 충돌 피해 대상이 아니다.
        if (!target.CanBeCombatTarget())
        {
            return false;
        }

        if (!_ratController.TryGetDefenseStat(out _))
        {
            Debug.LogError($"{name}: Defense 유닛인데 DefenseStat을 가져오지 못했습니다.");
            return false;
        }

        RatDamageCalculator.ApplyCollisionDamage(_ratController, target);
        return true;
    }
}