using UnityEngine;

// 모든 스테이지가 가져야 할 기본 규격
public abstract class BaseStage : MonoBehaviour
{
    public int StageIndex { get; protected set; }

    // 스테이지가 생성/로드되었을 때 (초기화)
    public abstract void SetupStage(int stageIndex);

    // 스테이지가 본격적으로 시작될 때
    public abstract void StartStage();

    // 스테이지가 종료(클리어/실패)될 때 정리 작업
    public abstract void CleanupStage();
}