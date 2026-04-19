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
            // 전역 게임 상태 모니터링
            EventBus.Instance.Subscribe<GameStateChangedEvent>(OnGlobalStateChanged);

            // 인게임 진행 이벤트 구독
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
            EventBus.Instance.Unsubscribe<GameStateChangedEvent>(OnGlobalStateChanged);
            EventBus.Instance.Unsubscribe<StageLoadedEvent>(OnStageLoaded);
            EventBus.Instance.Unsubscribe<WaveStartedEvent>(OnWaveStarted);
            EventBus.Instance.Unsubscribe<EnemyDefeatedEvent>(OnEnemyDefeated);
            EventBus.Instance.Unsubscribe<BaseDestroyedEvent>(OnBaseDestroyed);
        }
    }

    // ========================================================================
    // EventBus 이벤트 핸들러
    // ========================================================================

    private void OnGlobalStateChanged(GameStateChangedEvent evt)
    {
        // 게임 오버, 클리어, 혹은 로비(Ready)로 돌아갈 경우 인게임 상태 초기화
        if (evt.NewState == GameState.GameOver || evt.NewState == GameState.GameClear || evt.NewState == GameState.Ready)
        {
            ChangeFlowState(InGameState.None);
        }
    }

    private void OnStageLoaded(StageLoadedEvent evt)
    {
        Debug.Log($"[GameFlowManager] Stage {evt.StageIndex} 로드됨");
        ChangeFlowState(InGameState.Prepare);
    }

    private void OnWaveStarted(WaveStartedEvent evt)
    {
        Debug.Log($"[GameFlowManager] Stage {evt.StageIndex} - Wave {evt.WaveIndex} 시작");
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

    // ========================================================================
    // 상태 전환 및 로직 처리
    // ========================================================================

    public void ChangeFlowState(InGameState newState)
    {
        if (CurrentInGameState == newState) return;

        if (newState != InGameState.None && GameManager.Instance.CurrentState != GameState.Playing)
        {
            Debug.LogWarning($"[GameFlowManager] 전역 상태가 Playing이 아니므로 {newState} 전환을 거부합니다.");
            return;
        }

        InGameState previousState = CurrentInGameState;
        CurrentInGameState = newState;

        Debug.Log($"[GameFlowManager] State Changed: {CurrentInGameState} -> {newState}");

        // 상태 변경 방송
        EventBus.Instance.Publish(new InGameStateChangedEvent { NewState = CurrentInGameState });

        // 스테이지 매니저 동기화
        _stageManager?.UpdateState(CurrentInGameState);

        ProcessStateLogic(newState);
    }

    private void ProcessStateLogic(InGameState state)
    {
        switch (state)
        {
            case InGameState.WaveCleared:
                if (_stageManager.CurrentWaveIndex >= _stageManager.CurrentStageData.Waves.Count - 1)
                {
                    ChangeFlowState(InGameState.StageCleared);
                }
                else
                {
                    _stageManager.GoToNextWave();
                    // TODO: 다음 웨이브는 인터벌 이후 자동 진행되도록 해야함
                    ChangeFlowState(InGameState.Prepare);
                }
                break;

            case InGameState.StageCleared:
                EventBus.Instance.Publish(new StageClearedEvent { StageIndex = _stageManager.CurrentStageIndex });
                break;

            case InGameState.StageFailed:
                EventBus.Instance.Publish(new StageFailedEvent { StageIndex = _stageManager.CurrentStageIndex });
                break;
        }
    }

    public void CheckWinLossCondition(bool isBaseDestroyed)
    {
        // 전투 중일 때만 판정 수행
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