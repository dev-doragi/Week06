using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PlacedPart : MonoBehaviour
{
    public PartData data;
    public Vector2Int origin;
    public int rotation;
    public List<Vector2Int> occupiedCells = new();

    private List<SpriteRenderer> cellRenderers = new();

    public void Initialize(PartData data, Vector2Int origin, int rotation, List<Vector2Int> occupiedCells)
    {
        this.data = data;
        this.origin = origin;
        this.rotation = rotation;
        this.occupiedCells = new List<Vector2Int>(occupiedCells);
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
            sr.sprite = data.Icon;
            sr.color = color;

            float scale = gridRenderer.cellSize;
            cellObj.transform.localScale = new Vector3(scale, scale, 1f);

            cellRenderers.Add(sr);
        }
    }

    public void SetColor(Color color)
    {
        foreach (var sr in cellRenderers)
        {
            if (sr != null)
                sr.color = color;
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
}
