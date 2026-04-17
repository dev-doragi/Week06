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

        if (!_ratController.IsUnit())
        {
            return false;
        }

        if (!_ratController.CanUseCollision())
        {
            return false;
        }

        if (!_ratController.TryGetDefenseStat(out _))
        {
            Debug.LogError($"{name}: 충돌 가능한 스탯을 가져올 수 없습니다.");
            return false;
        }

        RatDamageCalculator.ApplyCollisionDamage(_ratController, target);
        return true;
    }
}