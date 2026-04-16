using System.Collections.Generic;
using UnityEngine;

public class GridBoard : MonoBehaviour
{
    public int width = 10;
    public int height = 10;

    private PlacedPart[,] cells;

    private void Awake()
    {
        cells = new PlacedPart[width, height];
    }

    public bool IsInside(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }

    public bool IsOccupied(Vector2Int pos)
    {
        if (!IsInside(pos)) return false;
        return cells[pos.x, pos.y] != null;
    }

    public PlacedPart GetCell(Vector2Int pos)
    {
        if (!IsInside(pos)) return null;
        return cells[pos.x, pos.y];
    }

    public void SetCell(Vector2Int pos, PlacedPart part)
    {
        if (!IsInside(pos)) return;
        cells[pos.x, pos.y] = part;
    }

    public void ClearCell(Vector2Int pos)
    {
        if (!IsInside(pos)) return;
        cells[pos.x, pos.y] = null;
    }

    public Vector2Int RotateCell(Vector2Int cell, int rotation)
    {
        rotation = ((rotation % 4) + 4) % 4;

        return rotation switch
        {
            0 => cell,
            1 => new Vector2Int(-cell.y, cell.x),
            2 => new Vector2Int(-cell.x, -cell.y),
            3 => new Vector2Int(cell.y, -cell.x),
            _ => cell
        };
    }

    public List<Vector2Int> GetRotatedCells(PartData data, Vector2Int origin, int rotation)
    {
        List<Vector2Int> result = new();

        foreach (var cell in data.shape)
        {
            Vector2Int rotated = RotateCell(cell, rotation);
            result.Add(origin + rotated);
        }

        return result;
    }

    public bool CanPlacePart(PartData data, Vector2Int origin, int rotation)
    {
        foreach (var target in GetRotatedCells(data, origin, rotation))
        {
            if (!IsInside(target))
                return false;

            if (IsOccupied(target))
                return false;
        }

        return true;
    }

    public bool PlacePart(PartData data, Vector2Int origin, int rotation, PlacedPart placedPart)
    {
        if (!CanPlacePart(data, origin, rotation))
            return false;

        List<Vector2Int> targets = GetRotatedCells(data, origin, rotation);

        placedPart.Initialize(data, origin, rotation, targets);

        foreach (var cell in targets)
        {
            SetCell(cell, placedPart);
        }

        return true;
    }

    public void RemovePart(PlacedPart part)
    {
        if (part == null) return;

        foreach (var cell in part.occupiedCells)
        {
            if (GetCell(cell) == part)
                ClearCell(cell);
        }
    }
}
