using System.Collections.Generic;
using UnityEngine;

public class RatAttackHandler : MonoBehaviour
{
    [SerializeField] private bool _useAutoAttack = true;

    [Header("Legacy / Fallback")]
    [SerializeField] private MonoBehaviour _attackPerformerComponent;

    [Header("Trajectory Based Performer")]
    [SerializeField] private MonoBehaviour _arcAttackPerformerComponent;
    [SerializeField] private MonoBehaviour _directAttackPerformerComponent;

    [Header("Animator")]
    [SerializeField] private Animator anim;

    private RatController _ratController;
    private RatTargetFinder _ratTargetFinder;
    private RatController _currentTarget;
    private float _lastAttackTime;

    private IAttackPerformer _fallbackAttackPerformer;
    private IAttackPerformer _arcAttackPerformer;
    private IAttackPerformer _directAttackPerformer;
    private IAttackPerformer attackPerformer;
    public bool UseAutoAttack => _useAutoAttack;
    public RatController CurrentTarget => _currentTarget;

    public bool CanAttack
    {
        get
        {
            if (_ratController == null)
            {
                Debug.LogError($"{name}: CanAttack 확인 실패 - RatController가 Null입니다.");
                return false;
            }

            if (!_ratController.IsAttackUnit())
            {
                return false;
            }

            if (!_ratController.TryGetAttackStat(out var attackStat))
            {
                Debug.LogError($"{name}: Attack 유닛인데 AttackStat을 가져오지 못했습니다.");
                return false;
            }

            if(Time.time >= _lastAttackTime + GetAttackInterval(attackStat.AttackSpeed))
            {
                anim.SetBool("OnAttack", true);
                return true;
            }
            else
            {
                anim.SetBool("OnAttack", false);
                return false;
            }
        }
    }

    private void Awake()
    {
        _ratController = GetComponent<RatController>();
        if (_ratController == null)
        {
            Debug.LogError($"{name}: RatAttackHandler에 RatController가 없습니다.");
            return;
        }

        _ratTargetFinder = GetComponent<RatTargetFinder>();
        if (_ratTargetFinder == null)
        {
            Debug.LogError($"{name}: RatAttackHandler에 RatTargetFinder가 없습니다.");
        }

        CacheAttackPerformers();
    }

    private void Update()
    {
        if (!_useAutoAttack)
        {
            return;
        }

        ProcessAutoAttack();
    }

    public void ProcessAutoAttack()
    {
        if (_ratController == null)
        {
            Debug.LogError($"{name}: ProcessAutoAttack 실패 - RatController가 Null입니다.");
            return;
        }

        if (_ratTargetFinder == null)
        {
            Debug.LogError($"{name}: ProcessAutoAttack 실패 - RatTargetFinder가 Null입니다.");
            return;
        }

        if (!_ratController.IsAttackUnit())
        {
            return;
        }

        MaintainOrAcquireTarget();

        if (_currentTarget == null)
        {
            return;
        }

        if (!CanAttack)
        {
            return;
        }
        if (!TryAttack(_currentTarget))
        {
            InvalidateTargetIfNeeded(_currentTarget);
        }
    }

    public bool TryAttackNearestEnemy()
    {
        if (_ratTargetFinder == null)
        {
            Debug.LogError($"{name}: TryAttackNearestEnemy 실패 - RatTargetFinder가 Null입니다.");
            return false;
        }

        if (!_ratController.IsAttackUnit())
        {
            return false;
        }

        RatController target = _ratTargetFinder.FindNearestEnemy();
        if (target == null)
        {
            return false;
        }

        _currentTarget = target;
        return TryAttack(target);
    }

