using UnityEngine;
using Unity.Cinemachine;

public class CameraManager : MonoBehaviour
{
    [Header("Required Components")]
    [SerializeField] private CinemachineCamera _cmCamera;
    [SerializeField] private Transform _cameraTarget;

    [Header("Zoom Settings")]
    [SerializeField] private float _zoomStep = 2.0f;
    [SerializeField] private float _minZoom = 5.0f;
    [SerializeField] private float _maxZoom = 30.0f;
    [SerializeField] private float _zoomSmoothing = 10f;

    private InputReader _inputReader;
    private float _targetZoom;
    private bool _isDragging = false;

    private void Awake()
    {
        // 규칙: ManagerRegistry를 통한 참조
        if (!ManagerRegistry.TryGet(out _inputReader))
        {
            Debug.LogError("CameraManager: InputReader를 Registry에서 찾을 수 없습니다.");
        }

        if (_cmCamera == null) _cmCamera = GetComponent<CinemachineCamera>();
        if (_cmCamera == null)
        {
            Debug.LogError("CameraManager: CinemachineCamera가 할당되지 않았습니다.");
            return;
        }

        if (_cameraTarget == null)
        {
            Debug.LogError("CameraManager: CameraTarget(Transform)이 할당되지 않았습니다.");
            return;
        }

        _targetZoom = _cmCamera.Lens.OrthographicSize;
    }

    private void OnEnable()
    {
        if (EventBus.Instance == null) return;
        EventBus.Instance.Subscribe<RightClickEvent>(HandleRightClick);
        EventBus.Instance.Subscribe<ScrollEvent>(HandleScroll);
    }

    private void OnDisable()
    {
        if (EventBus.Instance == null) return;
        EventBus.Instance.Unsubscribe<RightClickEvent>(HandleRightClick);
        EventBus.Instance.Unsubscribe<ScrollEvent>(HandleScroll);
    }

    private void Update()
    {
        if (Time.timeScale <= 0) return;

        HandlePanning();
        HandleZooming();
    }

    private void HandlePanning()
    {
        if (!_isDragging || _inputReader == null) return;

        Vector2 mouseDelta = _inputReader.GetMouseDelta();

        if (mouseDelta.sqrMagnitude > 0.01f)
        {
            // 현재 줌 배율에 따른 이동 속도 보정 (Orthographic 전용)
            float currentZoom = _cmCamera.Lens.OrthographicSize;
            float unitsPerPixel = (currentZoom * 2f) / Screen.height;

            Vector3 move = new Vector3(-mouseDelta.x, -mouseDelta.y, 0) * unitsPerPixel;
            _cameraTarget.position += move;
        }
    }

    private void HandleZooming()
    {
        float currentSize = _cmCamera.Lens.OrthographicSize;
        if (Mathf.Abs(currentSize - _targetZoom) < 0.001f) return;

        // Cinemachine Lens 직접 수정 방식 유지
        LensSettings lens = _cmCamera.Lens;
        lens.OrthographicSize = Mathf.Lerp(currentSize, _targetZoom, Time.deltaTime * _zoomSmoothing);
        _cmCamera.Lens = lens;
    }

    private void HandleRightClick(RightClickEvent e)
    {
        // UI 위에 있을 때 드래그 시작 방지
        if (_inputReader.IsPointerOverUI && e.IsStarted) return;
        _isDragging = e.IsStarted;
    }

    private void HandleScroll(ScrollEvent e)
    {
        if (_inputReader.IsPointerOverUI) return;

        float direction = e.Delta > 0 ? -1f : 1f;
        _targetZoom = Mathf.Clamp(_targetZoom + (direction * _zoomStep), _minZoom, _maxZoom);
    }
}