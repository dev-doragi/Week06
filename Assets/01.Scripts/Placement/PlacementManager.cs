
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;


public class PlacementManager : Singleton<PlacementManager>
{
    [SerializeField] private int _currentMouseCount = 1;

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

    [SerializeField] private int _alterMouseCostPerSecond = 1;
    [SerializeField] private float _mouseDrainInterval = 1f;

    private int _altarCount;
    private float _mouseDrainTimer;
    private bool _altarSupportEnabled = true;

    private bool _isWavePlaying;

    public int SpellMapCount => _spellmapCount;
    public bool IsAltarSupportEnabled => _altarSupportEnabled;
    public int TotalAltarMouseDrainPerSecond => _altarCount * _alterMouseCostPerSecond;

    [Header("Rates")]
    [SerializeField] private int subPerSpell = 0;
    [SerializeField] private float _mouseGenTime = 0.3f;
    [SerializeField] private float _mouseSubTime = 0.5f;
    [SerializeField] private float _addGaugeProgress = 0f;
    [SerializeField] private float _subGaugeProgress = 0f;

    [Header("Rates")]
    [SerializeField] private Slider _addGauge;
    [SerializeField] private Slider _subGauge;

    [Header ("buff state")]
    public bool BuffIsEnabled = false;

    private Vector2 _anchoredPosition; // Best for UI stability
    private bool _isAnimating = false;
    private bool _isFull = false;
    private int usedMouse;


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
        if (_isWavePlaying && _generatorCount > 0)
        {
            HandleAdding();
        }
        else
        {
            _addGaugeProgress = 0f;
            if (_addGauge != null)
            {
                _addGauge.value = 0f;
            }
        }

        usedMouse = _spellmapCount + subPerSpell;
        if (usedMouse > 0 && _isWavePlaying)
        {
            HandleSubtracting();
        }
        else
        {
            _subGaugeProgress = 0f;
            if (_subGauge != null)
            {
                _subGauge.value = 0f;
            }
        }


        //ProcessAltarMouseDrain();
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

        float addRate =  _mouseGenTime;
        if (addRate > 0)
        {
            _addGaugeProgress += addRate * Time.deltaTime;

            if (_addGaugeProgress >= 1f)
            {
                // Double check capacity before adding
                if (!_isFull) 
                { 
                    AddMouseCount(_generatorCount);
                    _addGaugeProgress = 0f;
                }
            }
        }
    }

    private void HandleSubtracting()
    {
        

        if (_mouseSubTime < 0) return;

        // get progress value but Time
        _subGaugeProgress += _mouseSubTime * Time.deltaTime;

        if (_subGaugeProgress >= 1f)
        {
            if (! ((usedMouse) > _currentMouseCount))
            {
                _subGaugeProgress = 0f;
                SubtractMouseCount(usedMouse);
                BuffIsEnabled = true;
            }
            else
            {
                BuffIsEnabled = false;
            }
        }
        // 3. Update Visuals LAST so the UI reflects the logic above immediately
        _subGauge.value = _subGaugeProgress;
    }

    private void OnEnable()
    {
        if (EventBus.Instance != null)
        {
            EventBus.Instance.Subscribe<InGameStateChangedEvent>(OnInGameStateChanged);
        }
    }

    private void OnDisable()
    {
        if (EventBus.Instance != null)
        {
            EventBus.Instance.Unsubscribe<InGameStateChangedEvent>(OnInGameStateChanged);
        }
    }

    private void OnInGameStateChanged(InGameStateChangedEvent evt)
    {
        _isWavePlaying = evt.NewState == InGameState.WavePlaying;

        if (!_isWavePlaying)
        {
            _addGaugeProgress = 0f;
            if (_addGauge != null)
            {
                _addGauge.value = 0f;
            }
        }
    }

    #region Generator Add / Subtract
    public void AddGenerator(int count)
    {
        _generatorCount += count;
    }

    public void SubtractGenerator(int count)
    {
        _generatorCount -= count;
    }

    public void AddSpellGenerator(int count)
    {
        _spellmapCount += count;
    }

    public void SubtractSpellGenerator(int count)
    {
        _spellmapCount = Mathf.Max(0, _spellmapCount - 1);
    }

    #endregion

    /*
    #region Altar
    public void AddAltar()
    {
        _altarCount++;
    }

    public void RemoveAltar()
    {
        _altarCount = Mathf.Max(0, _altarCount - 1);
    }

    private void ProcessAltarMouseDrain()
    {
        if (_altarCount <= 0)
        {
            _mouseDrainTimer = 0f;
            _altarSupportEnabled = true;
            return;
        }

        _mouseDrainTimer += Time.deltaTime;
        if (_mouseDrainTimer < _mouseDrainInterval)
        {
            return;
        }

        _mouseDrainTimer -= _mouseDrainInterval;

        int drainAmount = TotalAltarMouseDrainPerSecond;
        if (drainAmount <= 0)
        {
            _altarSupportEnabled = true;
            return;
        }

        if (_currentMouseCount < drainAmount)
        {
            // 주요 라인: mouse가 부족하면 이번 틱에는 소비하지 않고 제단 버프를 비활성화한다.
            _altarSupportEnabled = false;
            return;
        }

        SubtractMouseCount(drainAmount);
        _altarSupportEnabled = true;
    }
    #endregion
    */


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

    public bool SubtractMouseCount(int amount)
    {
        Debug.Log("@@@");
        if (_currentMouseCount < amount)
        {
            PlayDenyAction(true);
            Debug.Log("[StoreManager]: Not Enough money!");
            return false;
        }

        _isFull = false;
        _currentMouseCount = Mathf.Max(0, _currentMouseCount - amount);
        DespawnMouseAPoint(amount);
        UpdateDisplay();
        return true;
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