using UnityEngine;
using System;

public class RatStatRuntime : MonoBehaviour
{
    [SerializeField] private PartData _partData;

    private float _currentHp;

    public PartData PartData => _partData;
    public float CurrentHp => _currentHp;
    public float MaxHp => _partData != null ? _partData.CommonStat.Hp : 0f;
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

        _currentHp = _partData.CommonStat.Hp;
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

    public bool IsUnit()
    {
        if (_partData == null)
        {
            Debug.LogError($"{name}: IsUnit 실패 - PartData가 Null입니다.");
            return false;
        }

        return _partData.IsUnit;
    }

    public bool IsBuilding()
    {
        if (_partData == null)
        {
            Debug.LogError($"{name}: IsBuilding 실패 - PartData가 Null입니다.");
            return false;
        }

        return _partData.IsBuilding;
    }

    public bool IsAttackUnit()
    {
        if (_partData == null)
        {
            Debug.LogError($"{name}: IsAttackUnit 실패 - PartData가 Null입니다.");
            return false;
        }

        return _partData.IsAttackUnit;
    }

    public bool IsDefenseUnit()
    {
        if (_partData == null)
        {
            Debug.LogError($"{name}: IsDefenseUnit 실패 - PartData가 Null입니다.");
            return false;
        }

        return _partData.IsDefenseUnit;
    }

    public bool IsSupportUnit()
    {
        if (_partData == null)
        {
            Debug.LogError($"{name}: IsSupportUnit 실패 - PartData가 Null입니다.");
            return false;
        }

        return _partData.IsSupportUnit;
    }

    public bool CanUseAttack()
    {
        if(_partData == null)
        {
            Debug.LogError($"{name}: CanUseAttack 실패 - PartData가 Null입니다.");
            return false;
        }

        return _partData.CanUseAttack;
    }

    public bool CanUseCollision()
    {
        if (_partData == null)
        {
            Debug.LogError($"{name}: CanUseCollision 실패 - PartData가 Null입니다.");
            return false;
        }

        return _partData.CanUseCollision;
    }

    public bool CanUseSupport()
    {
        if (_partData == null)
        {
            Debug.LogError($"{name}: CanUseSupport 실패 - PartData가 Null입니다.");
            return false;
        }

        return _partData.CanUseSupport;
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

    public bool TryGetSupportStat(out PartSupportStatData supportStat)
    {
        supportStat = null;

        if (_partData == null)
        {
            Debug.LogError($"{name}: TryGetSupportStat 실패 - PartData가 Null입니다.");
            return false;
        }

        if (!_partData.CanUseSupport)
        {
            return false;
        }

        supportStat = _partData.SupportStat;
        return supportStat != null;
    }

    public float GetEffectiveDefenseRate(RatStatModifierRuntime modifierRuntime)
    {
        if (_partData == null)
        {
            Debug.LogError($"{name}: GetEffectiveDefenseRate 실패 - PartData가 Null입니다.");
            return 0f;
        }

        float baseValue = _partData.CommonStat.DefenseRate;

        if (modifierRuntime == null)
        {
            return baseValue;
        }

        float finalValue = baseValue;
        finalValue += modifierRuntime.DefenseRateFlatBonus;
        finalValue += baseValue * modifierRuntime.DefenseRatePercentBonus;

        return Mathf.Clamp01(finalValue);
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