using UnityEngine;
using System;

public class RatStatRuntime : MonoBehaviour
{
    [SerializeField] private PartData _partData;

    private float _currentHp;

    public PartData PartData => _partData;
    public float CurrentHp => _currentHp;
    public float MaxHp => _partData != null ? _partData.CommonStat.Health : 0f;
    public float DefenseRate => _partData != null ? _partData.CommonStat.DefenseRate : 0f;
    public bool IsDead => _currentHp <= 0f;

    public event Action<float, float> OnHpChanged;
    public event Action OnDead;

    private void Awake()
    {
        ValidatePartData();
        InitializeStat();
    }

    public void SetPartData(PartData partData)
    {
        if (partData == null)
        {
            Debug.LogError($"{name}: SetPartData 실패 - PartData가 Null입니다.");
            return;
        }

        _partData = partData;
        InitializeStat();
    }

    public void InitializeStat()
    {
        if (_partData == null)
        {
            Debug.LogError($"{name}: InitializeStat 실패 - PartData가 Null입니다.");
            return;
        }

        _currentHp = _partData.CommonStat.Health;
        OnHpChanged?.Invoke(_currentHp, MaxHp);
    }

    public void ApplyDirectDamage(float damage)
    {
        if (_partData == null)
        {
            Debug.LogError($"{name}: ApplyDirectDamage 실패 - PartData가 Null입니다.");
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
        if (_currentHp < 0f)
        {
            _currentHp = 0f;
        }

        OnHpChanged?.Invoke(_currentHp, MaxHp);

        if (IsDead)
        {
            OnDead?.Invoke();
        }
    }

    public void RecoverHp(float amount)
    {
        if (_partData == null)
        {
            Debug.LogError($"{name}: RecoverHp 실패 - PartData가 Null입니다.");
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
        if (_currentHp > MaxHp)
        {
            _currentHp = MaxHp;
        }

        OnHpChanged?.Invoke(_currentHp, MaxHp);
    }

    public bool TryGetAttackStat(out PartAttackStatData attackStat)
    {
        attackStat = null;

        if (_partData == null)
        {
            Debug.LogError($"{name}: TryGetAttackStat 실패 - PartData가 Null입니다.");
            return false;
        }

        if (!_partData.HasAttackStat)
        {
            return false;
        }

        attackStat = _partData.AttackStat;
        return attackStat != null;
    }

    public bool TryGetDefenseStat(out PartDefenseStatData defenseStat)
    {
        defenseStat = null;

        if (_partData == null)
        {
            Debug.LogError($"{name}: TryGetDefenseStat 실패 - PartData가 Null입니다.");
            return false;
        }

        if (!_partData.HasDefenseStat)
        {
            return false;
        }

        defenseStat = _partData.DefenseStat;
        return defenseStat != null;
    }

    private void ValidatePartData()
    {
        if (_partData == null)
        {
            Debug.LogError($"{name}: RatStatRuntime에 PartData가 할당되지 않았습니다.");
            return;
        }

        _partData.IsValid();
    }
}