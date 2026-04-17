using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PoolSetupData
{
    public GameObject Prefab;
    [Tooltip("초기 생성 개수 (Prewarm)")]
    public int InitialSize;
    [Tooltip("최대 생성 허용 개수")]
    public int MaxSize;
}

// 모든 스테이지가 가져야 할 기본 규격
public abstract class BaseStage : MonoBehaviour
{
    [Header("Stage Pool Settings")]
    [SerializeField] protected List<PoolSetupData> _poolSetupList = new List<PoolSetupData>();

    public int StageIndex { get; protected set; }

    // 스테이지가 생성/로드되었을 때 (초기화)
    public virtual void SetupStage(int stageIndex)
    {
        StageIndex = stageIndex;
        PrewarmPools();
    }

    /// <summary>
    /// 스테이지에 필요한 오브젝트들을 PoolManager에 미리 등록하고 생성합니다.
    /// </summary>
    protected virtual void PrewarmPools()
    {
        if (!ManagerRegistry.TryGet(out PoolManager poolManager))
        {
            Debug.LogError($"[BaseStage] PoolManager를 Registry에서 찾을 수 없어 스테이지({StageIndex}) 풀을 생성할 수 없습니다.");
            return;
        }

        foreach (var setupData in _poolSetupList)
        {
            if (setupData.Prefab != null)
            {
                poolManager.CreatePool(setupData.Prefab, setupData.InitialSize, setupData.MaxSize);
            }
        }
    }

    // 스테이지가 본격적으로 시작될 때
    public abstract void StartStage();

    // 스테이지가 종료(클리어/실패)될 때 정리 작업
    public abstract void CleanupStage();
}