    public bool TryAttack(RatController target)
    {
        if (_ratController == null)
        {
            Debug.LogError($"{name}: TryAttack 실패 - RatController가 Null입니다.");
            return false;
        }

        if (target == null)
        {
            Debug.LogError($"{name}: TryAttack 실패 - target이 Null입니다.");
            return false;
        }

        if (!_ratController.IsAttackUnit())
        {
            return false;
        }

        attackPerformer = ResolveAttackPerformer();
        if (attackPerformer == null)
        {
            Debug.LogError($"{name}: TryAttack 실패 - TrajectoryType에 맞는 공격 실행기가 없습니다.");
            return false;
        }

        if (!_ratController.IsEnemy(target))
        {
            return false;
        }

        if (!_ratController.TryGetAttackStat(out var attackStat))
        {
            Debug.LogError($"{name}: Attack 유닛인데 AttackStat을 가져오지 못했습니다.");
            return false;
        }

        if (target.RatStatRuntime == null)
        {
            Debug.LogError($"{target.name}: RatStatRuntime이 없어 공격 대상이 될 수 없습니다.");
            return false;
        }

        if (target.RatStatRuntime.IsDead)
        {
            return false;
        }

        if (!_ratTargetFinder.IsValidTarget(target))
        {
            return false;
        }

        if (!CanAttack)
        {
            return false;
        }
        if(_ratController.TeamType == TeamType.Player)
        {
            if (!IsTargetInAttackDistance(target, attackStat.AttackDistance))
                return false;

        }
        if(anim.GetBool("OnAttack") && PlacementManager.Instance.CurrentMouse == 0)
            anim.SetBool("OnAttack", false);


        return true;
    }
    public void AnimAttack()
    {
        if (_ratController.TeamType == TeamType.Player)
        {
            if (!PlacementManager.Instance.SubtractMouseCount(1))
            {
                anim.SetBool("OnAttack", false);
                return;
            }
        }

        bool launched = attackPerformer.TryPerformAttack(_ratController, _currentTarget);
        if (!launched)
        {
            return;
        }
        anim.SetBool("OnAttack", false);
        _lastAttackTime = Time.time;
    }
    public void AnimStop()
    {
        anim.SetBool("OnAttack", false);
    }

    public bool HasValidCurrentTarget()
    {
        if (_currentTarget == null)
        {
            return false;
        }

        if (_ratTargetFinder == null)
        {
            Debug.LogError($"{name}: HasValidCurrentTarget 실패 - RatTargetFinder가 Null입니다.");
            return false;
        }

        if (!_ratController.IsAttackUnit())
        {
            return false;
        }

        if (!_ratTargetFinder.IsValidTarget(_currentTarget))
        {
            return false;
        }

        if (!_ratController.TryGetAttackStat(out var attackStat))
        {
            Debug.LogError($"{name}: Attack 유닛인데 AttackStat을 가져오지 못했습니다.");
            return false;
        }

        if (!IsTargetInAttackDistance(_currentTarget, attackStat.AttackDistance))
        {
            return false;
        }

        return true;
    }

    public void MaintainOrAcquireTarget()
    {
        if (HasValidCurrentTarget())
        {
            return;
        }

        _currentTarget = AcquireNewTarget();
    }

    public RatController AcquireNewTarget()
    {
        if (_ratTargetFinder == null)
        {
            Debug.LogError($"{name}: AcquireNewTarget 실패 - RatTargetFinder가 Null입니다.");
            return null;
        }

        return _ratTargetFinder.FindNearestEnemy();
    }

    public void ClearCurrentTarget()
    {
        _currentTarget = null;
    }

    public bool IsTargetInAttackDistance(RatController target, float attackDistance)
    {
        if (target == null)
        {
            Debug.LogError($"{name}: IsTargetInAttackDistance 실패 - target이 Null입니다.");
            return false;
        }

        if (_ratController == null)
        {
            Debug.LogError($"{name}: IsTargetInAttackDistance 실패 - RatController가 Null입니다.");
            return false;
        }

        if (attackDistance < 0f)
        {
            Debug.LogError($"{name}: IsTargetInAttackDistance 실패 - attackDistance는 0 이상이어야 합니다. 입력값: {attackDistance}");
            return false;
        }

        IReadOnlyList<Vector2Int> attackerCells = _ratController.GetOccupiedCells();
        IReadOnlyList<Vector2Int> targetCells = target.GetOccupiedCells();

        if (attackerCells == null)
        {
            Debug.LogError($"{name}: IsTargetInAttackDistance 실패 - attackerCells가 Null입니다.");
            return false;
        }

        if (targetCells == null)
        {
            Debug.LogError($"{target.name}: IsTargetInAttackDistance 실패 - targetCells가 Null입니다.");
            return false;
        }

        return GridRangeUtility.IsWithinAttackDistance(attackerCells, targetCells, attackDistance);
    }

