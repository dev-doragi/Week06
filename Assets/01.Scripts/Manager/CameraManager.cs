using UnityEngine;
using Unity.Cinemachine;

public class CameraManager : MonoBehaviour
{
    [Header("Required Components")]
    [SerializeField] private CinemachineCamera _cmCamera;
    [SerializeField] private Transform _cameraTarget;
    [SerializeField] private Collider2D _confinerCollider;

    [Header("Zoom Settings")]
    [SerializeField] private float _zoomStep = 2.0f;
    [SerializeField] private float _minZoom = 5.0f;
    [SerializeField] private float _maxZoom = 30.0f;

    private InputReader _inputReader;
    private float _targetZoom;
    private float _initialZoom;
    private bool _isDragging = false;
    private float _aspectRatio;

    private void Awake()
    {
        if (!ManagerRegistry.TryGet(out _inputReader))
        {
            Debug.LogError("CameraManager: InputReader not found");
        }

        if (_cmCamera == null) _cmCamera = GetComponent<CinemachineCamera>();

        if (_cmCamera == null || _cameraTarget == null)
        {
            Debug.LogError("CameraManager: Critical components missing");
            return;
        }

        _initialZoom = _cmCamera.Lens.OrthographicSize;
        _targetZoom = _initialZoom;
        _aspectRatio = (float)Screen.width / Screen.height;

        if (_confinerCollider == null)
        {
            var confiner = GetComponent<CinemachineConfiner2D>();
            if (confiner != null) _confinerCollider = confiner.BoundingShape2D;
        }
    }

    private void Start()
    {
        Vector3 clampedPos = ClampTargetPosition(_cameraTarget.position);
        _cameraTarget.position = clampedPos;
        _cmCamera.ForceCameraPosition(clampedPos, Quaternion.identity);
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
    }

    private void HandlePanning()
    {
        if (!_isDragging || _inputReader == null) return;

        Vector2 mouseDelta = _inputReader.GetMouseDelta();

        if (mouseDelta.sqrMagnitude > 0.01f)
        {
            float currentZoom = _cmCamera.Lens.OrthographicSize;
            float unitsPerPixel = (currentZoom * 2f) / Screen.height;
            Vector3 move = new Vector3(-mouseDelta.x, -mouseDelta.y, 0) * unitsPerPixel;

            _cameraTarget.position = ClampTargetPosition(_cameraTarget.position + move);
        }
    }

    private void ApplyZoom()
    {
        LensSettings lens = _cmCamera.Lens;
        lens.OrthographicSize = _targetZoom;
        _cmCamera.Lens = lens;

        _cameraTarget.position = ClampTargetPosition(_cameraTarget.position);
    }

    private void HandleRightClick(RightClickEvent e)
    {
        if (_inputReader.IsPointerOverUI && e.IsStarted) return;
        _isDragging = e.IsStarted;
    }

    private void HandleScroll(ScrollEvent e)
    {
        if (_inputReader.IsPointerOverUI) return;

        float direction = e.Delta > 0 ? -1f : 1f;
        float nextZoom = _targetZoom + (direction * _zoomStep);

        bool isCrossingInitial = (_targetZoom > _initialZoom && nextZoom < _initialZoom) ||
                                 (_targetZoom < _initialZoom && nextZoom > _initialZoom);

        bool isAlreadyAtInitial = Mathf.Abs(_targetZoom - _initialZoom) < 0.01f;

        if (isCrossingInitial && !isAlreadyAtInitial)
        {
            _targetZoom = _initialZoom;
        }
        else
        {
            _targetZoom = Mathf.Clamp(nextZoom, _minZoom, _maxZoom);
        }

        ApplyZoom();
    }

    private Vector3 ClampTargetPosition(Vector3 targetPos)
    {
        if (_confinerCollider == null) return targetPos;

        Bounds b = _confinerCollider.bounds;
        float camHeight = _cmCamera.Lens.OrthographicSize;
        float camWidth = camHeight * _aspectRatio;

        float minX = b.min.x + camWidth;
        float maxX = b.max.x - camWidth;
        float minY = b.min.y + camHeight;
        float maxY = b.max.y - camHeight;

        if (minX > maxX) { float mid = (minX + maxX) / 2f; minX = maxX = mid; }
        if (minY > maxY) { float mid = (minY + maxY) / 2f; minY = maxY = mid; }

        float newX = Mathf.Clamp(targetPos.x, minX, maxX);
        float newY = Mathf.Clamp(targetPos.y, minY, maxY);

        return new Vector3(newX, newY, targetPos.z);
    }
}