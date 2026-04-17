using System.Collections.Generic;
using UnityEngine;

public class GridBoard : MonoBehaviour
{
    public const int WHEEL_KEY = 10001;

    [Header("Grid Size")]
    public int width = 7;
    public int height = 21;

    [Header("Rules")]
    public int maxWheelCount = 7;
    public int startWheelCount = 3;
    public int maxBlockHeight = 20;
    public int supportPerWheel = 5;

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
    public bool IsWheelPart(PartData data)
    {
        return data != null && data.Key == WHEEL_KEY;
    }

    public bool IsWheelCell(Vector2Int pos)
    {
        if (!IsInside(pos)) return false;

        PlacedPart part = GetCell(pos);
        return part != null && part.data != null && part.data.Key == WHEEL_KEY;
    }

    public bool IsBlockCell(Vector2Int pos)
    {
        if (!IsInside(pos)) return false;

        PlacedPart part = GetCell(pos);
        return part != null && part.data != null && part.data.Key != WHEEL_KEY;
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

        foreach (var cell in data.Shape)
        {
            Vector2Int rotated = RotateCell(cell, rotation);
            result.Add(origin + rotated);
        }

        return result;
    }

    public int GetWheelCount()
    {
        HashSet<PlacedPart> uniqueParts = new();

        for (int x = 0; x < width; x++)
        {
            PlacedPart part = GetCell(new Vector2Int(x, 0));
            if (part != null && part.data != null && part.data.Key == WHEEL_KEY)
                uniqueParts.Add(part);
        }

        return uniqueParts.Count;
    }

    public int GetBlockCellCount()
    {
        int count = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 1; y <= maxBlockHeight && y < height; y++)
            {
                PlacedPart part = GetCell(new Vector2Int(x, y));
                if (part != null && part.data != null && part.data.Key != WHEEL_KEY)
                    count++;
            }
        }

        return count;
    }

    public bool HasAnyBlock()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 1; y <= maxBlockHeight && y < height; y++)
            {
                PlacedPart part = GetCell(new Vector2Int(x, y));
                if (part != null && part.data != null && part.data.Key != WHEEL_KEY)
                    return true;
            }
        }
        return false;
    }

    public bool HasAdjacentStructure(Vector2Int pos)
    {
        Vector2Int[] dirs =
        {
            Vector2Int.up,
            Vector2Int.left,
            Vector2Int.right
        };
        Vector2Int down = pos + Vector2Int.down;
        foreach (var dir in dirs)
        {
            Vector2Int next = pos + dir;
            if (!IsInside(next)) continue;

            if (GetCell(down) != null)
                return true;
        }
        return false;
    }

    public bool HasWheelDirectlyBelow(Vector2Int pos)
    {
        Vector2Int below = pos + Vector2Int.down;

        if (!IsInside(below)) return false;

        return IsWheelCell(below);
    }

    public bool CanSupportMoreBlocks(PartData newPartData)
    {
        if (IsWheelPart(newPartData))
            return true;

        int currentBlockCount = GetBlockCellCount();
        int wheelCount = GetWheelCount();
        int newBlockCells = newPartData.Shape.Count;
        int maxSupport = wheelCount * supportPerWheel;

        return currentBlockCount + newBlockCells <= maxSupport;
    }
    public bool CanPlacePartByRules(PartData data, Vector2Int origin, int rotation)
    {
        if (data == null) return false;

        List<Vector2Int> targets = GetRotatedCells(data, origin, rotation);

        foreach (var cell in targets)
        {
            if (!IsInside(cell))
                return false;

            if (IsOccupied(cell))
                return false;
        }

        if (IsWheelPart(data))
            return CanPlaceWheel(targets);

        return CanPlaceBlock(targets, data);
    }

    private bool CanPlaceWheel(List<Vector2Int> targets)
    {
        foreach (var cell in targets)
        {
            if (cell.y != 0)
                return false;
        }

        int currentWheelCount = GetWheelCount();
        if (currentWheelCount + targets.Count > maxWheelCount)
            return false;

        return true;
    }

    private bool CanPlaceBlock(List<Vector2Int> targets, PartData data)
    {
        foreach (var cell in targets)
        {
            if (cell.y < 1 || cell.y > maxBlockHeight)
                return false;
        }

        if (!CanSupportMoreBlocks(data))
            return false;

        bool hasAnyBlock = HasAnyBlock();

        if (!hasAnyBlock)
        {
            foreach (var cell in targets)
            {
                if (HasWheelDirectlyBelow(cell))
                    return true;
            }

            return false;
        }

        foreach (var cell in targets)
        {
            if (HasAdjacentStructure(cell))
                return true;
        }

        return false;
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
        if (!CanPlacePartByRules(data, origin, rotation))
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
