using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 전역 스테이지 진행(클리어된 스테이지 인덱스)을 관리하고 영구 저장합니다.
/// StageClearedEvent를 구독하여 자동 저장합니다.
/// 저장 포맷: PlayerPrefs string (comma-separated ints), 키 = ClearedStages_v1
/// </summary>
[DefaultExecutionOrder(-95)]
public class ProgressManager : Singleton<ProgressManager>
{
    private const string PREF_KEY = "ClearedStages_v1";

    private readonly HashSet<int> _clearedStages = new HashSet<int>();
    public IReadOnlyCollection<int> ClearedStages => _clearedStages;
    public int HighestClearedStage { get; private set; } = -1;
    // 튜토리얼 완료 여부 (튜토리얼 보상으로 첫 스테이지만 해금할 때 사용)
    private bool _tutorialCompleted = false;

    protected override void Init()
    {
        // Do not load from persistent storage. Progress is kept in-memory only
        // so that each run starts with a fresh state.
        _clearedStages.Clear();
        HighestClearedStage = -1;
        _tutorialCompleted = false;
    }

    private void OnEnable()
    {
        if (EventBus.Instance == null) return;
        EventBus.Instance.Subscribe<StageClearedEvent>(OnStageCleared);

        // 튜토리얼 완료 이벤트 수신 추가
        EventBus.Instance.Subscribe<TutorialCompletedEvent>(OnTutorialCompleted);
    }

    private void OnDisable()
    {
        if (EventBus.Instance == null) return;
        EventBus.Instance.Unsubscribe<StageClearedEvent>(OnStageCleared);
        EventBus.Instance.Unsubscribe<TutorialCompletedEvent>(OnTutorialCompleted);
    }

    private void OnStageCleared(StageClearedEvent evt)
    {
        // Do not treat tutorial runs as real stage clears.
        // If StageLoadContext indicates we're in tutorial mode, ignore these events.
        if (StageLoadContext.IsTutorial)
        {
            Debug.Log($"[ProgressManager] Ignoring StageClearedEvent for Stage {evt.StageIndex} during tutorial.");
            return;
        }

        MarkStageCleared(evt.StageIndex);
    }

    private void OnTutorialCompleted(TutorialCompletedEvent evt)
    {
        // 튜토리얼 완료 시에는 "튜토리얼 보상으로 첫 스테이지 해금"을
        // 클리어 처리와 구분하여 별도 플래그로 관리합니다.
        // 일반적인 경우 RewardStageIndex == 0 이며, 이때는 실제로 클리어로
        // 기록하지 않고 튜토리얼 플래그만 설정합니다.
        if (evt.RewardStageIndex == 0)
        {
            _tutorialCompleted = true;
            Debug.Log("[ProgressManager] Tutorial completed. Stage 0 unlocked.");
            EventBus.Instance?.Publish(new StageProgressUpdatedEvent { HighestCleared = HighestClearedStage });
        }
        else if (evt.RewardStageIndex > 0)
        {
            // 만약 튜토리얼 보상이 0이 아닌 다른 스테이지라면 기존 동작대로 클리어 처리
            MarkStageCleared(evt.RewardStageIndex);
        }
    }

    /// <summary>
    /// 주어진 스테이지 인덱스를 클리어로 표시하고, 변경 시 저장합니다.
    /// </summary>
    public void MarkStageCleared(int stageIndex)
    {
        if (stageIndex < 0) return;

        if (_clearedStages.Add(stageIndex))
        {
            HighestClearedStage = Math.Max(HighestClearedStage, stageIndex);
            SaveToPrefs();
            EventBus.Instance?.Publish(new StageProgressUpdatedEvent { HighestCleared = HighestClearedStage });
        }
    }

    /// <summary>
    /// 지정된 스테이지가 해금되어 있는지 판정합니다.
    /// 규칙:
    /// - stage 0: 튜토리얼 완료(혹은 명시적으로 클리어 처리) 시 해금됩니다.
    /// - 그 외 스테이지 i: i <= HighestClearedStage + 1 이면 해금됩니다.
    /// </summary>
    public bool IsStageUnlocked(int stageIndex)
    {
        if (stageIndex < 0) return false;

        // Stage 0은 기본으로 해금되지 않음. TutorialCompletedEvent로 MarkStageCleared(0)가 호출되어야 해금된다.
        if (stageIndex == 0)
        {
            return _tutorialCompleted || _clearedStages.Contains(0);
        }

        return stageIndex <= (HighestClearedStage + 1);
    }

    private void LoadFromPrefs()
    {
        // Persistence disabled: do nothing. Progress is transient for each session.
    }

    private void SaveToPrefs()
    {
        // Persistence disabled: do nothing.
    }

    /// <summary>
    /// 모든 진행을 지웁니다. (런타임 호출 가능)
    /// </summary>
    [ContextMenu("ResetClearedStages")]
    public void ClearAllProgress()
    {
        _clearedStages.Clear();
        HighestClearedStage = -1;
        _tutorialCompleted = false;
        // Do not touch PlayerPrefs — progress should not be persisted across runs.
        EventBus.Instance?.Publish(new StageProgressUpdatedEvent { HighestCleared = HighestClearedStage });
    }
}

/// <summary>
/// 진행 상태가 변경될 때 발행되는 이벤트 (옵션: UI가 이를 구독하여 갱신할 수 있음)
/// </summary>
public struct StageProgressUpdatedEvent
{
    public int HighestCleared;
}