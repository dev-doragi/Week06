// GameEvents.cs

using UnityEngine;

public struct StageLoadedEvent { public int StageIndex; }

// [변경 및 추가] 웨이브 전용 이벤트
public struct WaveStartedEvent { public int StageIndex; public int WaveIndex; }
public struct WaveClearedEvent { public int StageIndex; public int WaveIndex; }
public struct StageClearedEvent { public int StageIndex; }
public struct StageCleanedUpEvent { public int StageIndex; }
public struct StateChangedEvent { public GameState NewState; }
public struct InGameStateChangedEvent { public InGameState NewState; }
public struct PlaySFXEvent
{
    public AudioClip Clip;
    public float Volume;
}