using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputReader : Singleton<InputReader>
{
    private PlayerInput _playerInput;

    private InputAction _clickAction;
    private InputAction _rightClickAction;
    private InputAction _rotateAction;
    private InputAction _pointAction;
    private InputAction _scrollAction;

    public bool IsPointerOverUI { get; private set; }

    protected override void Init()
    {
        _playerInput = GetComponent<PlayerInput>();
        if (_playerInput == null)
        {
            Debug.LogError("InputReader: PlayerInput 컴포넌트가 누락되었습니다.");
            return;
        }

        InputActionMap map = _playerInput.actions.FindActionMap("Player", true);

        _clickAction = map.FindAction("Click", true);
        _rightClickAction = map.FindAction("RightClick", true);
        _rotateAction = map.FindAction("Rotate", true);
        _pointAction = map.FindAction("Point", true);
        _scrollAction = map.FindAction("Scroll", true);

        BindEvents();
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
        // EventBus를 통한 이벤트 전파
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
    }

    // --- 폴링 메서드 (규칙 준수: _camelCase 필드 참조) ---
    public Vector2 GetMousePosition() => _pointAction?.ReadValue<Vector2>() ?? Vector2.zero;
    public Vector2 GetMouseDelta() => Mouse.current.delta.ReadValue();
}