using System.Collections.Generic;
using UnityEngine;

public class BuildManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridBoard board;
    [SerializeField] private GridRenderer gridRenderer;
    [SerializeField] private Transform placedPartsRoot;
    [SerializeField] private Transform ghostRoot;

    [Header("Current Selection")]
    [SerializeField] private int selectedPartKey;

    private PartData currentPartData;
    private int currentRotation;

    private PlacedPart ghostPart;

    private void Start()
    {
        SelectPart(selectedPartKey);
        CreateGhost();
    }

    private void Update()
    {
        if (currentPartData == null) return;

        HandleRotation();
        UpdateGhost();

        if (Input.GetMouseButtonDown(0))
        {
            TryPlaceCurrentPart();
        }

        if (Input.GetMouseButtonDown(1))
        {
            TryRemovePart();
        }
    }

    public void SelectPart(int key)
    {
        if (!GridManager.instance.partDic.TryGetValue(key, out currentPartData))
        {
            Debug.LogWarning($"[BuildManager] key {key} 에 해당하는 파츠가 없습니다.");
            currentPartData = null;
            return;
        }

        currentRotation = 0;

        if (ghostPart != null)
            Destroy(ghostPart.gameObject);

        CreateGhost();
    }

    private void CreateGhost()
    {
        if (currentPartData == null || ghostRoot == null) return;

        GameObject ghostObj = new GameObject("GhostPart");
        ghostObj.transform.SetParent(ghostRoot);

        ghostPart = ghostObj.AddComponent<PlacedPart>();
    }

    private void HandleRotation()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            currentRotation = (currentRotation + 1) % 4;
        }
    }

    private Vector2Int GetMouseGridPosition()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        return gridRenderer.WorldToGrid(mouseWorld);
    }

    private void UpdateGhost()
    {
        Vector2Int gridPos = GetMouseGridPosition();
        List<Vector2Int> targetCells = board.GetRotatedCells(currentPartData, gridPos, currentRotation);

        ghostPart.Initialize(currentPartData, gridPos, currentRotation, targetCells);
        ghostPart.BuildVisual(gridRenderer, ghostPart.transform, new Color(1f, 1f, 1f, 0.45f));

        bool canPlace = board.CanPlacePart(currentPartData, gridPos, currentRotation);
        ghostPart.SetColor(canPlace ? new Color(0f, 1f, 0f, 0.45f) : new Color(1f, 0f, 0f, 0.45f));
    }

    private void TryPlaceCurrentPart()
    {
        Vector2Int gridPos = GetMouseGridPosition();

        if (!board.CanPlacePart(currentPartData, gridPos, currentRotation))
            return;

        GameObject partObj = new GameObject($"Placed_{currentPartData.partName}");
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
}
