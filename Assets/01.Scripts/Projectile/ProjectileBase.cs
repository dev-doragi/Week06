using System.Collections;
using UnityEngine;

/// <summary>
/// 모든 투사체의 최상위 베이스 클래스입니다.
/// 오직 수명 관리와 풀링(회수) 로직만 담당합니다.
/// </summary>
public abstract class ProjectileBase : MonoBehaviour
{
    [Header("Lifecycle Settings")]
    [Tooltip("발사체가 생성된 후 자동으로 풀에 반환되는 시간(초)")]
    [SerializeField] protected float _lifeTime = 5f;

    private Coroutine _lifeTimeCoroutine;

    // 풀에서 꺼내져 활성화될 때 (생성 주기)
    protected virtual void OnEnable()
    {
        // 1. 스테이지가 끝날 때 스스로 파괴되도록 이벤트 구독
        EventBus.Instance.Subscribe<StageCleanedUpEvent>(HandleStageCleanedUp);

        // 2. 수명 타이머 시작
        _lifeTimeCoroutine = StartCoroutine(LifeTimeRoutine());
    }

    // 풀로 들어가 비활성화될 때
    protected virtual void OnDisable()
    {
        // 1. 이벤트 구독 해제 (메모리 누수 방지)
        EventBus.Instance.Unsubscribe<StageCleanedUpEvent>(HandleStageCleanedUp);

        // 2. 실행 중인 타이머 강제 종료
        if (_lifeTimeCoroutine != null)
        {
            StopCoroutine(_lifeTimeCoroutine);
            _lifeTimeCoroutine = null;
        }
    }

    // 설정된 시간이 지나면 자동으로 회수
    private IEnumerator LifeTimeRoutine()
    {
        yield return new WaitForSeconds(_lifeTime);
        Despawn();
    }

    // 스테이지 초기화 이벤트 발생 시 즉시 회수
    private void HandleStageCleanedUp(StageCleanedUpEvent evt)
    {
        Despawn();
    }

    /// <summary>
    /// 발사체를 파괴(풀로 반환)합니다.
    /// 적에게 적중했을 때 자식 클래스에서 이 메서드를 호출하면 됩니다.
    /// </summary>
    protected virtual void Despawn()
    {
        // 이미 비활성화된 상태에서 중복 호출되는 것을 방지
        if (gameObject.activeInHierarchy)
        {
            PoolManager.Instance.Despawn(gameObject);
        }
    }
}