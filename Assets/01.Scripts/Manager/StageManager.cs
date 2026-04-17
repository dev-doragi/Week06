using System;
using UnityEngine;

[DefaultExecutionOrder(-99)]
public class StageManager : Singleton<StageManager>
{
    [Header("Stage Settings")]
    [SerializeField] private BaseStage[] _stagePrefabs;
    [SerializeField] private Transform _stageParent;

    private BaseStage _currentStage;
    private EventBus _eventBus;
    private PoolManager _poolManager;

    public int CurrentStageIndex { get; private set; } = 0;
    public BaseStage CurrentStage => _currentStage;

    public static event Action<int> OnStageLoaded;
    public static event Action<int> OnStageStarted;
    public static event Action<int> OnStageCleared;
    public static event Action<int> OnStageCleanedUp;

    protected override void Init()
    {
        if (!ManagerRegistry.TryGet(out _eventBus))
            Debug.LogError("[StageManager] EventBus를 Registry에서 찾을 수 없습니다.");

        if (!ManagerRegistry.TryGet(out _poolManager))
            Debug.LogWarning("[StageManager] PoolManager를 찾을 수 없습니다. 풀 정리가 생략됩니다.");

        if (_stageParent == null)
        {
            _stageParent = new GameObject("StageContainer").transform;
        }
    }

    /// <summary>
    /// 다음 스테이지로 넘어갑니다.
    /// </summary>
    public void LoadNextStage()
    {
        int nextIndex = CurrentStageIndex + 1;

        if (nextIndex < _stagePrefabs.Length)
        {
            LoadStage(nextIndex);
        }
        else
        {
            GameManager.Instance.ChangeState(GameState.GameClear);
        }
    }

    /// <summary>
    /// 특정 스테이지를 로드합니다. 로드 전 기존 스테이지와 풀링된 객체들을 정리합니다.
    /// </summary>
    public void LoadStage(int stageIndex)
    {
        if (stageIndex < 0 || stageIndex >= _stagePrefabs.Length)
        {
            Debug.LogError($"[StageManager] 유효하지 않은 스테이지 인덱스: {stageIndex}");
            return;
        }

        // 1. 기존 스테이지 및 풀 매니저 정리
        ClearCurrentStage();

        // 2. 프리팹 생성 및 배치
        CurrentStageIndex = stageIndex;
        _currentStage = Instantiate(_stagePrefabs[CurrentStageIndex], _stageParent);

        if (_currentStage == null)
        {
            Debug.LogError($"[StageManager] Stage {stageIndex} 프리팹 생성 실패.");
            return;
        }

        // 3. 스테이지 초기화 (데이터 셋업)
        _currentStage.SetupStage(CurrentStageIndex);

        // 4. 이벤트 발행 (Action & EventBus)
        OnStageLoaded?.Invoke(CurrentStageIndex);
        _eventBus?.Publish(new StageLoadedEvent { StageIndex = CurrentStageIndex });

        Debug.Log($"[StageManager] Stage {stageIndex} Loaded.");
    }

    /// <summary>
    /// 로드된 스테이지의 실제 플레이(전투 등)를 시작합니다.
    /// </summary>
    public void PlayStage()
    {
        if (_currentStage == null) return;

        _currentStage.StartStage();

        OnStageStarted?.Invoke(CurrentStageIndex);
        _eventBus?.Publish(new StageStartedEvent { StageIndex = CurrentStageIndex });
    }

    /// <summary>
    /// 스테이지 클리어 판정 시 호출됩니다.
    /// </summary>
    public void NotifyStageCleared()
    {
        OnStageCleared?.Invoke(CurrentStageIndex);
        _eventBus?.Publish(new StageClearedEvent { StageIndex = CurrentStageIndex });
    }

    /// <summary>
    /// 현재 스테이지 인스턴스를 제거하고, PoolManager에 쌓인 모든 객체를 정리합니다.
    /// </summary>
    public void ClearCurrentStage()
    {
        if (_currentStage != null)
        {
            _currentStage.CleanupStage();
            Destroy(_currentStage.gameObject);
            _currentStage = null;
        }

        // 풀링된 모든 유닛/투사체 제거 (원씬 전략의 핵심)
        _poolManager?.ClearAllPools();

        OnStageCleanedUp?.Invoke(CurrentStageIndex);
        _eventBus?.Publish(new StageCleanedUpEvent { StageIndex = CurrentStageIndex });
    }
}