using UnityEngine;

public class AltarConnector : MonoBehaviour
{
    private PlacementManager _placementManager;
    private bool _isRegistered;

    public bool IsAltarActive
    {
        get
        {
            return _placementManager != null && _placementManager.IsAltarSupportEnabled;
        }
    }

    private void Awake()
    {
        _placementManager = FindFirstObjectByType<PlacementManager>();
        if (_placementManager == null)
        {
            Debug.LogError($"{name}: PlacementManager를 찾을 수 없습니다.");
        }
    }

    private void OnEnable()
    {
        TryRegister();
    }

    private void OnDisable()
    {
        TryUnregister();
    }

    private void OnDestroy()
    {
        TryUnregister();
    }

    private void TryRegister()
    {
        if (_isRegistered)
        {
            return;
        }

        if (_placementManager == null)
        {
            return;
        }

        _placementManager.AddSpellGenerator(1);
        _isRegistered = true;
    }

    private void TryUnregister()
    {
        if (!_isRegistered)
        {
            return;
        }

        if (_placementManager == null)
        {
            return;
        }

        _placementManager.SubtractGenerator(10);
        _isRegistered = false;
    }
}