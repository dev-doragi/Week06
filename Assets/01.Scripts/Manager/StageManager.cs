using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class StageManager : Singleton<StageManager>
{
    [Header("Stage Settings")]
    [SerializeField] private StageDataSO[] _stageDatas;
    [SerializeField] private Transform _stageParent;

    private StageLayout _currentLayout;

    public int CurrentStageIndex { get; private set; } = 0;
    public int CurrentWaveIndex { get; private set; } = 0;

    // [추가] 현재 진행 상태 저장
    public InGameState CurrentState { get; private set; } = InGameState.None;

    public StageDataSO CurrentStageData => _stageDatas[CurrentStageIndex];
    public StageLayout CurrentLayout => _currentLayout;

    protected override void Init()
    {
        if (_stageParent == null)
        {
            _stageParent = new GameObject("StageContainer").transform;
        }
    }

    private void Start()
    {
        LoadStage(0);
    }

    /// <summary>
    /// [GameFlowManager에서 호출] 현재 스테이지의 상태 업데이트
    /// </summary>
    public void UpdateState(InGameState newState)
    {
        CurrentState = newState;
        Debug.Log($"[StageManager] 상태 업데이트: {newState} (Stage {CurrentStageIndex}, Wave {CurrentWaveIndex})");
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
        CurrentWaveIndex = 0;
        CurrentState = InGameState.None;

        StageDataSO nextData = _stageDatas[CurrentStageIndex];

        if (nextData.StageLayoutPrefab != null)
        {
            _currentLayout = Instantiate(nextData.StageLayoutPrefab, _stageParent);
        }

        EventBus.Instance.Publish(new StageLoadedEvent { StageIndex = CurrentStageIndex });

        Debug.Log($"[StageManager] Stage {stageIndex} 로드 완료 (총 {CurrentStageData.Waves.Count}개 웨이브).");
    }

    /// <summary>
    /// 로드된 스테이지의 현재 웨이브를 시작합니다.
    /// </summary>
    public void PlayWave()
    {
        EventBus.Instance.Publish(new WaveStartedEvent { StageIndex = CurrentStageIndex, WaveIndex = CurrentWaveIndex });

        Debug.Log($"[StageManager] Stage {CurrentStageIndex} - Wave {CurrentWaveIndex}/{CurrentStageData.Waves.Count - 1} 시작");
    }

    /// <summary>
    /// 웨이브를 증가시킵니다. (GameFlowManager에서 호출)
    /// </summary>
    public void GoToNextWave()
    {
        CurrentWaveIndex++;
        Debug.Log($"[StageManager] 다음 웨이브로 이동: Wave {CurrentWaveIndex}/{CurrentStageData.Waves.Count - 1}");
    }

    public void NotifyStageCleared()
    {
        EventBus.Instance.Publish(new StageClearedEvent { StageIndex = CurrentStageIndex });
        Debug.Log($"[StageManager] Stage {CurrentStageIndex} 클리어 알림 발송");
    }

    public void ClearCurrentStage()
    {
        if (_currentLayout != null)
        {
            Destroy(_currentLayout.gameObject);
            _currentLayout = null;
        }

        EventBus.Instance.Publish(new StageCleanedUpEvent { StageIndex = CurrentStageIndex });

        CurrentState = InGameState.None;
        Debug.Log($"[StageManager] Stage {CurrentStageIndex} 정리 완료");
    }
}