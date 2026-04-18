
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;


public class PlacementManager : Singleton<PlacementManager>
{
    private int _currentMouseCount = 1;

    private List<GameObject> _activeMice = new List<GameObject>();

    [Header("Mouse Spawn Data")]
    [SerializeField] private TextMeshProUGUI _countDisplay;
    [SerializeField] private GameObject _mousePrefab;
    [SerializeField] private Transform _spawnLocation;

    [Header("Mouse Movement Bound Data")]
    [SerializeField] private RectTransform _movementBounds;

    [Header("UI References")]
    [Tooltip("The parent container holding the text/icon/background")]
    [SerializeField] private RectTransform _uiContainer; 
    [SerializeField] private Image _UISprite;
    
    [Header("Animation Settings")]
    [SerializeField] private Color _denyColor = Color.red;
    [SerializeField] private Color _fullColor = Color.green;
    [SerializeField] private float _shakeDuration = 0.3f;
    [SerializeField] private float _shakeAmount = 10f;


    [Header("Mouse Count")]
    public int CurrentMouse => _currentMouseCount;
    [SerializeField] private int _maxSpawnCount = 500;

    [Header("Mouse Generation / Consumtion")]
    [SerializeField] private int _generatorCount = 0;
    [SerializeField] private int _spellmapCount = 0;

    public int SpellMapCount => _spellmapCount;


    [Header("Rates")]
    [SerializeField] private int subtractAmount = 3;
    [SerializeField] private float _mouseGenPerGenerator = 0.3f;
    [SerializeField] private float _mouseSubPerSpellMap = 0.2f;
    [SerializeField] private float _addGaugeProgress = 0f;
    [SerializeField] private float _subGaugeProgress = 0f;

    [Header("Rates")]
    [SerializeField] private Slider _addGauge;
    [SerializeField] private Slider _subGauge;


    private Vector2 _anchoredPosition; // Best for UI stability
    private bool _isAnimating = false;
    private bool _isFull = false;


    void Start()
    {
        SpawnMouseAtPoint(_currentMouseCount);
        UpdateDisplay();

        if (_uiContainer != null) _anchoredPosition = _uiContainer.anchoredPosition;


        _UISprite.canvasRenderer.SetAlpha(0f);
        _UISprite.color = _denyColor;

    }

    private void Update()
    {
        HandleAdding();
        HandleSubtracting();
    }

    private void HandleAdding()
    {
         _addGauge.value = _addGaugeProgress;
        // Don't progress the gauge if we are already at max capacity
        if (_currentMouseCount >= _maxSpawnCount)
        {
            _addGaugeProgress = 1f; // Optional: Keep it full or reset it
            return;
        }

        float addRate = _generatorCount * _mouseGenPerGenerator;
        if (addRate > 0)
        {
            _addGaugeProgress += addRate * Time.deltaTime;

            if (_addGaugeProgress >= 1f)
            {
                // Double check capacity before adding
                if (!_isFull) 
                { 
                    AddMouseCount(1);
                    _addGaugeProgress = 0f;
                }
            }
        }
    }

    private void HandleSubtracting()
    {
        // 1. Check if we are broke FIRST
        if (_currentMouseCount <= 0)
        {
            _subGaugeProgress = 1f; // Keep it visibly "Full" or "Danger"
        }
        else
        {
            // 2. Only progress the logic if we actually have mice to lose
            float subRate = _spellmapCount * _mouseSubPerSpellMap;
            if (subRate > 0)
            {
                _subGaugeProgress += subRate * Time.deltaTime;

                if (_subGaugeProgress >= 1f)
                {
                    // if (! (subtractAmount > _currentMouseCount))
                    // {
                    //     _subGaugeProgress = 0f;
                    //     SubtractMouseCount(subtractAmount); 
                    // }
                }
            }
            else
            {
                // Optional: If no spellmaps are active, should the gauge empty?
                _subGaugeProgress = 0f; 
            }
        }

        // 3. Update Visuals LAST so the UI reflects the logic above immediately
        _subGauge.value = _subGaugeProgress;
    }

