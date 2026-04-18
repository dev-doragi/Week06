using UnityEngine;
using System.Collections.Generic;

public enum BombProjectileMoveType
{
    Direct = 0,
    Arc = 1
}

public class BombProjectile : MonoBehaviour
{
    [SerializeField] private bool _explodeOnTriggerEnter = true;

    private RatController _attacker;
    private RatController _primaryTarget;

    private int _attackRangeRadius;
    private BombProjectileMoveType _moveType;

    private Vector3 _startPosition;
    private Vector3 _targetPosition;
    private float _travelTime;
    private float _arcHeight;

    private float _elapsedTime;
    private bool _isInitialized;
    private bool _hasExploded;

    public void Initialize(
        RatController attacker,
        RatController primaryTarget,
        int attackRangeRadius,
        BombProjectileMoveType moveType,
        Vector3 startPosition,
        Vector3 targetPosition,
        float travelTime,
        float arcHeight)
    {
        if (attacker == null)
        {
            Debug.LogError($"{name}: BombProjectile Initialize 실패 - attacker가 Null입니다.");
            return;
        }

        _attacker = attacker;
        _primaryTarget = primaryTarget;
        _attackRangeRadius = attackRangeRadius;
        _moveType = moveType;
        _startPosition = startPosition;
        _targetPosition = targetPosition;
        _travelTime = Mathf.Max(0.01f, travelTime);
        _arcHeight = Mathf.Max(0f, arcHeight);

        transform.position = _startPosition;
        _elapsedTime = 0f;
        _isInitialized = true;
    }

    private void Update()
    {
        if (!_isInitialized || _hasExploded) return;

        _elapsedTime += Time.deltaTime;
        float t = Mathf.Clamp01(_elapsedTime / _travelTime);

        if (_moveType == BombProjectileMoveType.Direct)
        {
            transform.position = Vector3.Lerp(_startPosition, _targetPosition, t);
        }
        else
        {
            transform.position = CalculateArcPosition(_startPosition, _targetPosition, t, _arcHeight);
        }

        if(t >= 1f)
        {
            Explode();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_explodeOnTriggerEnter || _hasExploded || !_isInitialized) return;

        RatController hitTarget = other.GetComponent<RatController>();
        if (hitTarget == null) return;

        if (_attacker == null)
        {
            Debug.LogError($"{name}: OnTriggerEnter2D 실패 - attacker가 Null입니다.");
            return;
        }

        if(!_attacker.IsEnemy(hitTarget)) return;

        Explode();
    }

    private void Explode()
    {
        if (_hasExploded) return;

        _hasExploded = true;

        List<RatController> hitTargets = CollectHitTargetsByCellRadius();
        for(int i = 0; i < hitTargets.Count; i++)
        {
            RatController hitTarget = hitTargets[i];
            if (hitTarget == null) continue;

            RatDamageCalculator.ApplyCollisionDamage(_attacker, hitTarget);
        }

        Destroy(gameObject);
    }

    private List<RatController> CollectHitTargetsByCellRadius()
    {
        List<RatController> result = new List<RatController>();

        if(_attacker == null)
        {
            Debug.LogError($"{name}: CollectHitTargets 실패 - attacker가 Null입니다.");
            return result;
        }

        Vector2Int explosionCenterCell = ResolveExplosionCenterCell();
        List<Vector2Int> explosionCells = new List<Vector2Int> { explosionCenterCell };

        RatController[] allTargets = FindObjectsByType<RatController>(FindObjectsSortMode.None);

        if(allTargets == null || allTargets.Length == 0) return result;

        for(int i = 0; i < allTargets.Length; i++)
        {
            RatController target = allTargets[i];

            if(target == null) continue;

            if (!_attacker.IsEnemy(target)) continue;

            if(target.RatStatRuntime == null || target.RatStatRuntime.IsDead) continue;

            IReadOnlyList<Vector2Int> targetCells = target.GetOccupiedCells();

            if (targetCells == null || targetCells.Count == 0) continue;

            if(_attackRangeRadius <= 0)
            {
                if(GridRangeUtility.IsWithinCellRadius(explosionCells, targetCells, 0))
                {
                    result.Add(target);
                    break;
                }

                continue;
            }

            if(GridRangeUtility.IsWithinCellRadius(explosionCells, targetCells, _attackRangeRadius))
                result.Add(target);

            return result;
        }

        return result;
    }

    private Vector2Int ResolveExplosionCenterCell()
    {
        if(_primaryTarget != null)
        {
            IReadOnlyList<Vector2Int> targetCells = _primaryTarget.GetOccupiedCells();
            if(targetCells != null && targetCells.Count > 0)
            {
                return targetCells[0];
            }
        }

        return Vector2Int.RoundToInt(transform.position);
    }

    private Vector3 CalculateArcPosition(Vector3 start, Vector3 end, float t, float arcHeight)
    {
        Vector3 linear = Vector3.Lerp(start, end, t);
        float heightOffset = 4f * arcHeight * t * (1f - t);
        return linear + Vector3.up * heightOffset;
    }
}
