using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class PlacedPart : SerializedMonoBehaviour
{
    [Header("Legacy / Existing Fields")]
    [SerializeField] public PartData data;
    public Vector2Int origin;
    public int rotation;
    public List<Vector2Int> occupiedCells = new();

    [SerializeField] private float currentHp;

    private readonly List<SpriteRenderer> cellRenderers = new();

    [Header("Added For Grid/Owner Structure")]
    [SerializeField] private int _partKey;
    [SerializeField] private Vector2Int _anchorCell;

    public float CurrentHp => currentHp;

    // ------------------------------------------------------------
    // 새 구조에서 사용하는 읽기 전용 프로퍼티
    // ------------------------------------------------------------
    public int PartKey => _partKey;
    public Vector2Int AnchorCell => _anchorCell;
    public IReadOnlyList<Vector2Int> OccupiedCells => occupiedCells;
    public bool IsInitialized => data != null;

    // ------------------------------------------------------------
    // 기존 초기화 방식 유지
    // 외부에서 직접 PartData를 넣는 기존 코드가 깨지지 않도록 유지한다.
    // ------------------------------------------------------------
    public void Initialize(PartData data, Vector2Int origin, int rotation, List<Vector2Int> occupiedCells)
    {
        this.data = data;
        this.origin = origin;
        this.rotation = rotation;
        this.occupiedCells = new List<Vector2Int>(occupiedCells);

        // 기존 구조와의 호환성을 위해 함께 동기화
        _partKey = data != null ? data.Key : 0;
        _anchorCell = origin;

        currentHp = data != null ? data.Hp : 0f;
    }

    // ------------------------------------------------------------
    // 새 구조용 초기화
    // key와 anchor cell만으로 배치 정보를 세팅할 수 있다.
    // ------------------------------------------------------------
    public void SetPlacement(int partKey, Vector2Int anchorCell)
    {
        _partKey = partKey;
        _anchorCell = anchorCell;

        ResolvePartData();
        RebuildOccupiedCells();

        // 기존 필드와 동기화
        origin = anchorCell;
        rotation = 0;

        if (data != null)
        {
            currentHp = data.Hp;
        }
    }

    public void SetAnchorCell(Vector2Int anchorCell)
    {
        _anchorCell = anchorCell;

        // 기존 필드와 동기화
        origin = anchorCell;

        RebuildOccupiedCells();
    }

    private void Awake()
    {
        // 기존 방식으로 data가 직접 들어와 있으면 그것을 우선 사용
        if (data != null)
        {
            _partKey = data.Key;
            _anchorCell = origin;

            if (occupiedCells == null || occupiedCells.Count == 0)
            {
                RebuildOccupiedCells();
            }

            if (currentHp <= 0f)
            {
                currentHp = data.Hp;
            }

            return;
        }

        // 새 구조에서는 key 기반으로 조회
        if (_partKey > 0)
        {
            ResolvePartData();
            RebuildOccupiedCells();

            origin = _anchorCell;
            rotation = 0;

            if (data != null && currentHp <= 0f)
            {
                currentHp = data.Hp;
            }
        }
    }

    private void ResolvePartData()
    {
        if (_partKey <= 0)
        {
            Debug.LogError($"{name}: ResolvePartData 실패 - _partKey가 0 이하입니다. 입력값: {_partKey}");
            return;
        }

        if (PartDatabaseProvider.Instance == null)
        {
            Debug.LogError($"{name}: ResolvePartData 실패 - PartDatabaseProvider.Instance가 Null입니다.");
            return;
        }

        data = PartDatabaseProvider.Instance.GetPartData(_partKey);
        if (data == null)
        {
            Debug.LogError($"{name}: ResolvePartData 실패 - key {_partKey}에 해당하는 PartData를 찾을 수 없습니다.");
            return;
        }

        data.IsValid();
    }

    private void RebuildOccupiedCells()
    {
        occupiedCells.Clear();

        if (data == null)
        {
            Debug.LogError($"{name}: RebuildOccupiedCells 실패 - data가 Null입니다.");
            return;
        }

        if (data.Shape == null || data.Shape.Count == 0)
        {
            Debug.LogError($"{name}: RebuildOccupiedCells 실패 - Shape가 비어 있습니다.");
            return;
        }

        for (int i = 0; i < data.Shape.Count; i++)
        {
            Vector2Int localCell = data.Shape[i];
            Vector2Int worldCell = _anchorCell + localCell;
            occupiedCells.Add(worldCell);
        }
    }

    public void BuildVisual(GridRenderer gridRenderer, Transform visualParent, Color color)
    {
        ClearVisual();

        foreach (var cell in occupiedCells)
        {
            GameObject cellObj = new GameObject($"Cell_{cell.x}_{cell.y}");
            cellObj.transform.SetParent(visualParent != null ? visualParent : transform);

            cellObj.transform.position = gridRenderer.GridToWorld(cell);

            SpriteRenderer sr = cellObj.AddComponent<SpriteRenderer>();
            sr.sprite = data != null ? data.Icon : null;
            sr.color = color;

            float scale = gridRenderer.cellSize;
            cellObj.transform.localScale = new Vector3(scale, scale, 1f);

            cellRenderers.Add(sr);
        }

        // 기존 기능 유지: BoxCollider2D 추가
        BoxCollider2D boxCol = GetComponent<BoxCollider2D>();
        if (boxCol == null)
        {
            boxCol = gameObject.AddComponent<BoxCollider2D>();
        }

        boxCol.size = new Vector2(1, 1);
    }

    public void SetColor(Color color)
    {
        foreach (var sr in cellRenderers)
        {
            if (sr != null)
            {
                sr.color = color;
            }
        }
    }

    public void ClearVisual()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        cellRenderers.Clear();
    }

    public void DecreaseHp(float damage)
    {
        currentHp -= damage;

        if (currentHp <= 0f)
        {
            BuildManager.Instance.BrokenPart(this);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 여기서 무기 충돌 판정
        // DecreaseHp(float damage)
    }
}