    #region Generator Add / Subtract
    public void AddGenerator()
    {
        _generatorCount++;
    }

    public void SubtractGenerator()
    {
        _generatorCount--;
    }

    public void AddSpellGenerator()
    {
        _spellmapCount++;
    }

    public void SubtractSpellGenerator()
    {
        _spellmapCount--;
    }

    #endregion


    public void AddMouseCount(int amount)
    {
        if (_currentMouseCount >= _maxSpawnCount)
        {
            Debug.Log("[PlacementManager]: Cannot add more mouse, had reached the max");
            PlayDenyAction(false);
            _isFull = true;
            return;
        }

        if (_currentMouseCount + amount > _maxSpawnCount)
        {
            Debug.Log("[PlacementManager]: Mouse number as reached the max");
            amount = _maxSpawnCount - _currentMouseCount;
        }

        _currentMouseCount += amount;

        SpawnMouseAtPoint(amount);
        UpdateDisplay();
    }

    public void SubtractMouseCount(int amount)
    {
        if (_currentMouseCount < amount)
        {
            PlayDenyAction(true);
            Debug.Log("[StoreManager]: Not Enough money!");
            return;
        }

        _isFull = false;
        _currentMouseCount = Mathf.Max(0, _currentMouseCount - amount);
        DespawnMouseAPoint(amount);
        UpdateDisplay();
    }

    public void ResetMouseCount()
    {
        DespawnMouseAPoint(_activeMice.Count);
        _currentMouseCount = 0;
        UpdateDisplay();
    }

    // This property allows your UI Slider to see the progress
    private void UpdateDisplay()
    {
        if (_countDisplay != null)
        {
            _countDisplay.text = _currentMouseCount.ToString() + " / " + _maxSpawnCount.ToString();
        }
    }  

    private void SpawnMouseAtPoint(int number)
    {
        for (int i = 0; i < number; i++)
        {
            GameObject obj = PoolManager.Instance.Spawn(_mousePrefab.name, _spawnLocation.position, Quaternion.identity);

            if (obj != null)
            {
                _activeMice.Add(obj);

                if (obj.TryGetComponent(out MouseAgent agent))
                {
                    agent.Setup(_movementBounds);
                }
            }
        }
    }

    private void DespawnMouseAPoint(int number)
    {
        
        int count = Mathf.Min(number, _activeMice.Count);

        for (int i = 0; i < count; i++)
        {
            int lastIndex = _activeMice.Count - 1;
            GameObject target = _activeMice[lastIndex];

            
            PoolManager.Instance.Despawn(target);

            _activeMice.RemoveAt(lastIndex);
        }
    }


    public void PlayDenyAction(bool isZero)
    {
        if (!_isAnimating && _uiContainer != null) 
        {
            StartCoroutine(DenyFeedbackRoutine(isZero));
        }
    }

    private System.Collections.IEnumerator DenyFeedbackRoutine(bool _isDenied)
    {
        _isAnimating = true;
        float elapsed = 0f;

        if(_UISprite != null) 
        {
            _UISprite.canvasRenderer.SetAlpha(0.25f);

            _UISprite.color = _isDenied? _denyColor: _fullColor;
        }

        while (elapsed < _shakeDuration)
        {
            // We use anchoredPosition to move relative to its UI anchors
            float xOffset = UnityEngine.Random.Range(-1f, 1f) * _shakeAmount;
            _uiContainer.anchoredPosition = _anchoredPosition + new Vector2(xOffset, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Reset to exact starting state
        _uiContainer.anchoredPosition = _anchoredPosition;
        if(_UISprite != null) 
        {
            _UISprite.canvasRenderer.SetAlpha(0f);
        }
        
        _isAnimating = false;
    }
}