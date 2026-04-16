using System;
using UnityEngine;

[DefaultExecutionOrder(-99)]
public class StageManager : Singleton<StageManager>
{
    private BaseStage _currentStage;
    public int CurrentStageIndex { get; private set; } = 0;

    public static event Action<int> OnStageLoaded;
    public static event Action<int> OnStageStarted;
    public static event Action<int> OnStageCleared;

    protected override void Init()
    {
        // 초기화 로직 필요 시 작성
    }

    // 스테이지 진입 (GameManager 등에서 호출)
    public void LoadStage(int stageIndex, BaseStage stagePrefab = null)
    {
        if (_currentStage != null)
        {
            _currentStage.CleanupStage();
            Destroy(_currentStage.gameObject);
        }

        CurrentStageIndex = stageIndex;

        // 예시: 프리팹을 받아와서 생성하거나 씬에 있는 스테이지를 찾음
        if (stagePrefab != null)
        {
            _currentStage = Instantiate(stagePrefab);
        }
        else
        {
            _currentStage = FindAnyObjectByType<BaseStage>();
        }

        if (_currentStage == null)
        {
            Debug.LogError("로드할 스테이지 객체를 찾을 수 없습니다.");
            return;
        }

        _currentStage.SetupStage(CurrentStageIndex);

        OnStageLoaded?.Invoke(CurrentStageIndex);
    }

    public void PlayStage()
    {
        if (_currentStage == null) return;

        _currentStage.StartStage();
        OnStageStarted?.Invoke(CurrentStageIndex);
    }

    public void ClearCurrentStage()
    {
        OnStageCleared?.Invoke(CurrentStageIndex);
    }
}