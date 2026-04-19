using UnityEngine;

public class EventTester : MonoBehaviour
{
    [Header("Test Data")]
    [SerializeField] private AudioClip _testSFX;

    // 1. 적 처치 테스트 (웨이브 클리어 판정용)
    public void FireEnemyDefeated()
    {
        EventBus.Instance.Publish(new EnemyDefeatedEvent());
        Debug.Log("[GameEventTester] 적 처치 이벤트 (EnemyDefeatedEvent) 발행");
    }

    // 2. 기지 파괴 테스트 (게임 오버 판정용)
    public void FireBaseDestroyed()
    {
        EventBus.Instance.Publish(new BaseDestroyedEvent());
        Debug.Log("[GameEventTester] 기지 파괴 이벤트 (BaseDestroyedEvent) 발행");
    }

    // 3. 사운드 재생 테스트 (SoundManager 연동 확인용)
    public void FirePlaySFX()
    {
        if (_testSFX != null)
        {
            EventBus.Instance.Publish(new PlaySFXEvent { Clip = _testSFX, Volume = 1f });
            Debug.Log("[GameEventTester] 사운드 재생 이벤트 (PlaySFXEvent) 발행");
        }
        else
        {
            Debug.LogError("[GameEventTester] 테스트용 오디오 클립(_testSFX)이 할당되지 않았습니다.");
        }
    }

    // 4. 강제 스테이지 클리어 테스트 (다음 스테이지 넘어가기용)
    public void FireStageCleared()
    {
        if (ManagerRegistry.TryGet(out StageManager stageManager))
        {
            //stageManager.NotifyStageCleared();
            Debug.Log("[GameEventTester] 강제 스테이지 클리어 완료");
        }
    }
}