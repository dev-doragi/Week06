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

[DefaultExecutionOrder(-100)]
public class GameFlowManager : Singleton<GameFlowManager>
{
    private StageManager _stageManager
    {
        get
        {
            if (ManagerRegistry.TryGet(out StageManager sm))
                return sm;
            return StageManager.Instance;
        }
    }

    public InGameState CurrentInGameState { get; private set; } = InGameState.None;

    private void OnEnable()
    {
        if (EventBus.Instance != null)
        {
            EventBus.Instance.Subscribe<StageLoadedEvent>(OnStageLoaded);
            EventBus.Instance.Subscribe<WaveStartedEvent>(OnWaveStarted);
            EventBus.Instance.Subscribe<EnemyDefeatedEvent>(OnEnemyDefeated);
            EventBus.Instance.Subscribe<BaseDestroyedEvent>(OnBaseDestroyed);
        }
    }

    private void OnDisable()
    {
        if (EventBus.Instance != null)
        {
            EventBus.Instance.Unsubscribe<StageLoadedEvent>(OnStageLoaded);
            EventBus.Instance.Unsubscribe<WaveStartedEvent>(OnWaveStarted);
            EventBus.Instance.Unsubscribe<EnemyDefeatedEvent>(OnEnemyDefeated);
            EventBus.Instance.Unsubscribe<BaseDestroyedEvent>(OnBaseDestroyed);
        }
    }

    // ========================================================================
    // EventBus 이벤트 핸들러
    // ========================================================================

    /// <summary>
    /// 스테이지 로드 완료 시 Prepare 상태로 진입한다.
    /// </summary>
    private void OnStageLoaded(StageLoadedEvent evt)
    {
        Debug.Log($"[GameFlowManager] Stage {evt.StageIndex} 로드됨");
        ChangeFlowState(InGameState.Prepare);
    }

    /// <summary>
    /// 웨이브 시작 이벤트 수신 시 WavePlaying 상태로 진입한다.
    /// </summary>
    private void OnWaveStarted(WaveStartedEvent evt)
    {
        Debug.Log($"[GameFlowManager] Stage {evt.StageIndex} - Wave {evt.WaveIndex} 시작");
        ChangeFlowState(InGameState.WavePlaying);
    }

    /// <summary>
    /// 웨이브 종료 조건 대상 파괴 시 승리 조건을 체크한다.
    /// </summary>
    private void OnEnemyDefeated(EnemyDefeatedEvent evt)
    {
        Debug.Log("[GameFlowManager] 적 격파 감지 - 승리 조건 체크");
        CheckWinLossCondition(isBaseDestroyed: false);
    }

    /// <summary>
    /// 아군 기지 파괴 시 패배 처리한다.
    /// </summary>
    private void OnBaseDestroyed(BaseDestroyedEvent evt)
    {
        Debug.Log("[GameFlowManager] 기지 파괴 감지 - 패배 처리");
        CheckWinLossCondition(isBaseDestroyed: true);
    }

    // ========================================================================
    // 상태 전환 및 로직 처리
    // ========================================================================

    public void ChangeFlowState(InGameState newState)
    {
        if (CurrentInGameState == newState) return;

        InGameState previousState = CurrentInGameState;
        CurrentInGameState = newState;

        Debug.Log($"[GameFlowManager] 상태 전환: {previousState} → {CurrentInGameState}");

        EventBus.Instance.Publish(new InGameStateChangedEvent { NewState = CurrentInGameState });

        _stageManager?.UpdateState(CurrentInGameState);

        ProcessStateLogic(newState);
    }

    private void ProcessStateLogic(InGameState state)
    {
        switch (state)
        {
            case InGameState.Prepare:
                Debug.Log($"[GameFlowManager] 배치 페이즈 진입 - Stage {_stageManager.CurrentStageIndex}, Wave {_stageManager.CurrentWaveIndex}/{_stageManager.CurrentStageData.Waves.Count - 1}");
                break;

            case InGameState.WavePlaying:
                Debug.Log($"[GameFlowManager] 전투 시작 - Stage {_stageManager.CurrentStageIndex}, Wave {_stageManager.CurrentWaveIndex}/{_stageManager.CurrentStageData.Waves.Count - 1}");
                break;

            case InGameState.WaveCleared:
                int clearedWave = _stageManager.CurrentWaveIndex;

                EventBus.Instance.Publish(new WaveClearedEvent
                {
                    StageIndex = _stageManager.CurrentStageIndex,
                    WaveIndex = clearedWave
                });

                Debug.Log($"[GameFlowManager] Wave {clearedWave} 클리어! (총 {_stageManager.CurrentStageData.Waves.Count}개 웨이브 중)");

                if (_stageManager.CurrentWaveIndex >= _stageManager.CurrentStageData.Waves.Count - 1)
                {
                    Debug.Log("[GameFlowManager] 모든 웨이브 클리어 → 스테이지 클리어");
                    ChangeFlowState(InGameState.StageCleared);
                }
                else
                {
                    Debug.Log("[GameFlowManager] 다음 웨이브 준비 중...");
                    _stageManager.GoToNextWave();
                    ChangeFlowState(InGameState.Prepare);
                }
                break;

            case InGameState.StageCleared:
                // 스테이지 클리어 시점에 저장 이벤트를 먼저 발행한다.
                // 이후 다음 스테이지 로드를 요청하면,
                // StageManager.LoadStage() 내부에서 기존 스테이지 정리와 다음 스테이지 로드가 함께 처리된다.
                Debug.Log($"[GameFlowManager] Stage {_stageManager.CurrentStageIndex} 클리어!");
                _stageManager?.NotifyStageCleared();
                _stageManager?.LoadNextStage();
                break;

            case InGameState.StageFailed:
                Debug.Log($"[GameFlowManager] Stage {_stageManager.CurrentStageIndex} 실패!");

                EventBus.Instance.Publish(new StageFailedEvent
                {
                    StageIndex = _stageManager.CurrentStageIndex
                });
                break;
        }
    }

    public void CheckWinLossCondition(bool isBaseDestroyed)
    {
        if (CurrentInGameState != InGameState.WavePlaying)
        {
            Debug.LogWarning($"[GameFlowManager] 승패 판정 무시 - 현재 상태: {CurrentInGameState}");
            return;
        }

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