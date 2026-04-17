using System;
using UnityEngine;

public enum InGameState
{
    None,
    Prepare,      // 배치 페이즈 (상점 등)
    WavePlaying,  // 현재 웨이브 전투 진행 중
    WaveCleared,  // 현재 웨이브 클리어 (다음 웨이브가 있다면 다시 Prepare로)
    StageCleared, // 스테이지의 모든 웨이브 클리어 (승리)
    StageFailed   // 기지 파괴 등 (패배)
}

[DefaultExecutionOrder(-98)]
public class GameFlowManager : Singleton<GameFlowManager>
{
    private StageManager _stageManager;
    private EventBus _eventBus;

    public InGameState CurrentInGameState { get; private set; } = InGameState.None;

    public static event Action<InGameState> OnInGameStateChanged;

    protected override void Init()
    {
        if (!ManagerRegistry.TryGet(out _stageManager))
            Debug.LogError("[GameFlowManager] StageManager를 Registry에서 찾을 수 없습니다.");
    }

    private void OnEnable()
    {
        StageManager.OnStageLoaded += HandleStageLoaded;
        StageManager.OnWaveStarted += HandleWaveStarted; // 명칭 변경
    }

    private void OnDisable()
    {
        StageManager.OnStageLoaded -= HandleStageLoaded;
        StageManager.OnWaveStarted -= HandleWaveStarted;
    }

    private void HandleStageLoaded(int stageIndex)
    {
        ChangeFlowState(InGameState.Prepare);
    }

    private void HandleWaveStarted(int stageIndex, int waveIndex)
    {
        ChangeFlowState(InGameState.WavePlaying);
    }

    public void ChangeFlowState(InGameState newState)
    {
        if (CurrentInGameState == newState) return;

        CurrentInGameState = newState;
        OnInGameStateChanged?.Invoke(CurrentInGameState);

        // EventBus.Instance.Publish(new InGameStateChangedEvent { NewState = CurrentInGameState });

        ProcessStateLogic(newState);
    }

    private void ProcessStateLogic(InGameState state)
    {
        switch (state)
        {
            case InGameState.Prepare:
                break;
            case InGameState.WavePlaying:
                break;
            case InGameState.WaveCleared:
                // 웨이브 클리어 시, 마지막 웨이브인지 검사
                if (_stageManager.CurrentWaveIndex >= _stageManager.CurrentStageData.Waves.Count - 1)
                {
                    // 모든 웨이브를 깼다면 스테이지 클리어로 넘어감
                    ChangeFlowState(InGameState.StageCleared);
                }
                else
                {
                    // 남은 웨이브가 있다면 인덱스를 올리고 다시 준비(배치) 상태로
                    _stageManager.GoToNextWave();
                    ChangeFlowState(InGameState.Prepare);
                }
                break;
            case InGameState.StageCleared:
                _stageManager?.ClearCurrentStage();
                break;
            case InGameState.StageFailed:
                break;
        }
    }

    public void CheckWinLossCondition(bool isBaseDestroyed)
    {
        if (CurrentInGameState != InGameState.WavePlaying) return;

        if (isBaseDestroyed)
        {
            ChangeFlowState(InGameState.StageFailed);
        }
        else
        {
            // 적 격파 시 무조건 WaveCleared 상태로 넘김 (이후 ProcessStateLogic에서 검사)
            ChangeFlowState(InGameState.WaveCleared);
        }
    }
}