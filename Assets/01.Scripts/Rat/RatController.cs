using UnityEngine;

public class RatController : MonoBehaviour
{
    [SerializeField] private RatData _ratData;

    private RatStatRuntime _ratStatRuntime;
    private RatAttackHandler _ratAttackHandler;
    private RatCollisionHandler _ratCollisionHandler;

    public RatData RatData => _ratData;
    public RatType RatType => _ratData != null ? _ratData.RatType : RatType.White;
    public RatStatRuntime RatStatRuntime => _ratStatRuntime;
    public RatAttackHandler RatAttackHandler => _ratAttackHandler;
    public RatCollisionHandler RatCollisionHandler => _ratCollisionHandler;

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

        if (_ratData == null)
        {
            Debug.LogError($"{name}: RatController에 RatData가 할당되지 않았습니다.");
            return;
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

    public bool TryAttack(RatController target)
    {
        if (_ratAttackHandler == null)
        {
            Debug.LogError($"{name}: TryAttack 실패 - RatAttackHandler가 없습니다.");
            return false;
        }

        return _ratAttackHandler.TryAttack(target);
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
