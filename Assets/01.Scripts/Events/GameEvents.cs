public struct StageLoadedEvent { public int StageIndex; }
public struct StageStartedEvent { public int StageIndex; }
public struct StageClearedEvent { public int StageIndex; }
public struct StateChangedEvent { public GameState NewState; }

// 스테이지가 정리될 때 (다음 스테이지 이동 전이나 종료 시)
public struct StageCleanedUpEvent
{
    public int StageIndex;
}