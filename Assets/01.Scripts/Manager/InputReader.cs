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
        _clickAction.started += _ => EventBus.Instance.Publish(new ClickEvent { IsStarted = true });
        _clickAction.canceled += _ => EventBus.Instance.Publish(new ClickEvent { IsStarted = false });

        _rightClickAction.started += _ => EventBus.Instance.Publish(new RightClickEvent { IsStarted = true });
        _rightClickAction.canceled += _ => EventBus.Instance.Publish(new RightClickEvent { IsStarted = false });

        _rotateAction.performed += _ => EventBus.Instance.Publish(new RotateEvent());

        _scrollAction.performed += ctx =>
        {
            float scrollValue = ctx.ReadValue<Vector2>().y;
            if (Mathf.Abs(scrollValue) > 0.01f)
            {
                EventBus.Instance.Publish(new ScrollEvent { Delta = scrollValue });
            }
        };

        _pauseAction.performed += _ => EventBus.Instance.Publish(new PausePressedEvent());
    }

    private void OnGameStateChanged(GameStateChangedEvent evt)
    {
        switch (evt.NewState)
        {
            case GameState.Playing:
                _playerMap.Enable();
                break;
            case GameState.Paused:
            case GameState.GameOver:
            case GameState.GameClear:
                _playerMap.Disable();
                break;
        }
    }

    public Vector2 GetMousePosition() => _pointAction?.ReadValue<Vector2>() ?? Vector2.zero;
    public Vector2 GetMouseDelta() => Mouse.current.delta.ReadValue();
}