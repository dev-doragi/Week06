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

        BombProjectile projectile = Instantiate(_bombProjectilePrefab, spawnPosition, Quaternion.identity);
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