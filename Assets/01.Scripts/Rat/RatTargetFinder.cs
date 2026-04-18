using System.Collections.Generic;
using UnityEngine;

public class RatTargetFinder : MonoBehaviour
{
    private RatController _ratController;

    private void Awake()
    {
        _ratController = GetComponent<RatController>();
        if (_ratController == null)
        {
            Debug.LogError($"{name}: RatTargetFinder에 RatController가 없습니다.");
        }
    }

    public RatController FindNearestEnemy()
    {
        if (_ratController == null)
        {
            Debug.LogError($"{name}: FindNearestEnemy 실패 - RatController가 Null입니다.");
            return null;
        }

        // 1순위: 사거리 내 적 Core 탐색
        RatController coreTarget = FindEnemyCoreInAttackDistance();
        if (coreTarget != null)
        {
            return coreTarget;
        }

        // 2순위: 가장 가까운 적 Unit / Building 탐색
        return FindClosestEnemyTarget();
    }

    public bool IsValidTarget(RatController target)
    {
        if (_ratController == null)
        {
            Debug.LogError($"{name}: IsValidTarget 실패 - RatController가 Null입니다.");
            return false;
        }

        if (target == null)
        {
            return false;
        }

        if (target == _ratController)
        {
            return false;
        }

        if (target.RatStatRuntime == null)
        {
            return false;
        }

        if (target.RatStatRuntime.IsDead)
        {
            return false;
        }

        if (!_ratController.IsEnemy(target))
        {
            return false;
        }

        // 주요 라인: 이제 적 Unit뿐 아니라 적 Building도 타겟이 될 수 있다.
        if (!target.IsUnit() && !target.IsBuilding())
        {
            return false;
        }

        return true;
    }

    private RatController FindEnemyCoreInAttackDistance()
    {
        if (_ratController == null)
        {
            Debug.LogError($"{name}: FindEnemyCoreInAttackDistance 실패 - RatController가 Null입니다.");
            return null;
        }

        if (!_ratController.IsAttackUnit())
        {
            return null;
        }

        if (!_ratController.TryGetAttackStat(out var attackStat))
        {
            Debug.LogError($"{name}: Attack 유닛인데 AttackStat을 가져오지 못했습니다.");
            return null;
        }

        RatController[] allTargets = FindObjectsByType<RatController>(FindObjectsSortMode.None);

        RatController bestCoreTarget = null;
        float bestDistanceSqr = float.MaxValue;

        for (int i = 0; i < allTargets.Length; i++)
        {
            RatController candidate = allTargets[i];
            if (!IsValidTarget(candidate))
            {
                continue;
            }

            PartData candidateData = candidate.PartData;
            if (candidateData == null)
            {
                continue;
            }

            if (!candidateData.IsCoreBuilding)
            {
                continue;
            }

            if (!IsWithinAttackDistance(candidate, attackStat.AttackDistance))
            {
                continue;
            }

            float distanceSqr = GetWorldDistanceSqr(candidate);
            if (distanceSqr < bestDistanceSqr)
            {
                bestDistanceSqr = distanceSqr;
                bestCoreTarget = candidate;
            }
        }

        return bestCoreTarget;
    }

    private RatController FindClosestEnemyTarget()
    {
        if (_ratController == null)
        {
            Debug.LogError($"{name}: FindClosestEnemyTarget 실패 - RatController가 Null입니다.");
            return null;
        }

        RatController[] allTargets = FindObjectsByType<RatController>(FindObjectsSortMode.None);

        RatController bestTarget = null;
        float bestDistanceSqr = float.MaxValue;

        for (int i = 0; i < allTargets.Length; i++)
        {
            RatController candidate = allTargets[i];
            if (!IsValidTarget(candidate))
            {
                continue;
            }

            float distanceSqr = GetWorldDistanceSqr(candidate);
            if (distanceSqr < bestDistanceSqr)
            {
                bestDistanceSqr = distanceSqr;
                bestTarget = candidate;
            }
        }

        return bestTarget;
    }

    private bool IsWithinAttackDistance(RatController target, float attackDistance)
    {
        if (_ratController == null)
        {
            Debug.LogError($"{name}: IsWithinAttackDistance 실패 - RatController가 Null입니다.");
            return false;
        }

        if (target == null)
        {
            return false;
        }

        if (attackDistance < 0f)
        {
            Debug.LogError($"{name}: IsWithinAttackDistance 실패 - attackDistance는 0 이상이어야 합니다. 입력값: {attackDistance}");
            return false;
        }

        IReadOnlyList<Vector2Int> attackerCells = _ratController.GetOccupiedCells();
        IReadOnlyList<Vector2Int> targetCells = target.GetOccupiedCells();

        if (attackerCells == null || targetCells == null)
        {
            return false;
        }

        return GridRangeUtility.IsWithinAttackDistance(attackerCells, targetCells, attackDistance);
    }

    private float GetWorldDistanceSqr(RatController target)
    {
        if (target == null)
        {
            return float.MaxValue;
        }

        Vector3 delta = target.transform.position - transform.position;
        return delta.sqrMagnitude;
    }
}