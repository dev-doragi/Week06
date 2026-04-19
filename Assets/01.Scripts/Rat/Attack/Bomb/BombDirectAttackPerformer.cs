using UnityEngine;

public class BombDirectAttackPerformer : BaseAttackPerformer
{
    [SerializeField] private BombProjectile _bombProjectilePrefab;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private float _travelTime = 0.35f;

    public override bool TryPerformAttack(RatController attacker, RatController target)
    {
        if (!ValidateAttackContext(attacker, target))
        {
            return false;
        }

        if (_bombProjectilePrefab == null)
        {
            Debug.LogError($"{name}: BombDirectAttackPerformer 실패 - _bombProjectilePrefab이 Null입니다.");
            return false;
        }

        if (!attacker.TryGetAttackStat(out var attackStat))
        {
            Debug.LogError($"{attacker.name}: AttackStat이 없어 직사 폭탄 공격을 수행할 수 없습니다.");
            return false;
        }

        Vector3 spawnPosition = _spawnPoint != null ? _spawnPoint.position : transform.position;
        Vector3 targetPosition = target.transform.position;

        GameObject spawned = PoolManager.Instance.Spawn(_bombProjectilePrefab.name, spawnPosition, Quaternion.identity);
        if (spawned == null)
        {
            Debug.LogError($"{name}: 폭탄 투사체 풀 스폰 실패 - {_bombProjectilePrefab.name}");
            return false;
        }

        BombProjectile projectile = spawned.GetComponent<BombProjectile>();
        if (projectile == null)
        {
            Debug.LogError($"{name}: 스폰된 오브젝트에 BombProjectile 컴포넌트가 없습니다.");
            PoolManager.Instance.Despawn(spawned);
            return false;
        }

        projectile.Initialize(
            attacker,
            target,
            attackStat.AttackRangeRadius,
            BombProjectileMoveType.Direct,
            spawnPosition,
            targetPosition,
            _travelTime,
            0f);

        return true;
    }
}