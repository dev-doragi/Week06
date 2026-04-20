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

    // 내부 저장: 클리어된 스테이지 인덱스 집합
    private readonly HashSet<int> _clearedStages = new HashSet<int>();

    public IReadOnlyCollection<int> ClearedStages => _clearedStages;

    // 가장 높은 클리어 인덱스(없으면 -1)
    public int HighestClearedStage { get; private set; } = -1;

    protected override void Init()
    {
        LoadFromPrefs();
    }

    private void OnEnable()
    {
        if (EventBus.Instance == null) return;
        EventBus.Instance.Subscribe<StageClearedEvent>(OnStageCleared);
    }

    private void OnDisable()
    {
        if (EventBus.Instance == null) return;
        EventBus.Instance.Unsubscribe<StageClearedEvent>(OnStageCleared);
    }

    private void OnStageCleared(StageClearedEvent evt)
    {
        MarkStageCleared(evt.StageIndex);
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
            Debug.Log($"[ProgressManager] Stage {stageIndex} marked cleared. HighestClearedStage={HighestClearedStage}");
            EventBus.Instance?.Publish(new StageProgressUpdatedEvent { HighestCleared = HighestClearedStage });
        }
    }

    /// <summary>
    /// 지정된 스테이지가 해금되어 있는지 판정합니다.
    /// 규칙: stage 0은 항상 해금. 그 외 스테이지 i는 i <= HighestClearedStage + 1 이면 해금.
    /// </summary>
    public bool IsStageUnlocked(int stageIndex)
    {
        if (stageIndex <= 0) return true;
        return stageIndex <= (HighestClearedStage + 1);
    }

    private void LoadFromPrefs()
    {
        _clearedStages.Clear();
        HighestClearedStage = -1;

        string raw = PlayerPrefs.GetString(PREF_KEY, string.Empty);
        if (string.IsNullOrWhiteSpace(raw)) return;

        try
        {
            var parts = raw.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var p in parts)
            {
                if (int.TryParse(p, out int idx))
                {
                    _clearedStages.Add(idx);
                    HighestClearedStage = Math.Max(HighestClearedStage, idx);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[ProgressManager] LoadFromPrefs 실패: {ex.Message}");
            _clearedStages.Clear();
            HighestClearedStage = -1;
        }

        Debug.Log($"[ProgressManager] Loaded cleared stages ({_clearedStages.Count}) Highest={HighestClearedStage}");
    }

    private void SaveToPrefs()
    {
        try
        {
            if (_clearedStages.Count == 0)
            {
                PlayerPrefs.DeleteKey(PREF_KEY);
            }
            else
            {
                // 정렬해서 저장
                var sorted = _clearedStages.OrderBy(i => i);
                string raw = string.Join(",", sorted);
                PlayerPrefs.SetString(PREF_KEY, raw);
            }
            PlayerPrefs.Save();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[ProgressManager] SaveToPrefs 실패: {ex.Message}");
        }
    }

    /// <summary>
    /// 모든 진행을 지웁니다. (런타임 호출 가능)
    /// </summary>
    [ContextMenu("ResetClearedStages")]
    public void ClearAllProgress()
    {
        _clearedStages.Clear();
        HighestClearedStage = -1;
        PlayerPrefs.DeleteKey(PREF_KEY);
        PlayerPrefs.Save();
        Debug.Log("[ProgressManager] Cleared all progress (reset).");
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