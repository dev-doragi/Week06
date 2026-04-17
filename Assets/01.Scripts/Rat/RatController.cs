using System.Collections.Generic;
using UnityEngine;

public class RatController : MonoBehaviour
{
    [SerializeField] private PartData _partData;
    [SerializeField] private PlacedPart _placedPart;
    [SerializeField] private RatTeamType _teamType;

    private RatStatRuntime _ratStatRuntime;
    private RatAttackHandler _ratAttackHandler;
    private RatCollisionHandler _ratCollisionHandler;
    private RatTargetFinder _ratTargetFinder;
    private RatSupportHandler _ratSupportHandler;
    private RatStatModifierRuntime _ratStatModifierRuntime;

    public PartData PartData => _partData;
    public RatStatRuntime RatStatRuntime => _ratStatRuntime;
    public RatAttackHandler RatAttackHandler => _ratAttackHandler;
    public RatCollisionHandler RatCollisionHandler => _ratCollisionHandler;
    public RatTargetFinder RatTargetFinder => _ratTargetFinder;
    public RatSupportHandler RatSupportHandler => _ratSupportHandler;
    public RatStatModifierRuntime RatStatModifierRuntime => _ratStatModifierRuntime;
    public RatTeamType TeamType => _teamType;
    public PlacedPart PlacedPart => _placedPart;

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
        _ratSupportHandler = GetComponent<RatSupportHandler>();
        _ratStatModifierRuntime = GetComponent<RatStatModifierRuntime>();

        if (_partData == null)
        {
            Debug.LogError($"{name}: RatController에 PartData가 할당되지 않았습니다.");
            return;
        }

        if (_placedPart == null)
        {
            Debug.LogError($"{name}: RatController에 PlacedPart가 할당되지 않았습니다.");
        }

        if (_teamType == RatTeamType.None)
        {
            Debug.LogError($"{name}: TeamType이 None으로 설정되어 있습니다.");
        }

        if (_ratStatModifierRuntime == null)
        {
            Debug.LogError($"{name}: RatStatModifierRuntime 컴포넌트를 찾을 수 없습니다.");
        }

        _ratStatRuntime.SetPartData(_partData);
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

    public bool IsUnit() => _ratStatRuntime != null && _ratStatRuntime.IsUnit();
    public bool IsBuilding() => _ratStatRuntime != null && _ratStatRuntime.IsBuilding();
    public bool IsAttackUnit() => _ratStatRuntime != null && _ratStatRuntime.IsAttackUnit();
    public bool IsDefenseUnit() => _ratStatRuntime != null && _ratStatRuntime.IsDefenseUnit();
    public bool IsSupportUnit() => _ratStatRuntime != null && _ratStatRuntime.IsSupportUnit();

    public float GetCurrentHp() => _ratStatRuntime != null ? _ratStatRuntime.CurrentHp : 0f;
    public float GetMaxHp() => _ratStatRuntime != null ? _ratStatRuntime.MaxHp : 0f;
    public float GetDefenseRate() => _ratStatRuntime != null ? _ratStatRuntime.DefenseRate : 0f;

    public int GetCost()
    {
        if (_partData == null)
        {
            Debug.LogError($"{name}: GetCost 실패 - PartData가 Null입니다.");
            return 0;
        }

        return _partData.CommonStat.Cost;
    }

    public bool CanUseAttack() => IsAttackUnit();
    public bool CanUseCollision() => IsDefenseUnit();
    public bool CanUseSupport() => IsSupportUnit();

    public bool IsArcAttack()
    {
        if (_partData == null)
        {
            Debug.LogError($"{name}: IsArcAttack 실패 - PartData가 Null입니다.");
            return false;
        }

        return _partData.IsArcAttack;
    }

    public bool IsDirectAttack()
    {
        if (_partData == null)
        {
            Debug.LogError($"{name}: IsDirectAttack 실패 - PartData가 Null입니다.");
            return false;
        }

        return _partData.IsDirectAttack;
    }

    public bool IsAreaAttack()
    {
        if (_partData == null)
        {
            Debug.LogError($"{name}: IsAreaAttack 실패 - PartData가 Null입니다.");
            return false;
        }

        return _partData.IsAreaAttack;
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

    public bool TryGetAttackStat(out PartAttackStatData attackStat)
    {
        attackStat = null;

        if (_ratStatRuntime == null)
        {
            Debug.LogError($"{name}: TryGetAttackStat 실패 - RatStatRuntime이 Null입니다.");
            return false;
        }

        return _ratStatRuntime.TryGetAttackStat(out attackStat);
    }

    public bool TryGetDefenseStat(out PartDefenseStatData defenseStat)
    {
        defenseStat = null;

        if (_ratStatRuntime == null)
        {
            Debug.LogError($"{name}: TryGetDefenseStat 실패 - RatStatRuntime이 Null입니다.");
            return false;
        }

        return _ratStatRuntime.TryGetDefenseStat(out defenseStat);
    }

    public bool TryGetSupportStat(out PartSupportStatData supportStat)
    {
        supportStat = null;

        if (_ratStatRuntime == null)
        {
            Debug.LogError($"{name}: TryGetSupportStat 실패 - RatStatRuntime이 Null입니다.");
            return false;
        }

        return _ratStatRuntime.TryGetSupportStat(out supportStat);
    }

    public void ProcessSupport()
    {
        if (_ratSupportHandler == null)
        {
            Debug.LogError($"{name}: ProcessSupport 실패 - RatSupportHandler가 없습니다.");
            return;
        }

        _ratSupportHandler.ProcessSupport();
    }

    public RatStatModifierRuntime GetStatModifierRuntime()
    {
        if (_ratStatModifierRuntime == null)
        {
            Debug.LogError($"{name}: GetStatModifierRuntime 실패 - RatStatModifierRuntime이 Null입니다.");
            return null;
        }

        return _ratStatModifierRuntime;
    }

    public IReadOnlyList<Vector2Int> GetOccupiedCells()
    {
        if (_placedPart == null)
        {
            Debug.LogError($"{name}: GetOccupiedCells 실패 - PlacedPart가 Null입니다.");
            return null;
        }

        return _placedPart.OccupiedCells;
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
        Debug.Log($"{name}: Part 사망 처리");
    }
}