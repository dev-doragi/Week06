using System;
using UnityEngine;

public class StageManager : Singleton<StageManager>
{
    [Header("Stage Settings")]
    [SerializeField] private StageDataSO[] _stageDatas;
    [SerializeField] private Transform _stageParent;

    private StageLayout _currentLayout;

    public int CurrentStageIndex { get; private set; } = 0;
    public int CurrentWaveIndex { get; private set; } = 0; // 추가: 현재 웨이브 인덱스

    public StageDataSO CurrentStageData => _stageDatas[CurrentStageIndex];
    public StageLayout CurrentLayout => _currentLayout;

    public static event Action<int> OnStageLoaded;
    public static event Action<int, int> OnWaveStarted; // 스테이지 인덱스, 웨이브 인덱스
    public static event Action<int> OnStageCleared;
    public static event Action<int> OnStageCleanedUp;

    protected override void Init()
    {
        if (_stageParent == null)
        {
            _stageParent = new GameObject("StageContainer").transform;
        }
    }

    private void Start()
    {
        // 디버깅용
        LoadStage(0);
        PlayWave(); // PlayStage -> PlayWave로 변경
    }

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

    public void LoadStage(int stageIndex)
    {
        if (stageIndex < 0 || stageIndex >= _stageDatas.Length)
        {
            Debug.LogError($"[StageManager] 유효하지 않은 스테이지 인덱스: {stageIndex}");
            return;
        }

        ClearCurrentStage();

        CurrentStageIndex = stageIndex;
        CurrentWaveIndex = 0; // 스테이지 로드 시 웨이브 초기화
        StageDataSO nextData = _stageDatas[CurrentStageIndex];

        if (nextData.StageLayoutPrefab != null)
        {
            _currentLayout = Instantiate(nextData.StageLayoutPrefab, _stageParent);
        }

        OnStageLoaded?.Invoke(CurrentStageIndex);
        EventBus.Instance.Publish(new StageLoadedEvent { StageIndex = CurrentStageIndex });

        Debug.Log($"[StageManager] Stage {stageIndex} 로드 완료.");
    }

    /// <summary>
    /// 로드된 스테이지의 현재 웨이브를 시작합니다.
    /// </summary>
    public void PlayWave()
    {
        OnWaveStarted?.Invoke(CurrentStageIndex, CurrentWaveIndex);
        EventBus.Instance.Publish(new WaveStartedEvent { StageIndex = CurrentStageIndex, WaveIndex = CurrentWaveIndex });
        Debug.Log($"[StageManager] Stage {CurrentStageIndex} - Wave {CurrentWaveIndex} Start");
    }

    /// <summary>
    /// 웨이브를 증가시킵니다. (GameFlowManager에서 호출)
    /// </summary>
    public void GoToNextWave()
    {
        CurrentWaveIndex++;
    }

    public void NotifyStageCleared()
    {
        OnStageCleared?.Invoke(CurrentStageIndex);
        EventBus.Instance.Publish(new StageClearedEvent { StageIndex = CurrentStageIndex });
    }

    public void ClearCurrentStage()
    {
        if (_currentLayout != null)
        {
            Destroy(_currentLayout.gameObject);
            _currentLayout = null;
        }

        OnStageCleanedUp?.Invoke(CurrentStageIndex);
        EventBus.Instance.Publish(new StageCleanedUpEvent { StageIndex = CurrentStageIndex });
    }
}