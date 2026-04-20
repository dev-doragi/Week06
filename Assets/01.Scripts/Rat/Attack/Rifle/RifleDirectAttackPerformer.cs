using UnityEngine;

public class RifleDirectAttackPerformer : BaseAttackPerformer
{
    [SerializeField] private string _bulletPoolName = "RifleBullet";
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private float _travelTime = 0.2f;

    public override bool TryPerformAttack(RatController attacker, RatController target)
    {
        if (!ValidateAttackContext(attacker, target))
        {
            return false;
        }

        if (PoolManager.Instance == null)
        {
            Debug.LogError($"{name}: PoolManager.Instance가 없습니다.");
            return false;
        }

        Vector3 startPosition = _spawnPoint != null ? _spawnPoint.position : transform.position;
        Vector3 targetPosition = target.transform.position;

        Debug.Log($"startPosition: {startPosition}, targetPosition: {targetPosition}");

        GameObject bulletObject = PoolManager.Instance.Spawn(
            _bulletPoolName,
            startPosition,
            Quaternion.identity);

        if (bulletObject == null)
        {
            Debug.LogError($"{name}: Spawn 실패 - poolName={_bulletPoolName}");
            return false;
        }

        RifleBullet bullet = bulletObject.GetComponent<RifleBullet>();
        if (bullet == null)
        {
            Debug.LogError($"{name}: Spawn된 오브젝트에 RifleBullet 컴포넌트가 없습니다.");
            PoolManager.Instance.Despawn(bulletObject);
            return false;
        }

        bullet.Initialize(
            attacker,
            target,
            startPosition,
            targetPosition,
            _travelTime);

        return true;
    }
}