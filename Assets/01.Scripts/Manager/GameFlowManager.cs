using System;
using UnityEngine;

public enum InGameState
{
    None,
    Prepare,
    WavePlaying,
    WaveCleared,
    StageCleared,
    StageFailed
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
        StageManager.OnWaveStarted += HandleWaveStarted;

        EventBus.Instance.Subscribe<EnemyDefeatedEvent>(OnEnemyDefeated);
        EventBus.Instance.Subscribe<BaseDestroyedEvent>(OnBaseDestroyed);
    }

    private void OnDisable()
    {
        StageManager.OnStageLoaded -= HandleStageLoaded;
        StageManager.OnWaveStarted -= HandleWaveStarted;

        if (EventBus.Instance != null)
        {
            EventBus.Instance.Unsubscribe<EnemyDefeatedEvent>(OnEnemyDefeated);
            EventBus.Instance.Unsubscribe<BaseDestroyedEvent>(OnBaseDestroyed);
        }
    }

    private void HandleStageLoaded(int stageIndex)
    {
        ChangeFlowState(InGameState.Prepare);
    }

    private void HandleWaveStarted(int stageIndex, int waveIndex)
    {
        ChangeFlowState(InGameState.WavePlaying);
    }

    private void OnEnemyDefeated(EnemyDefeatedEvent evt)
    {
        CheckWinLossCondition(isBaseDestroyed: false);
    }

    private void OnBaseDestroyed(BaseDestroyedEvent evt)
    {
        CheckWinLossCondition(isBaseDestroyed: true);
    }

    public void ChangeFlowState(InGameState newState)
    {
        if (CurrentInGameState == newState) return;

        CurrentInGameState = newState;
        OnInGameStateChanged?.Invoke(CurrentInGameState);
        EventBus.Instance.Publish(new InGameStateChangedEvent { NewState = CurrentInGameState });

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
                // [추가] WaveClearedEvent 발행
                EventBus.Instance.Publish(new WaveClearedEvent
                {
                    StageIndex = _stageManager.CurrentStageIndex,
                    WaveIndex = _stageManager.CurrentWaveIndex
                });

                if (_stageManager.CurrentWaveIndex >= _stageManager.CurrentStageData.Waves.Count - 1)
                {
                    ChangeFlowState(InGameState.StageCleared);
                }
                else
                {
                    _stageManager.GoToNextWave();
                    ChangeFlowState(InGameState.Prepare);
                }
                break;
            case InGameState.StageCleared:
                _stageManager?.NotifyStageCleared();
                _stageManager?.ClearCurrentStage();
                break;
            case InGameState.StageFailed:
                EventBus.Instance.Publish(new StageFailedEvent{ StageIndex = _stageManager.CurrentStageIndex });
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
            ChangeFlowState(InGameState.WaveCleared);
        }
    }
}