    public int GetAttackRangeRadius()
    {
        if (_ratController == null)
        {
            Debug.LogError($"{name}: GetAttackRangeRadius 실패 - RatController가 Null입니다.");
            return 0;
        }

        if (!_ratController.IsAttackUnit())
        {
            return 0;
        }

        if (!_ratController.TryGetAttackStat(out var attackStat))
        {
            Debug.LogError($"{name}: Attack 유닛인데 AttackStat을 가져오지 못했습니다.");
            return 0;
        }

        return attackStat.AttackRangeRadius;
    }

    private void InvalidateTargetIfNeeded(RatController target)
    {
        if (target == null)
        {
            ClearCurrentTarget();
            return;
        }

        if (_currentTarget != target)
        {
            return;
        }

        if (!HasValidCurrentTarget())
        {
            ClearCurrentTarget();
        }
    }

    private float GetAttackInterval(float attackSpeed)
    {
        if (_ratController == null)
        {
            Debug.LogError($"{name}: GetAttackInterval 실패 - RatController가 Null입니다.");
            return float.MaxValue;
        }

        RatStatModifierRuntime modifierRuntime = _ratController.GetStatModifierRuntime();

        float finalAttackSpeed = attackSpeed;
        if (modifierRuntime != null)
        {
            finalAttackSpeed += modifierRuntime.AttackSpeedFlatBonus;
            finalAttackSpeed += attackSpeed * modifierRuntime.AttackSpeedPercentBonus;
        }

        if (finalAttackSpeed <= 0f)
        {
            Debug.LogError($"{name}: GetAttackInterval 실패 - 최종 attackSpeed는 0보다 커야 합니다. 입력값: {finalAttackSpeed}");
            return float.MaxValue;
        }

        return 1f / finalAttackSpeed;
    }

    private void CacheAttackPerformers()
    {
        _fallbackAttackPerformer = _attackPerformerComponent as IAttackPerformer;
        _arcAttackPerformer = _arcAttackPerformerComponent as IAttackPerformer;
        _directAttackPerformer = _directAttackPerformerComponent as IAttackPerformer;

        if (_attackPerformerComponent != null && _fallbackAttackPerformer == null)
        {
            Debug.LogError($"{name}: _attackPerformerComponent가 IAttackPerformer를 구현하지 않았습니다.");
        }

        if (_arcAttackPerformerComponent != null && _arcAttackPerformer == null)
        {
            Debug.LogError($"{name}: _arcAttackPerformerComponent가 IAttackPerformer를 구현하지 않았습니다.");
        }

        if (_directAttackPerformerComponent != null && _directAttackPerformer == null)
        {
            Debug.LogError($"{name}: _directAttackPerformerComponent가 IAttackPerformer를 구현하지 않았습니다.");
        }

        if (_attackPerformerComponent == null &&
            _arcAttackPerformerComponent == null &&
            _directAttackPerformerComponent == null)
        {
            Debug.LogError($"{name}: RatAttackHandler에 공격 실행기 컴포넌트가 하나도 할당되지 않았습니다.");
        }
    }

    private IAttackPerformer ResolveAttackPerformer()
    {
        if (_ratController == null)
        {
            Debug.LogError($"{name}: ResolveAttackPerformer 실패 - RatController가 Null입니다.");
            return null;
        }

        PartData partData = _ratController.PartData;
        if (partData == null)
        {
            Debug.LogError($"{name}: ResolveAttackPerformer 실패 - PartData가 Null입니다.");
            return _fallbackAttackPerformer;
        }

        if (partData.IsArcAttack)
        {
            if (_arcAttackPerformer != null)
            {
                return _arcAttackPerformer;
            }

            Debug.LogWarning($"{name}: TrajectoryType은 Arc인데 Arc 공격 실행기가 없습니다. fallback을 사용합니다.");
            return _fallbackAttackPerformer;
        }

        if (partData.IsDirectAttack)
        {
            if (_directAttackPerformer != null)
            {
                return _directAttackPerformer;
            }

            Debug.LogWarning($"{name}: TrajectoryType은 Direct인데 Direct 공격 실행기가 없습니다. fallback을 사용합니다.");
            return _fallbackAttackPerformer;
        }

        Debug.LogWarning($"{name}: TrajectoryType에 맞는 공격 실행기를 찾지 못했습니다. fallback을 사용합니다.");
        return _fallbackAttackPerformer;
    }
}