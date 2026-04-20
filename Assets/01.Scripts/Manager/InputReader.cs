using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputReader : Singleton<InputReader>
{
    private PlayerInput _playerInput;

    private InputActionMap _playerMap;
    private InputActionMap _systemMap;

    private InputAction _clickAction;
    private InputAction _rightClickAction;
    private InputAction _rotateAction;
    private InputAction _pointAction;
    private InputAction _scrollAction;
    private InputAction _pauseAction;

    public bool IsPointerOverUI { get; private set; }

    // 튜토리얼/모달용 입력 차단 플래그
    private bool _isTutorialBlocked = false;

    protected override void Init()
    {
        _playerInput = GetComponent<PlayerInput>();

        _playerMap = _playerInput.actions.FindActionMap("Player", true);
        _systemMap = _playerInput.actions.FindActionMap("System", true);

        _clickAction = _playerMap.FindAction("Click", true);
        _rightClickAction = _playerMap.FindAction("RightClick", true);
        _rotateAction = _playerMap.FindAction("Rotate", true);
        _pointAction = _playerMap.FindAction("Point", true);
        _scrollAction = _playerMap.FindAction("Scroll", true);

        _pauseAction = _systemMap.FindAction("Pause", true);

        BindEvents();
    }

    private void OnEnable()
    {
        if (EventBus.Instance != null)
        {
            EventBus.Instance.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
        }
    }

    private void OnDisable()
    {
        if (EventBus.Instance != null)
        {
            EventBus.Instance.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
        }
    }

    private void Update()
    {
        if (EventSystem.current != null)
        {
            IsPointerOverUI = EventSystem.current.IsPointerOverGameObject();
        }
    }

    private void BindEvents()
    {
        _clickAction.started += _ =>
        {
            if (_isTutorialBlocked) return;
            EventBus.Instance.Publish(new ClickEvent { IsStarted = true });
        };
        _clickAction.canceled += _ =>
        {
            if (_isTutorialBlocked) return;
            EventBus.Instance.Publish(new ClickEvent { IsStarted = false });
        };

        _rightClickAction.started += _ =>
        {
            if (_isTutorialBlocked) return;
            EventBus.Instance.Publish(new RightClickEvent { IsStarted = true });
        };
        _rightClickAction.canceled += _ =>
        {
            if (_isTutorialBlocked) return;
            EventBus.Instance.Publish(new RightClickEvent { IsStarted = false });
        };

        _rotateAction.performed += _ =>
        {
            if (_isTutorialBlocked) return;
            EventBus.Instance.Publish(new RotateEvent());
        };

        _scrollAction.performed += ctx =>
        {
            if (_isTutorialBlocked) return;
            float scrollValue = ctx.ReadValue<Vector2>().y;
            if (Mathf.Abs(scrollValue) > 0.01f)
            {
                EventBus.Instance.Publish(new ScrollEvent { Delta = scrollValue });
            }
        };

        _pauseAction.performed += _ =>
        {
            // Pause는 튜토리얼 중에도 허용하지 않으려면 아래 검사 추가 가능
            if (_isTutorialBlocked) return;
            EventBus.Instance.Publish(new PausePressedEvent());
        };
    }

    private void OnGameStateChanged(GameStateChangedEvent evt)
    {
        switch (evt.NewState)
        {
            case GameState.Playing:
                // 튜토리얼/모달이 활성화된 동안에는 맵을 활성화하지 않음
                if (!_isTutorialBlocked)
                    _playerMap.Enable();
                break;
            case GameState.Paused:
            case GameState.GameOver:
            case GameState.GameClear:
                _playerMap.Disable();
                break;
        }
    }

    public void SetInputBlocked(bool blocked)
    {
        _isTutorialBlocked = blocked;

        if (_playerMap == null) return;

        if (blocked)
        {
            _playerMap.Disable();
        }
        else
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
                _playerMap.Enable();
        }
    }

    public Vector2 GetMousePosition() => _pointAction?.ReadValue<Vector2>() ?? Vector2.zero;
    public Vector2 GetMouseDelta() => Mouse.current.delta.ReadValue();
}