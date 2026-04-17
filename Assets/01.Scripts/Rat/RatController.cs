using UnityEngine;

public class RatController : MonoBehaviour
{
    [SerializeField] private RatData _ratData;
    [SerializeField] private RatTeamType _teamType;

    private RatStatRuntime _ratStatRuntime;
    private RatAttackHandler _ratAttackHandler;
    private RatCollisionHandler _ratCollisionHandler;
    private RatTargetFinder _ratTargetFinder;

    public RatData RatData => _ratData;
    public RatType RatType => _ratData != null ? _ratData.RatType : RatType.White;
    public RatStatRuntime RatStatRuntime => _ratStatRuntime;
    public RatAttackHandler RatAttackHandler => _ratAttackHandler;
    public RatCollisionHandler RatCollisionHandler => _ratCollisionHandler;
    public RatTargetFinder RatTargetFinder => _ratTargetFinder;
    public RatTeamType TeamType => _teamType;

    private void Awake()
    {
        _ratStatRuntime = GetComponent<RatStatRuntime>();
        if (_ratStatRuntime == null)
        {
            Debug.LogError($"{name}: RatStatRuntime 컴포넌트를 찾을 수 없습니다.");
            return;
        }

        _ratAttackHandler = GetComponent<RatAttackHandler>();
        _ratCollisionHandler = GetComponent<RatCollisionHandler>();
        _ratTargetFinder = GetComponent<RatTargetFinder>();

        if (_ratData == null)
        {
            Debug.LogError($"{name}: RatController에 RatData가 할당되지 않았습니다.");
            return;
        }

        if (_teamType == RatTeamType.None)
        {
            Debug.LogError($"{name}: TeamType이 None으로 설정되어 있습니다.");
        }

        _ratStatRuntime.SetRatData(_ratData);
    }

    private void OnEnable()
    {
        if (_ratStatRuntime != null)
        {
            _ratStatRuntime.OnDead += HandleDead;
        }
    }

    private void OnDisable()
    {
        if (_ratStatRuntime != null)
        {
            _ratStatRuntime.OnDead -= HandleDead;
        }
    }

    public float GetCurrentHp()
    {
        if (_ratStatRuntime == null)
        {
            Debug.LogError($"{name}: GetCurrentHp 실패 - RatStatRuntime이 Null입니다.");
            return 0f;
        }

        return _ratStatRuntime.CurrentHp;
    }

    public float GetMaxHp()
    {
        if (_ratStatRuntime == null)
        {
            Debug.LogError($"{name}: GetMaxHp 실패 - RatStatRuntime이 Null입니다.");
            return 0f;
        }

        return _ratStatRuntime.MaxHp;
    }

    public float GetDefenceRate()
    {
        if (_ratStatRuntime == null)
        {
            Debug.LogError($"{name}: GetDefenceRate 실패 - RatStatRuntime이 Null입니다.");
            return 0f;
        }

        return _ratStatRuntime.DefenceRate;
    }

    public int GetCost()
    {
        if (_ratData == null)
        {
            Debug.LogError($"{name}: GetCost 실패 - RatData가 Null입니다.");
            return 0;
        }

        if (_ratData.CommonStat == null)
        {
            Debug.LogError($"{name}: GetCost 실패 - CommonStat이 Null입니다.");
            return 0;
        }

        return _ratData.CommonStat.Cost;
    }

    public RatController GetCurrentTarget()
    {
        if (_ratAttackHandler == null)
        {
            Debug.LogError($"{name}: GetCurrentTarget 실패 - RatAttackHandler가 없습니다.");
            return null;
        }

        return _ratAttackHandler.CurrentTarget;
    }

    public void ProcessAutoAttack()
    {
        if (_ratAttackHandler == null)
        {
            Debug.LogError($"{name}: ProcessAutoAttack 실패 - RatAttackHandler가 없습니다.");
            return;
        }

        _ratAttackHandler.ProcessAutoAttack();
    }

    public void ClearCurrentTarget()
    {
        if (_ratAttackHandler == null)
        {
            Debug.LogError($"{name}: ClearCurrentTarget 실패 - RatAttackHandler가 없습니다.");
            return;
        }

        _ratAttackHandler.ClearCurrentTarget();
    }

    public bool TryGetAttackStat(out RatAttackStatData attackStat)
    {
        attackStat = null;

        if (_ratStatRuntime == null)
        {
            Debug.LogError($"{name}: TryGetAttackStat 실패 - RatStatRuntime이 Null입니다.");
            return false;
        }

        return _ratStatRuntime.TryGetAttackStat(out attackStat);
    }

    public bool TryGetDefenceStat(out RatDefenceStatData defenceStat)
    {
        defenceStat = null;

        if (_ratStatRuntime == null)
        {
            Debug.LogError($"{name}: TryGetDefenceStat 실패 - RatStatRuntime이 Null입니다.");
            return false;
        }

        return _ratStatRuntime.TryGetDefenceStat(out defenceStat);
    }

    public void ApplyDirectDamage(float damage)
    {
        if (_ratStatRuntime == null)
        {
            Debug.LogError($"{name}: ApplyDirectDamage 실패 - RatStatRuntime이 Null입니다.");
            return;
        }

        _ratStatRuntime.ApplyDirectDamage(damage);
    }

    public void RecoverHp(float amount)
    {
        if (_ratStatRuntime == null)
        {
            Debug.LogError($"{name}: RecoverHp 실패 - RatStatRuntime이 Null입니다.");
            return;
        }

        _ratStatRuntime.RecoverHp(amount);
    }

    public bool IsEnemy(RatController other)
    {
        if (other == null)
        {
            Debug.LogError($"{name}: IsEnemy 실패 - other가 Null입니다.");
            return false;
        }

        if (_teamType == RatTeamType.None || other.TeamType == RatTeamType.None)
        {
            Debug.LogError($"{name}: IsEnemy 실패 - TeamType이 None인 대상이 있습니다.");
            return false;
        }

        return _teamType != other.TeamType;
    }

    public bool TryAttack(RatController target)
    {
        if (_ratAttackHandler == null)
        {
            Debug.LogError($"{name}: TryAttack 실패 - RatAttackHandler가 없습니다.");
            return false;
        }

        return _ratAttackHandler.TryAttack(target);
    }

    public bool TryAttackNearestEnemy()
    {
        if (_ratAttackHandler == null)
        {
            Debug.LogError($"{name}: TryAttackNearestEnemy 실패 - RatAttackHandler가 없습니다.");
            return false;
        }

        return _ratAttackHandler.TryAttackNearestEnemy();
    }

    public bool TryCollide(RatController target)
    {
        if (_ratCollisionHandler == null)
        {
            Debug.LogError($"{name}: TryCollide 실패 - RatCollisionHandler가 없습니다.");
            return false;
        }

        return _ratCollisionHandler.TryCollide(target);
    }

    private void HandleDead()
    {
        Debug.Log($"{name}: Rat 사망 처리");
    }
}
