using UnityEngine;

public class RatTargetFinder : MonoBehaviour
{
    [SerializeField] private float _searchRadius = 10f;

    private RatController _ratController;
    public float SearchRadius => _searchRadius;

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

        RatController[] allRats = FindObjectsByType<RatController>(FindObjectsSortMode.None);
        if (allRats == null || allRats.Length == 0)
        {
            Debug.LogError($"{name}: FindNearestEnemy 실패 - RatController를 찾을 수 없습니다.");
            return null;
        }

        RatController nearestTarget = null;
        float nearestDistance = float.MaxValue;

        for(int i = 0; i < allRats.Length; i++)
        {
            RatController candidate = allRats[i];

            if (!IsValidTarget(candidate)) continue;

            float distance = Vector2.Distance(transform.position, candidate.transform.position);

            if (distance > _searchRadius) continue;

            if(distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestTarget = candidate;
            }
        }

        return nearestTarget;
    }

    public bool IsTargetWithinSearchRadius(RatController target)
    {
        if (_ratController == null)
        {
            Debug.LogError($"{name}: IsTargetWithinSearchRadius 실패 - RatController가 Null입니다.");
            return false;
        }

        if (target == null)
        {
            Debug.LogError($"{name}: IsTargetWithinSearchRadius 실패 - target이 Null입니다.");
            return false;
        }

        if (_searchRadius < 0f)
        {
            Debug.LogError($"{name}: IsTargetWithinSearchRadius 실패 - SearchRadius는 0 이상이어야 합니다. 입력값: {_searchRadius}");
            return false;
        }

        float distance = Vector2.Distance(transform.position, target.transform.position);
        return distance <= _searchRadius;
    }

    private bool IsValidTarget(RatController candidate)
    {
        if (candidate == null) return false;

        if (candidate == _ratController) return false;

        if(!_ratController.IsEnemy(candidate)) return false;

        if (candidate.RatStatRuntime == null)
        {
            Debug.LogError($"{candidate.name}: RatStatRuntime이 없어 타겟으로 사용할 수 없습니다.");
            return false;
        }

        if (candidate.RatStatRuntime.IsDead)
        {
            return false;
        }

        return true;
    }
}
