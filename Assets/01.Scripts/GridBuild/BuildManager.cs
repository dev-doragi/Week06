using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance;

    [SerializeField] private GridBoard board;
    [SerializeField] private GridRenderer gridRenderer;
    [SerializeField] private Transform placedPartsRoot;
    [SerializeField] private Transform ghostRoot;

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
        if (VehicleCache.HasSavedData) return; // Jaein 추가

        SpawnStartWheels();
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
        ghostPart.BuildVisual(gridRenderer, ghostPart.transform, new Color(1f, 1f, 1f, 0.45f));

        bool canPlace = board.CanPlacePartByRules(currentPartData, gridPos, currentRotation);
        ghostPart.SetColor(canPlace ? new Color(0f, 1f, 0f, 0.45f) : new Color(1f, 0f, 0f, 0.45f));
    }

    public void TryPlaceCurrentPart()
    {
        if (currentPartData == null)
            return;

        Vector2Int gridPos = GetMouseGridPosition();

        if (!board.CanPlacePartByRules(currentPartData, gridPos, currentRotation))
        {
            return;
        }

        GameObject partObj = new GameObject($"Placed_{currentPartData.PartName}");
        partObj.transform.SetParent(placedPartsRoot);

        PlacedPart placedPart = partObj.AddComponent<PlacedPart>();

        bool success = board.PlacePart(currentPartData, gridPos, currentRotation, placedPart);

        if (!success)
        {
            Destroy(partObj);
            return;
        }

        placedPart.BuildVisual(gridRenderer, placedPart.transform, Color.white);
    }

    private void TryRemovePart()
    {
        Vector2Int gridPos = GetMouseGridPosition();

        if (!board.IsInside(gridPos)) return;

        PlacedPart targetPart = board.GetCell(gridPos);
        if (targetPart == null) return;

        board.RemovePart(targetPart);
        Destroy(targetPart.gameObject);
    }

    public void BrokenPart(PlacedPart targetPart)
    {
        board.RemovePart(targetPart);
        Destroy(targetPart.gameObject);
    }

    private void ClearSelection()
    {
        haveMouse = false;
        currentPartData = null;
        currentRotation = 0;

        if (ghostPart != null)
        {
            Destroy(ghostPart.gameObject);
            ghostPart = null;
        }
    }

    private void SpawnStartWheels()
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
                placedPart.BuildVisual(gridRenderer, placedPart.transform, Color.white);
            }
            else
            {
                Destroy(partObj);
            }
        }
    }
}
