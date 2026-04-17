using UnityEngine;
using System;

public class RatStatRuntime : MonoBehaviour
{
    [SerializeField] private RatData _ratData;

    private float _currentHp;

    public RatData RatData => _ratData;
    public float CurrentHp => _currentHp;
    public float MaxHp => _ratData != null ? _ratData.CommonStat.Hp : 0f;
    public float DefenceRate => _ratData != null ? _ratData.CommonStat.DefenceRate : 0f;
    public bool IsDead => _currentHp <= 0f;

    public event Action<float, float> OnHpChanged;
    public event Action OnDead;

    private void Awake()
    {
        ValidateRatData();
        InitializeStat();
    }

    public void SetRatData(RatData ratData)
    {
        if (ratData == null)
        {
            Debug.LogError($"{name}: SetRatData 실패 - RatData가 Null입니다.");
            return;
        }

        _ratData = ratData;
        InitializeStat();
    }

    public void InitializeStat()
    {
        if (_ratData == null)
        {
            Debug.LogError($"{name}: InitializeStat 실패 - RatData가 Null입니다.");
            return;
        }

        _currentHp = _ratData.CommonStat.Hp;
        OnHpChanged?.Invoke(_currentHp, MaxHp);
    }

    public void ApplyDirectDamage(float damage)
    {
        if (_ratData == null)
        {
            Debug.LogError($"{name}: ApplyDirectDamage 실패 - RatData가 Null입니다.");
            return;
        }

        if (damage < 0f)
        {
            Debug.LogError($"{name}: ApplyDirectDamage 실패 - damage는 0 이상이어야 합니다. 입력값: {damage}");
            return;
        }

        if (IsDead)
        {
            return;
        }

        _currentHp -= damage;
        if( _currentHp < 0f)
        {
            _currentHp = 0f;
        }
        
        OnHpChanged?.Invoke(_currentHp, MaxHp);

        if (IsDead)
            OnDead?.Invoke();
    }

    public void RecoverHp(float amount)
    {
        if (_ratData == null)
        {
            Debug.LogError($"{name}: RecoverHp 실패 - RatData가 Null입니다.");
            return;
        }

        if (amount < 0f)
        {
            Debug.LogError($"{name}: RecoverHp 실패 - amount는 0 이상이어야 합니다. 입력값: {amount}");
            return;
        }

        if (IsDead)
        {
            return;
        }

        _currentHp += amount;
        if(_currentHp > MaxHp)
            _currentHp = MaxHp;

        OnHpChanged?.Invoke( _currentHp, MaxHp);
    }

    public bool TryGetAttackStat(out RatAttackStatData attackStat)
    {
        attackStat = null;

        if(_ratData == null)
        {
            Debug.LogError($"{name}: TryGetAttackStat 실패 - RatData가 Null입니다.");
            return false;
        }

        if (!_ratData.HasAttackStat)
        {
            return false;
        }

        attackStat = _ratData.AttackStat;
        return attackStat != null;
    }

    public bool TryGetDefenceStat(out RatDefenceStatData defenceStat)
    {
        defenceStat = null;

        if (_ratData == null)
        {
            Debug.LogError($"{name}: TryGetDefenceStat 실패 - RatData가 Null입니다.");
            return false;
        }

        if (!_ratData.HasDefenceStat)
        {
            return false;
        }

        defenceStat = _ratData.DefenceStat;
        return defenceStat != null;
    }

    private void ValidateRatData()
    {
        if (_ratData == null)
        {
            Debug.LogError($"{name}: RatStatRuntime에 RatData가 할당되지 않았습니다.");
        }
    }
}
