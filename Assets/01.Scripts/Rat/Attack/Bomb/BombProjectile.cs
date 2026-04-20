using UnityEngine;
using System.Collections.Generic;

public enum BombProjectileMoveType
{
    Direct = 0,
    Arc = 1
}

public class BombProjectile : ProjectileBase
{
    [SerializeField] private bool _explodeOnTriggerEnter = true;
    [SerializeField] GameObject _bullet;
    [SerializeField] int _bulletCount;
    private RatController _attacker;
    private RatController _primaryTarget;
    private RatController _impactTarget;

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

        // 주요 라인: 풀 재사용을 고려해 상태를 매번 완전히 초기화한다.
        _attacker = attacker;
        _primaryTarget = primaryTarget;
        _impactTarget = null;

        _attackRangeRadius = attackRangeRadius;
        _moveType = moveType;

        _startPosition = startPosition;
        _targetPosition = targetPosition;
        _travelTime = Mathf.Max(0.01f, travelTime);
        _arcHeight = Mathf.Max(0f, arcHeight);

        _elapsedTime = 0f;
        _hasExploded = false;
        _isInitialized = true;

        transform.position = _startPosition;
    }

    private void Update()
    {
        if (!_isInitialized || _hasExploded)
        {
            return;
        }

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

        if (t >= 1f)
        {
            Explode();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_explodeOnTriggerEnter || _hasExploded || !_isInitialized)
        {
            return;
        }

        RatController hitTarget = other.GetComponent<RatController>();
        if (hitTarget == null)
        {
            return;
        }

        if (_attacker == null)
        {
            Debug.LogError($"{name}: OnTriggerEnter2D 실패 - attacker가 Null입니다.");
            return;
        }

        if (!_attacker.IsEnemy(hitTarget))
        {
            return;
        }

        if (hitTarget.RatStatRuntime == null || hitTarget.RatStatRuntime.IsDead)
        {
            return;
        }

        // 주요 라인: wheel은 직접 맞아도 전투 대상이 아니므로 무시한다.
        if (!hitTarget.CanBeCombatTarget())
        {
            return;
        }

        _impactTarget = hitTarget;
        Explode();
    }

    private void Explode()
    {
        if (_hasExploded)
        {
            return;
        }

        _hasExploded = true;

        List<RatController> hitTargets = CollectHitTargetsByCellRadius();
        for (int i = 0; i < hitTargets.Count; i++)
        {
            RatController hitTarget = hitTargets[i];
            if (hitTarget == null)
            {
                continue;
            }

            RatDamageCalculator.ApplyAttackDamage(_attacker, hitTarget);
        }

        // 주요 라인: ProjectileBase의 회수 로직을 사용한다.
        for (int i = 0; i < _bulletCount; i++)
        {
            SpawnBullet(_bullet.name);
        }
        Despawn();
    }

    private List<RatController> CollectHitTargetsByCellRadius()
    {
        List<RatController> result = new List<RatController>();

        if (_attacker == null)
        {
            Debug.LogError($"{name}: CollectHitTargets 실패 - attacker가 Null입니다.");
            return result;
        }

        Vector2Int explosionCenterCell = ResolveExplosionCenterCell();
        List<Vector2Int> explosionCells = new List<Vector2Int> { explosionCenterCell };

        RatController[] allTargets = FindObjectsByType<RatController>(FindObjectsSortMode.None);
        if (allTargets == null || allTargets.Length == 0)
        {
            return result;
        }

        for (int i = 0; i < allTargets.Length; i++)
        {
            RatController target = allTargets[i];
            if (target == null)
            {
                continue;
            }

            if (!_attacker.IsEnemy(target))
            {
                continue;
            }

            if (target.RatStatRuntime == null || target.RatStatRuntime.IsDead)
            {
                continue;
            }

            // 주요 라인: wheel은 폭발 반경 안에 있어도 피해 계산 대상이 아니다.
            if (!target.CanBeCombatTarget())
            {
                continue;
            }

            IReadOnlyList<Vector2Int> targetCells = target.GetOccupiedCells();
            if (targetCells == null || targetCells.Count == 0)
            {
                continue;
            }

            if (_attackRangeRadius <= 0)
            {
                if (GridRangeUtility.IsWithinCellRadius(explosionCells, targetCells, 0))
                {
                    result.Add(target);
                }

                continue;
            }

            if (GridRangeUtility.IsWithinCellRadius(explosionCells, targetCells, _attackRangeRadius))
            {
                result.Add(target);
            }
        }

        return result;
    }

    private Vector2Int ResolveExplosionCenterCell()
    {
        if (_impactTarget != null)
        {
            IReadOnlyList<Vector2Int> impactCells = _impactTarget.GetOccupiedCells();
            if (impactCells != null && impactCells.Count > 0)
            {
                return impactCells[0];
            }
        }

        if (_primaryTarget != null)
        {
            IReadOnlyList<Vector2Int> targetCells = _primaryTarget.GetOccupiedCells();
            if (targetCells != null && targetCells.Count > 0)
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

    protected override void Despawn()
    {
        // 주요 라인: 풀 반환 전에 내부 상태를 정리해 다음 재사용 시 꼬이지 않게 한다.
        _isInitialized = false;
        _hasExploded = false;
        _elapsedTime = 0f;

        _attacker = null;
        _primaryTarget = null;
        _impactTarget = null;

        base.Despawn();
    }
}