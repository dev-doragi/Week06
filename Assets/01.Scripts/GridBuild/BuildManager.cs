using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

//GridBuilder Manager아님
public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance;

    [SerializeField] private GridBoard board;
    [SerializeField] private GridRenderer gridRenderer;
    [SerializeField] private Transform placedPartsRoot;
    [SerializeField] private Transform ghostRoot;
    [SerializeField] private Transform placeableHighlightRoot;
    [SerializeField] private Sprite highlightSprite;

    [SerializeField] private PartRuntimeSpawner _partRuntimeSpawner;
    [SerializeField] private TeamType _teamType = TeamType.Player;

    [SerializeField] private PartPrefabCatalog _partPrefabCatalog;
    [SerializeField] private bool _spawnRuntimePrefab = true;

    private readonly List<GameObject> placeableHighlights = new();

    private PartData currentPartData;
    private int currentRotation;
    private PlacedPart ghostPart;

    private bool haveMouse;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (VehicleCache.HasSavedData) return;

        SpawnBase();
    }

    private void Update()
    {
        if (currentPartData != null && haveMouse)
        {
            UpdateGhost();

            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                currentRotation = (currentRotation + 1) % 4;
            }
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryPlaceCurrentPart();
        }

        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            ClearSelection();
            TryRemovePart();
        }
    }

    public void SelectPart(int key)
    {
        if (!GridManager.instance.partDic.TryGetValue(key, out currentPartData))
        {
            currentPartData = null;
            haveMouse = false;

            if (ghostPart != null)
            {
                Destroy(ghostPart.gameObject);
                ghostPart = null;
            }
            ClearPlaceableHighlights();
            return;
        }

        haveMouse = true;
        currentRotation = 0;

        if (ghostPart != null)
        {
            Destroy(ghostPart.gameObject);
            ghostPart = null;
        }

        CreateGhost();
        ShowPlaceableCells();
    }

    private void CreateGhost()
    {
        if (currentPartData == null || ghostRoot == null) return;

        GameObject ghostObj = new GameObject("GhostPart");
        ghostObj.transform.SetParent(ghostRoot);

        ghostPart = ghostObj.AddComponent<PlacedPart>();
    }

    private Vector2Int GetMouseGridPosition()
    {
        if (Mouse.current == null)
            return Vector2Int.zero;

        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);
        mouseWorld.z = 0f;

        return gridRenderer.WorldToGrid(mouseWorld);
    }

    private void UpdateGhost()
    {
        if (ghostPart == null)
            CreateGhost();

        if (ghostPart == null || currentPartData == null)
            return;

        Vector2Int gridPos = GetMouseGridPosition();
        List<Vector2Int> targetCells = board.GetRotatedCells(currentPartData, gridPos, currentRotation);

        ghostPart.Initialize(currentPartData, gridPos, currentRotation, targetCells);
        ghostPart.BuildVisual(gridRenderer, ghostPart.transform, new Color(1f, 1f, 1f, 0.45f), true);

        bool canPlace = board.CanPlacePartByRules(currentPartData, gridPos, currentRotation);
        ghostPart.SetColor(canPlace ? new Color(0f, 1f, 0f, 0.45f) : new Color(1f, 0f, 0f, 0.45f));
    }

    public void TryPlaceCurrentPart()
    {
        if (currentPartData == null)
            return;

        Vector2Int gridPos = GetMouseGridPosition();
        PlacePartInternal(currentPartData, gridPos, currentRotation);
    }

    private bool PlacePartInternal(PartData partData, Vector2Int gridPos, int rotation)
    {
        if (partData == null)
        {
            Debug.LogError($"{name}: PlacePartInternal 실패 - partData가 Null입니다.");
            return false;
        }

        if (!board.CanPlacePartByRules(partData, gridPos, rotation))
        {
            return false;
        }
        /*
        if (!PlacementManager.Instance.SubtractMouseCount(partData.Cost))
            return;
        */
        GameObject partObj = CreatePlacedPartObject(partData, gridPos);
        if (partObj == null) return false;

        PlacedPart placedPart = partObj.AddComponent<PlacedPart>();

        bool success = board.PlacePart(partData, gridPos, rotation, placedPart);

        if (!success)
        {
            Destroy(partObj);
            return false;
        }

        placedPart.BuildVisual(gridRenderer, placedPart.transform, Color.white);
        TrySpawnRuntimePrefab(partData, placedPart);

        ShowPlaceableCells();
        return true;
    }

    private GameObject CreatePlacedPartObject(PartData partData, Vector2Int gridPos)
    {
        GameObject partObj = new GameObject($"Placed_{partData.PartName}");
        partObj.transform.SetParent(placedPartsRoot);

        partObj.transform.position = gridRenderer.GridToWorld(gridPos);

        return partObj;
    }

    private void TrySpawnRuntimePrefab(PartData partData, PlacedPart placedPart)
    {
        if (_partRuntimeSpawner == null)
        {
            Debug.LogWarning($"{name}: PartRuntimeSpawner가 연결되지 않았습니다.");
            return;
        }
        GridBoard gridBoard = placedPart.GetComponentInParent<GridBoard>();
        _partRuntimeSpawner.SpawnRuntime(partData, placedPart, _teamType, gridBoard);
    }

    private void TryRemovePart()
    {
        Vector2Int gridPos = GetMouseGridPosition();

        if (!board.IsInside(gridPos)) return;

        PlacedPart targetPart = board.GetCell(gridPos);
        if (targetPart == null) return;

        if (board.HasPartAbove(targetPart))
        {
            Debug.LogWarning("위에 파츠가 있어서 제거할 수 없습니다.");
            return;
        }

        RemovePartAndCollapse(targetPart);
    }

    public void BrokenPart(PlacedPart brokenPart)
    {
        if (brokenPart == null) return;

        RemovePartAndCollapse(brokenPart);
    }

    private void RemovePartAndCollapse(PlacedPart targetPart)
    {
        if (targetPart == null) return;
        GridBoard targetBoard = targetPart.GetComponentInParent<GridBoard>();
        if(targetBoard == null) return;
        // 1. 먼저 대상 파츠 제거
        targetBoard.RemovePart(targetPart);
        targetPart.DestroyAnim();

        // 2. 바퀴와 연결 안 된 모든 파츠 찾기
        List<PlacedPart> disconnectedParts = targetBoard.GetDisconnectedParts();

        // 3. 연결 안 된 파츠들 전부 제거
        foreach (var part in disconnectedParts)
        {
            if (part == null) continue;
            targetBoard.RemovePart(part);
            part.DestroyAnim();
        }
    }

    private void ClearSelection()
    {
        haveMouse = false;
        currentPartData = null;
        currentRotation = 0;
        ClearPlaceableHighlights();
        if (ghostPart != null)
        {
            Destroy(ghostPart.gameObject);
            ghostPart = null;
        }
    }

    private void SpawnBase()
    {
        if (!GridManager.instance.partDic.TryGetValue(GridBoard.WHEEL_KEY, out PartData wheelData))
        {
            Debug.LogWarning("[BuildManager] Key 10001 바퀴 데이터가 없습니다.");
            return;
        }


        int startX = Mathf.Max(0, (board.width / 2) - 1);
        for (int i = 0; i < board.startWheelCount; i++)
        {
            Vector2Int pos = new Vector2Int(startX + i, 0);

            if (!board.CanPlacePartByRules(wheelData, pos, 0))
                continue;

            GameObject partObj = new GameObject($"StartWheel_{i}");
            partObj.transform.SetParent(placedPartsRoot);

            PlacedPart placedPart = partObj.AddComponent<PlacedPart>();

            bool success = board.PlacePart(wheelData, pos, 0, placedPart);

            if (success)
            {
                TrySpawnRuntimePrefab(wheelData, placedPart);
                placedPart.BuildVisual(gridRenderer, placedPart.transform, Color.white);
            }
            else
            {
                Destroy(partObj);
            }
        }

        if (GridManager.instance.partDic.TryGetValue(GridBoard.CORE_KEY, out PartData coreData))
        {
            Vector2Int pos = new Vector2Int(startX, 1);

            if (!board.CanPlacePartByRules(coreData, pos, 0))
                return;

            GameObject partObj = new GameObject($"Core");
            partObj.transform.SetParent(placedPartsRoot);

            PlacedPart placedPart = partObj.AddComponent<PlacedPart>();

            bool success = board.PlacePart(coreData, pos, 0, placedPart);

            if (success)
            {
                TrySpawnRuntimePrefab(coreData, placedPart);
                placedPart.BuildVisual(gridRenderer, placedPart.transform, Color.white);
            }
            else
            {
                Destroy(partObj);
            }
            PlacePartInternal(coreData, pos, 0);
        }
        else
        {
            Debug.LogWarning("[BuildManager] Key 10002 코어 데이터가 없습니다.");
            return;
        }

    }
    private void ShowPlaceableCells()
    {
        ClearPlaceableHighlights();

        if (currentPartData == null || placeableHighlightRoot == null || highlightSprite == null)
            return;

        for (int x = 0; x < board.width; x++)
        {
            for (int y = 0; y < board.height; y++)
            {
                Vector2Int origin = new Vector2Int(x, y);

                if (!board.CanPlacePartByRules(currentPartData, origin, currentRotation))
                    continue;

                GameObject cellObj = new GameObject($"PlaceableOrigin_{x}_{y}");
                cellObj.transform.SetParent(placeableHighlightRoot, true);
                cellObj.transform.position = gridRenderer.GridToWorld(origin);
                cellObj.transform.localScale = new Vector3(gridRenderer.cellSize, gridRenderer.cellSize, 1f);

                SpriteRenderer sr = cellObj.AddComponent<SpriteRenderer>();
                sr.sprite = highlightSprite;
                sr.color = new Color(0f, 0.8f, 1f, 0.25f);
                sr.sortingOrder = -1;

                placeableHighlights.Add(cellObj);
            }
        }
    }

    // Grid 미리 표시 해제 함수
    private void ClearPlaceableHighlights()
    {
        for (int i = 0; i < placeableHighlights.Count; i++)
        {
            if (placeableHighlights[i] != null)
                Destroy(placeableHighlights[i]);
        }

        placeableHighlights.Clear();
    }
}
