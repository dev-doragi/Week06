using System.Collections.Generic;
using UnityEngine;

public class RatController : MonoBehaviour, IPartRuntimeBindable
{
    [SerializeField] private PartData _partData;
    [SerializeField] private PlacedPart _placedPart;
    [SerializeField] private TeamType _teamType;

    private RatStatRuntime _ratStatRuntime;
    private RatAttackHandler _ratAttackHandler;
    private RatCollisionHandler _ratCollisionHandler;
    private RatTargetFinder _ratTargetFinder;
    private RatSupportHandler _ratSupportHandler;
    private RatStatModifierRuntime _ratStatModifierRuntime;
    private bool _isTutorialEnemy = false;

    public PartData PartData
    {
        get
        {
            // 주요 라인: 가능하면 PlacedPart가 가진 데이터(owner)를 우선 사용한다.
            if (_placedPart != null && _placedPart.data != null)
            {
                return _placedPart.data;
            }

            return _partData;
        }
    }

    public RatStatRuntime RatStatRuntime => _ratStatRuntime;
    public RatAttackHandler RatAttackHandler => _ratAttackHandler;
    public RatCollisionHandler RatCollisionHandler => _ratCollisionHandler;
    public RatTargetFinder RatTargetFinder => _ratTargetFinder;
    public RatSupportHandler RatSupportHandler => _ratSupportHandler;
    public RatStatModifierRuntime RatStatModifierRuntime => _ratStatModifierRuntime;
    public TeamType TeamType => _teamType;
    public PlacedPart PlacedPart => _placedPart;
    public bool IsTutorialEnemy
    {
        get => _isTutorialEnemy;
        set => _isTutorialEnemy = value;
    }

    private void Awake()
    {
        // 주요 라인: 같은 GameObject 내부 컴포넌트는 Awake에서 캐싱한다.
        _ratStatRuntime = GetComponent<RatStatRuntime>();
        _ratAttackHandler = GetComponent<RatAttackHandler>();
        _ratCollisionHandler = GetComponent<RatCollisionHandler>();
        _ratTargetFinder = GetComponent<RatTargetFinder>();
        _ratSupportHandler = GetComponent<RatSupportHandler>();
        _ratStatModifierRuntime = GetComponent<RatStatModifierRuntime>();

        if (_ratStatRuntime == null)
        {
            Debug.LogError($"{name}: RatStatRuntime 컴포넌트를 찾을 수 없습니다.");
            return;
        }

        if (_ratStatModifierRuntime == null)
        {
            Debug.LogError($"{name}: RatStatModifierRuntime 컴포넌트를 찾을 수 없습니다.");
        }
    }

