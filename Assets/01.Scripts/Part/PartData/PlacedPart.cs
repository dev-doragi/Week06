using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
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
    private readonly List<BoxCollider2D> cellCols = new();

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

    private void Awake()
    {
        SyncLegacyAndOwnerData();
    }

    // ------------------------------------------------------------
    // 기존 초기화 방식 유지
    // 외부에서 직접 PartData를 넣는 기존 코드가 깨지지 않도록 유지한다.
    // ------------------------------------------------------------
    public void Initialize(PartData data, Vector2Int origin, int rotation, List<Vector2Int> occupiedCells)
    {
        this.data = data;
        this.origin = origin;
        this.rotation = rotation;

        this.occupiedCells.Clear();
        if (occupiedCells != null)
        {
            this.occupiedCells.AddRange(occupiedCells);
        }

        // 주요 라인: 기존 구조로 초기화되더라도 새 owner 필드를 항상 함께 동기화한다.
        _partKey = data != null ? data.Key : 0;
        _anchorCell = origin;

        if (data != null && currentHp <= 0f)
        {
            currentHp = data.Hp;
        }
    }

    // ------------------------------------------------------------
    // 새 구조용 초기화
    // PartData를 직접 알고 있는 현재 배치 흐름(BuildManager / EnemyBuilder)에 맞춘 방식
    // ------------------------------------------------------------
    public void SetPlacement(PartData data, Vector2Int anchorCell, int rotation, List<Vector2Int> occupiedCells = null)
    {
        if (data == null)
        {
            Debug.LogError($"{name}: SetPlacement 실패 - PartData가 Null입니다.");
            return;
        }

        this.data = data;
        _partKey = data.Key;
        _anchorCell = anchorCell;

        // 기존 구조와 호환
        origin = anchorCell;
        this.rotation = rotation;

        this.occupiedCells.Clear();

        if (occupiedCells != null && occupiedCells.Count > 0)
        {
            this.occupiedCells.AddRange(occupiedCells);
        }
        else
        {
            RebuildOccupiedCells();
        }

        if (currentHp <= 0f)
        {
            currentHp = data.Hp;
        }
    }

    // ------------------------------------------------------------
    // 새 구조용 초기화
    // key와 anchor cell만으로 세팅해야 하는 과도기 상황도 지원
    // ------------------------------------------------------------
    public void SetPlacement(int partKey, Vector2Int anchorCell)
    {
        _partKey = partKey;
        _anchorCell = anchorCell;

        // 기존 구조와 동기화
        origin = anchorCell;
        rotation = 0;

        ResolvePartData();
        RebuildOccupiedCells();

        if (data != null && currentHp <= 0f)
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

    private void SyncLegacyAndOwnerData()
    {
        // 주요 라인: 기존 방식으로 data가 직접 들어와 있으면 그것을 우선 신뢰한다.
        if (data != null)
        {
            _partKey = data.Key;
            _anchorCell = origin;

            if (occupiedCells == null)
            {
                occupiedCells = new List<Vector2Int>();
            }

            if (occupiedCells.Count == 0)
            {
                RebuildOccupiedCells();
            }

            if (currentHp <= 0f)
            {
                currentHp = data.Hp;
            }

            return;
        }

        // 주요 라인: 새 구조/저장 복구 구조에서는 key 기반 fallback을 지원한다.
        if (_partKey > 0)
        {
            ResolvePartData();
            RebuildOccupiedCells();

            origin = _anchorCell;

            if (data != null && currentHp <= 0f)
            {
                currentHp = data.Hp;
            }
        }
    }

    public void ResolvePartData()
    {
        // 주요 라인: 이미 owner가 data를 들고 있으면 그것을 최우선으로 사용한다.
        if (data != null)
        {
            _partKey = data.Key;
            return;
        }

        if (_partKey <= 0)
        {
            Debug.LogError($"{name}: ResolvePartData 실패 - _partKey가 0 이하입니다. 입력값: {_partKey}");
            return;
        }

        if (GridManager.instance == null)
        {
            Debug.LogError($"{name}: ResolvePartData 실패 - GridManager.instance가 Null입니다.");
            return;
        }

        if (GridManager.instance.partDic == null)
        {
            Debug.LogError($"{name}: ResolvePartData 실패 - GridManager.partDic가 Null입니다.");
            return;
        }

        if (!GridManager.instance.partDic.TryGetValue(_partKey, out PartData foundData))
        {
            Debug.LogError($"{name}: ResolvePartData 실패 - key {_partKey}에 해당하는 PartData를 찾을 수 없습니다.");
            return;
        }

        data = foundData;
        data.IsValid();
    }

    public void RebuildOccupiedCells()
    {
        if (occupiedCells == null)
        {
            occupiedCells = new List<Vector2Int>();
        }

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

        // 주요 라인: GridBoard가 없더라도 기존 shape를 기준으로 셀을 재구성한다.
        for (int i = 0; i < data.Shape.Count; i++)
        {
            Vector2Int localCell = RotateLocalCell(data.Shape[i], rotation);
            Vector2Int worldCell = _anchorCell + localCell;
            occupiedCells.Add(worldCell);
        }
    }

    private Vector2Int RotateLocalCell(Vector2Int localCell, int rotation)
    {
        int normalizedRotation = ((rotation % 4) + 4) % 4;

        switch (normalizedRotation)
        {
            case 0:
                return localCell;
            case 1:
                return new Vector2Int(-localCell.y, localCell.x);
            case 2:
                return new Vector2Int(-localCell.x, -localCell.y);
            case 3:
                return new Vector2Int(localCell.y, -localCell.x);
            default:
                return localCell;
        }
    }

    public void BuildVisual(GridRenderer gridRenderer, Transform visualParent, Color color)
    {
        ClearVisual();

        if (gridRenderer == null)
        {
            Debug.LogError($"{name}: BuildVisual 실패 - GridRenderer가 Null입니다.");
            return;
        }

        if (data == null)
        {
            ResolvePartData();
        }

        if (data == null)
        {
            Debug.LogError($"{name}: BuildVisual 실패 - PartData가 Null입니다.");
            return;
        }

        foreach (Vector2Int cell in occupiedCells)
        {
            GameObject cellObj = new GameObject($"Cell_{cell.x}_{cell.y}");
            cellObj.transform.SetParent(visualParent != null ? visualParent : transform, false);
            cellObj.transform.localPosition = gridRenderer.GridToLocal(cell);

            SpriteRenderer sr = cellObj.AddComponent<SpriteRenderer>();
            BoxCollider2D boxCol = cellObj.AddComponent<BoxCollider2D>();
            boxCol.size = new Vector2(1, 1);

            sr.sprite = data.Icon;
            sr.color = color;

            cellObj.transform.localScale = new Vector3(gridRenderer.cellSize, gridRenderer.cellSize, 1f);

            cellRenderers.Add(sr);
            cellCols.Add(boxCol);
        }
    }

    public void SetColor(Color color)
    {
        foreach (SpriteRenderer sr in cellRenderers)
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
        cellCols.Clear();
    }

    public void ShakeX()
    {
        if (transform.parent == null)
        {
            return;
        }

        if (DOTween.IsTweening(transform.parent))
        {
            return;
        }

        transform.parent.DOPunchPosition(new Vector3(0.3f, 0f, 0f), 0.25f, 12, 0.8f);
    }

    public void DecreaseHp(float damage)
    {
        if (data == null)
        {
            ResolvePartData();
        }

        if (data == null)
        {
            Debug.LogError($"{name}: DecreaseHp 실패 - PartData가 Null입니다.");
            return;
        }

        currentHp -= damage;

        float value = 0.5f + ((currentHp / data.Hp) / 2f);
        Color color = new Color(1f, value, value, 1f);
        SetColor(color);

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

    public void DestroyAnim()
    {
        StartCoroutine(DropAnim());
    }

    private IEnumerator DropAnim()
    {
        Rigidbody2D rigid = gameObject.AddComponent<Rigidbody2D>();
        rigid.bodyType = RigidbodyType2D.Dynamic;

        Vector2 explosionVector = new Vector2(Random.Range(-0.7f, 0.7f), 1f);

        foreach (BoxCollider2D col in cellCols)
        {
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        rigid.AddForce(explosionVector * 2f, ForceMode2D.Impulse);

        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }
}