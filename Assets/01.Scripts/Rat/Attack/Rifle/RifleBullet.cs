using UnityEngine;

public class RifleBullet : ProjectileBase
{
    [SerializeField] GameObject _bullet;
    private RatController _attacker;
    private RatController _primaryTarget;
    private RatController _impactTarget;

    private Vector3 _startPosition;
    private Vector3 _targetPosition;
    private float _travelTime;
    private float _elapsedTime;

    private bool _isInitialized;
    private bool _hasHit;

    public void Initialize(
        RatController attacker,
        RatController primaryTarget,
        Vector3 startPosition,
        Vector3 targetPosition,
        float travelTime)
    {
        if (attacker == null)
        {
            Debug.LogError($"{name}: RifleBullet Initialize 실패 - attacker가 Null입니다.");
            return;
        }

        // 주요 라인: 풀 재사용 기준으로 상태를 매번 완전히 초기화한다.
        _attacker = attacker;
        _primaryTarget = primaryTarget;
        _impactTarget = null;

        _startPosition = startPosition;
        _targetPosition = targetPosition;
        _travelTime = Mathf.Max(0.01f, travelTime);
        _elapsedTime = 0f;

        _isInitialized = true;
        _hasHit = false;

        transform.position = _startPosition;
        transform.rotation = Quaternion.identity;
    }

    private void Update()
    {
        if (!_isInitialized || _hasHit)
        {
            return;
        }

        _elapsedTime += Time.deltaTime;
        float t = Mathf.Clamp01(_elapsedTime / _travelTime);

        transform.position = Vector3.Lerp(_startPosition, _targetPosition, t);

        if (t >= 1f)
        {
            ResolveFinalHitOrDespawn();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_isInitialized || _hasHit)
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

        if (!hitTarget.CanBeCombatTarget())
        {
            return;
        }

        if (hitTarget.RatStatRuntime == null || hitTarget.RatStatRuntime.IsDead)
        {
            return;
        }

        // 주요 라인: 목표가 아니더라도 먼저 맞은 적이 실제 피격 대상이 된다.
        _impactTarget = hitTarget;
        ApplyHitAndDespawn(_impactTarget);
    }

    private void ResolveFinalHitOrDespawn()
    {
        if (_hasHit)
        {
            return;
        }

        RatController finalTarget = _impactTarget != null ? _impactTarget : _primaryTarget;

        if (finalTarget == null)
        {
            Despawn();
            return;
        }

        if (_attacker == null)
        {
            Debug.LogError($"{name}: ResolveFinalHitOrDespawn 실패 - attacker가 Null입니다.");
            Despawn();
            return;
        }

        if (!_attacker.IsEnemy(finalTarget))
        {
            Despawn();
            return;
        }

        if (!finalTarget.CanBeCombatTarget())
        {
            Despawn();
            return;
        }

        if (finalTarget.RatStatRuntime == null || finalTarget.RatStatRuntime.IsDead)
        {
            Despawn();
            return;
        }

        ApplyHitAndDespawn(finalTarget);
    }

    private void ApplyHitAndDespawn(RatController hitTarget)
    {
        if (_hasHit)
        {
            return;
        }

        if (_attacker == null)
        {
            Debug.LogError($"{name}: ApplyHitAndDespawn 실패 - attacker가 Null입니다.");
            Despawn();
            return;
        }

        if (hitTarget == null)
        {
            Despawn();
            return;
        }

        _hasHit = true;

        // 주요 라인: 총알도 발사자의 현재 스탯 기준으로 공격 데미지를 계산한다.
        RatDamageCalculator.ApplyAttackDamage(_attacker, hitTarget);
        SpawnBullet(_bullet.name);
        Despawn();
    }

    protected override void Despawn()
    {
        // 주요 라인: 반납 전에 내부 상태를 확실히 정리한다.
        _isInitialized = false;
        _hasHit = false;
        _elapsedTime = 0f;

        _attacker = null;
        _primaryTarget = null;
        _impactTarget = null;

        base.Despawn();
    }


}