using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TutorialSkipButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Settings")]
    [SerializeField] private float _holdDuration = 1.5f; // 1.5초 정도가 적당합니다.
    [SerializeField] private Image _fillImage;           // 게이지용 UI 이미지

    private float _timer = 0f;
    private bool _isHolding = false;

    private void Update()
    {
        if (_isHolding)
        {
            // 튜토리얼 중 시간이 멈춰있을 수 있으므로 unscaledDeltaTime 사용
            _timer += Time.unscaledDeltaTime;

            if (_fillImage != null)
                _fillImage.fillAmount = _timer / _holdDuration;

            if (_timer >= _holdDuration)
            {
                OnSkipComplete();
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isHolding = true;
        _timer = 0f;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ResetButton();
    }

    private void ResetButton()
    {
        _isHolding = false;
        _timer = 0f;
        if (_fillImage != null)
            _fillImage.fillAmount = 0f;
    }

    private void OnSkipComplete()
    {
        ResetButton();
        Debug.Log("[Tutorial] Skip Hold Complete!");

        // 1. 튜토리얼 매니저 정리 (시간 복구 및 코루틴 정지)
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.StopAllCoroutines();
            Time.timeScale = 1f; // 혹시 멈춰있을 시간을 위해 강제 복구
        }

        // 2. UIManager를 통해 스테이지 선택 창으로 이동
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OnGoToStageSelectClicked();
        }
    }
}