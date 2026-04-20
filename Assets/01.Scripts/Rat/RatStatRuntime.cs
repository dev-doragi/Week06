using UnityEngine;
using System;

public class RatStatRuntime : MonoBehaviour
{
    [SerializeField] private PartData _partData;

    private float _currentHp;
    private float _colorValue =1;

    //public PartData PartData => _partData;
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

        _colorValue = 0.5f + ((_currentHp / _partData.Hp) / 2f);
        Color color = new Color(1f, _colorValue, _colorValue, 1f);
        SetColor(color);

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
    public void SetColor(Color color)
    {
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = color;
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
        return _partData != null && _partData.IsUnit;
    }

    public bool IsBuilding()
    {
        return _partData != null && _partData.IsBuilding;
    }

    public bool IsAttackUnit()
    {
        return _partData != null && _partData.IsAttackUnit;
    }

    public bool IsDefenseUnit()
    {
        return _partData != null && _partData.IsDefenseUnit;
    }

    public bool IsSupportUnit()
    {
        return _partData != null && _partData.IsSupportUnit;
    }

    public bool CanUseAttack()
    {
        return IsAttackUnit();
    }

    public bool CanUseCollision()
    {
        return IsDefenseUnit();
    }

    public bool CanUseSupport()
    {
        return IsSupportUnit();
    }

    public bool TryGetAttackStat(out PartAttackStatData attackStat)
    {
        attackStat = null;

        if (_partData == null)
        {
            Debug.LogError($"{name}: TryGetAttackStat 실패 - PartData가 Null입니다.");
            return false;
        }

        if (!_partData.IsAttackUnit)
        {
            return false;
        }

        attackStat = _partData.AttackStat;
        if (attackStat == null)
        {
            Debug.LogError($"{name}: Attack 유닛인데 AttackStat이 Null입니다.");
            return false;
        }

        return true;
    }

    public bool TryGetDefenseStat(out PartDefenseStatData defenseStat)
    {
        defenseStat = null;

        if (_partData == null)
        {
            Debug.LogError($"{name}: TryGetDefenseStat 실패 - PartData가 Null입니다.");
            return false;
        }

        if (!_partData.IsDefenseUnit)
        {
            return false;
        }

        defenseStat = _partData.DefenseStat;
        if (defenseStat == null)
        {
            Debug.LogError($"{name}: Defense 유닛인데 DefenseStat이 Null입니다.");
            return false;
        }

        return true;
    }

    public bool TryGetSupportStat(out PartSupportStatData supportStat)
    {
        supportStat = null;

        if (_partData == null)
        {
            Debug.LogError($"{name}: TryGetSupportStat 실패 - PartData가 Null입니다.");
            return false;
        }

        if (!_partData.IsSupportUnit)
        {
            return false;
        }

        supportStat = _partData.SupportStat;
        if (supportStat == null)
        {
            Debug.LogError($"{name}: Support 유닛인데 SupportStat이 Null입니다.");
            return false;
        }

        return true;
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