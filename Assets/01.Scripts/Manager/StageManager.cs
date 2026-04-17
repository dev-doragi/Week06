using System;
using UnityEngine;

public class StageManager : Singleton<StageManager>
{
    [Header("Stage Settings")]
    [SerializeField] private StageDataSO[] _stageDatas;
    [SerializeField] private Transform _stageParent;

    private StageLayout _currentLayout;
    private PoolManager _poolManager;

    public int CurrentStageIndex { get; private set; } = 0;

    public StageDataSO CurrentStageData => _stageDatas[CurrentStageIndex];
    public StageLayout CurrentLayout => _currentLayout;

    public static event Action<int> OnStageLoaded;
    public static event Action<int> OnStageStarted;
    public static event Action<int> OnStageCleared;
    public static event Action<int> OnStageCleanedUp;

    protected override void Init()
    {
        if (_stageParent == null)
        {
            _stageParent = new GameObject("StageContainer").transform;
        }

        if (!ManagerRegistry.TryGet(out _poolManager))
            Debug.LogWarning("[StageManager] PoolManager를 찾을 수 없습니다.");
    }

    private void Start()
    {
        // 디버깅용
        LoadStage(0);
        PlayStage();
    }

    /// <summary>
    /// 다음 스테이지로 넘어갑니다.
    /// </summary>
    public void LoadNextStage()
    {
        int nextIndex = CurrentStageIndex + 1;

        if (nextIndex < _stageDatas.Length)
        {
            LoadStage(nextIndex);
        }
        else
        {
            GameManager.Instance.ChangeState(GameState.GameClear);
            Debug.Log("[StageManager] 모든 스테이지 클리어!");
        }
    }

    /// <summary>
    /// 특정 스테이지를 로드합니다. 로드 전 기존 레이아웃과 풀을 정리합니다.
    /// </summary>
    public void LoadStage(int stageIndex)
    {
        if (stageIndex < 0 || stageIndex >= _stageDatas.Length)
        {
            Debug.LogError($"[StageManager] 유효하지 않은 스테이지 인덱스: {stageIndex}");
            return;
        }

        ClearCurrentStage();

        CurrentStageIndex = stageIndex;
        StageDataSO nextData = _stageDatas[CurrentStageIndex];

        PrewarmStagePools(nextData);

        if (nextData.StageLayoutPrefab != null)
        {
            _currentLayout = Instantiate(nextData.StageLayoutPrefab, _stageParent);
        }
        else
        {
            Debug.LogWarning($"[StageManager] Stage {stageIndex}의 StageLayoutPrefab이 비어있습니다.");
        }

        OnStageLoaded?.Invoke(CurrentStageIndex);
        EventBus.Instance.Publish(new StageLoadedEvent { StageIndex = CurrentStageIndex });

        Debug.Log($"[StageManager] Stage {stageIndex} 로드 완료 (데이터 기반).");
    }

    /// <summary>
    /// SO 데이터에 기록된 풀 세팅 정보를 바탕으로 풀을 생성합니다.
    /// </summary>
    private void PrewarmStagePools(StageDataSO data)
    {
        if (_poolManager == null || data.PoolSetupList == null) return;

        foreach (var setupData in data.PoolSetupList)
        {
            if (setupData.Prefab != null)
            {
                _poolManager.CreatePool(setupData.Prefab, setupData.InitialSize, setupData.MaxSize);
            }
        }
    }

    /// <summary>
    /// 로드된 스테이지의 실제 플레이(웨이브 등)를 시작합니다.
    /// </summary>
    public void PlayStage()
    {
        // TODO: 스테이지 시작 할 때 처리 필요함

        OnStageStarted?.Invoke(CurrentStageIndex);
        EventBus.Instance.Publish(new StageStartedEvent { StageIndex = CurrentStageIndex });
        Debug.Log("Stage Start");
    }

    /// <summary>
    /// 스테이지 클리어 판정 시 호출됩니다.
    /// </summary>
    public void NotifyStageCleared()
    {
        // 이 이벤트가 터질 때 VehicleSaveLoader가 현재 맵의 아군 병기 정보를 저장(Cache)합니다.
        OnStageCleared?.Invoke(CurrentStageIndex);
        EventBus.Instance.Publish(new StageClearedEvent { StageIndex = CurrentStageIndex });
    }

    /// <summary>
    /// 현재 스테이지 인스턴스를 제거하고, PoolManager에 쌓인 모든 객체를 정리합니다.
    /// </summary>
    public void ClearCurrentStage()
    {
        if (_currentLayout != null)
        {
            Destroy(_currentLayout.gameObject);
            _currentLayout = null;
        }

        // 풀링된 모든 오브젝트 일괄 제거
        _poolManager?.ClearAllPools();

        OnStageCleanedUp?.Invoke(CurrentStageIndex);
        EventBus.Instance.Publish(new StageCleanedUpEvent { StageIndex = CurrentStageIndex });
    }
}