    private void Start()
    {
        // 주요 라인: 런타임 바인딩 이후 최종 데이터 기준으로 StatRuntime을 초기화한다.
        if (PartData == null)
        {
            Debug.LogError($"{name}: RatController 초기화 실패 - PartData가 할당되지 않았습니다.");
            return;
        }

        if (_placedPart == null)
        {
            Debug.LogError($"{name}: RatController 초기화 실패 - PlacedPart가 할당되지 않았습니다.");
        }

        if (_teamType == TeamType.None)
        {
            Debug.LogError($"{name}: RatController 초기화 실패 - TeamType이 None입니다.");
        }

        _ratStatRuntime.SetPartData(PartData);
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

    public void BindRuntime(PartRuntimeContext context)
    {
        if (context.PlacedPart == null)
        {
            Debug.LogError($"{name}: BindRuntime 실패 - PlacedPart가 Null입니다.");
            return;
        }

        if (context.PartData == null)
        {
            Debug.LogError($"{name}: BindRuntime 실패 - PartData가 Null입니다.");
            return;
        }

        // 주요 라인: 런타임 생성 시 전달받은 배치 owner와 데이터를 연결한다.
        _placedPart = context.PlacedPart;
        _partData = context.PartData;
        _teamType = context.TeamType;

        if (_ratStatRuntime != null)
        {
            _ratStatRuntime.SetPartData(PartData);
        }
    }

    public bool IsUnit() => _ratStatRuntime != null && _ratStatRuntime.IsUnit();
    public bool IsBuilding() => _ratStatRuntime != null && _ratStatRuntime.IsBuilding();
    public bool IsAttackUnit() => _ratStatRuntime != null && _ratStatRuntime.IsAttackUnit();
    public bool IsDefenseUnit() => _ratStatRuntime != null && _ratStatRuntime.IsDefenseUnit();
    public bool IsSupportUnit() => _ratStatRuntime != null && _ratStatRuntime.IsSupportUnit();

    public bool CanBeCombatTarget()
    {
        return PartData != null && PartData.CanBeCombatTarget;
    }

    public float GetCurrentHp() => _ratStatRuntime != null ? _ratStatRuntime.CurrentHp : 0f;
    public float GetMaxHp() => _ratStatRuntime != null ? _ratStatRuntime.MaxHp : 0f;
    public float GetDefenseRate() => _ratStatRuntime != null ? _ratStatRuntime.DefenseRate : 0f;

    public int GetCost()
    {
        if (PartData == null)
        {
            Debug.LogError($"{name}: GetCost 실패 - PartData가 Null입니다.");
            return 0;
        }

        return PartData.CommonStat.Cost;
    }

    public bool CanUseAttack() => IsAttackUnit();
    public bool CanUseCollision() => IsDefenseUnit();
    public bool CanUseSupport()
    {
        return PartData != null && PartData.CanUseSupport;
    }

    public bool IsArcAttack()
    {
        if (PartData == null)
        {
            Debug.LogError($"{name}: IsArcAttack 실패 - PartData가 Null입니다.");
            return false;
        }

        return PartData.IsArcAttack;
    }

    public bool IsDirectAttack()
    {
        if (PartData == null)
        {
            Debug.LogError($"{name}: IsDirectAttack 실패 - PartData가 Null입니다.");
            return false;
        }

        return PartData.IsDirectAttack;
    }

    public bool IsAreaAttack()
    {
        if (PartData == null)
        {
            Debug.LogError($"{name}: IsAreaAttack 실패 - PartData가 Null입니다.");
            return false;
        }

        return PartData.IsAreaAttack;
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
        if (_partData.Key == 10001 || _partData.Key == 20001)
        {
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

    // 튜토리얼 전용: 이벤트 발행 없이 객체 제거/비활성화를 수행
    public void KillForTutorial()
    {
        // 기존 HandleDead()와 달리 EventBus 발행을 하지 않음
        if (_placedPart != null)
        {
            _placedPart.Break();
        }

        if (_ratAttackHandler != null)
            _ratAttackHandler.enabled = false;
        if (_ratCollisionHandler != null)
            _ratCollisionHandler.enabled = false;
        if (_ratTargetFinder != null)
            _ratTargetFinder.enabled = false;
        if (_ratSupportHandler != null)
            _ratSupportHandler.enabled = false;

        // 안전하게 GameObject 제거 또는 비활성화
        // 여기서는 즉시 제거: 튜토리얼 목적이므로 간결하게 Destroy
        Destroy(gameObject);
    }

    public bool IsEnemy(RatController other)
    {
        if (other == null)
        {
            Debug.LogError($"{name}: IsEnemy 실패 - other가 Null입니다.");
            return false;
        }

        if (_teamType == TeamType.None || other.TeamType == TeamType.None)
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
        _placedPart.Break();
        if(_ratAttackHandler != null)
            _ratAttackHandler.enabled = false;

        // Tutorial enemies should not trigger normal game events
        // IsTutorialEnemy 플래그 or 튜토리얼 씬 전체를 이중 방어
        if (_isTutorialEnemy || StageLoadContext.IsTutorial) return;

        if (_partData.BuildingType == BuildingType.Core)
            EventBus.Instance.Publish(new BaseDestroyedEvent());
        if(_partData.BuildingType == BuildingType.EnemyCore)
        {
            int addRat = StageManager.Instance.CurrentWaveIndex * 15 + StageManager.Instance.CurrentStageIndex * 20;
            for (int i = 0; i < 25 + addRat; i++)
            {
                GameObject spawned = PoolManager.Instance.Spawn("DropRat", transform.position, Quaternion.identity);
            }
            EventBus.Instance.Publish(new EnemyDefeatedEvent());
        }
    }
}