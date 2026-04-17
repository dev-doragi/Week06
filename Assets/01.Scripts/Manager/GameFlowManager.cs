using System;
using UnityEngine;

public enum InGameState
{
    None,
    Prepare,      // 스테이지 로드 완료, 전투 시작 전 대기 (UI 세팅 등)
    WavePlaying,  // 실제 전투(웨이브) 진행 중
    WaveCleared,  // 현재 웨이브 클리어 (다음 웨이브 대기)
    StageCleared, // 스테이지 전체 클리어 (승리)
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

        if (!ManagerRegistry.TryGet(out _eventBus))
            Debug.LogError("[GameFlowManager] EventBus를 Registry에서 찾을 수 없습니다.");
    }

    private void OnEnable()
    {
        StageManager.OnStageLoaded += HandleStageLoaded;
        StageManager.OnStageStarted += HandleStageStarted;
    }

    private void OnDisable()
    {
        StageManager.OnStageLoaded -= HandleStageLoaded;
        StageManager.OnStageStarted -= HandleStageStarted;
    }

    /// <summary>
    /// 스테이지 매니저가 프리팹을 스왑하고 셋업을 마치면 호출됩니다.
    /// </summary>
    private void HandleStageLoaded(int stageIndex)
    {
        ChangeFlowState(InGameState.Prepare);
    }

    /// <summary>
    /// 사용자가 '전투 시작'을 누르거나, 연출이 끝나서 본격적인 스테이지가 시작될 때 호출됩니다.
    /// </summary>
    private void HandleStageStarted(int stageIndex)
    {
        ChangeFlowState(InGameState.WavePlaying);
    }

    public void ChangeFlowState(InGameState newState)
    {
        if (CurrentInGameState == newState) return;

        CurrentInGameState = newState;
        OnInGameStateChanged?.Invoke(CurrentInGameState);

        // 필요하다면 EventBus를 통해 전역으로 상태 변경 알림
        // _eventBus?.Publish(new InGameStateChangedEvent { NewState = CurrentInGameState });

        ProcessStateLogic(newState);
    }

    private void ProcessStateLogic(InGameState state)
    {
        switch (state)
        {
            case InGameState.Prepare:
                // TODO: UI에서 웨이브 정보 표시, 카메라 초기화 등
                break;
            case InGameState.WavePlaying:
                // TODO: BaseStage 또는 WaveManager에 적 스폰 시작 명령
                break;
            case InGameState.StageCleared:
                // TODO: 결과 UI 출력 후 StageManager.LoadNextStage() 호출 대기
                _stageManager?.ClearCurrentStage();
                break;
            case InGameState.StageFailed:
                // TODO: 패배 UI 출력 및 재시작 로직
                break;
        }
    }

    /// <summary>
    /// 적군이 모두 죽었거나 기지가 파괴되었을 때 외부(캐릭터/성벽 로직)에서 호출
    /// </summary>
    public void CheckWinLossCondition(bool isBaseDestroyed)
    {
        if (CurrentInGameState != InGameState.WavePlaying) return;

        if (isBaseDestroyed)
        {
            ChangeFlowState(InGameState.StageFailed);
        }
        else
        {
            // 임시 클리어 처리 (실제로는 웨이브 종료 체크 필요)
            ChangeFlowState(InGameState.StageCleared);
        }
    }
}