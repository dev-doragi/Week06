using System.Collections.Generic;
using UnityEngine;

public class GridBoard : MonoBehaviour
{
    public enum BoardOwnerType
    {
        Player,
        Enemy
    }

    public BoardOwnerType boardOwner;

    public const int WHEEL_KEY = 10001;
    public const int CORE_KEY = 10002;
    public const int ENEMY_WHEEL_KEY = 20001;
    public const int ENEMY_CORE_KEY = 20002;

    [Header("Grid Size")]
    public int width = 7;
    public int height = 21;

    [Header("Rules")]
    public int maxWheelCount = 7;
    public int startWheelCount = 3;
    public int maxBlockHeight = 20;
    public int supportPerWheel = 5;

    public BuildManager _buildManager;
    private PlacedPart[,] cells;

    // 보드의 셀 배열을 초기화하는 함수
    private void Awake()
    {
        cells = new PlacedPart[width, height];
    }
    
    #region BatchRule

    // 규칙을 포함해서 현재 파츠를 배치할 수 있는지 최종 판정하는 함수
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

    // 파츠의 모든 셀 좌표를 회전과 원점 기준으로 계산해서 반환하는 함수
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

    // 상대 좌표를 회전값에 맞게 회전시키는 함수
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

    // 전달된 좌표가 보드 범위 안에 있는지 확인하는 함수
    public bool IsInside(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }

    // 전달된 좌표에 이미 파츠가 배치되어 있는지 확인하는 함수
    public bool IsOccupied(Vector2Int pos)
    {
        if (!IsInside(pos)) return false;
        return cells[pos.x, pos.y] != null;
    }

    // 전달된 데이터가 바퀴 파츠인지 확인하는 함수
    public bool IsWheelPart(PartData data)
    {
        return data != null && (data.Key == WHEEL_KEY || data.Key == ENEMY_WHEEL_KEY);
    }

    // 바퀴 파츠가 현재 위치에 규칙상 배치 가능한지 확인하는 함수
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

    // 현재 보드에 배치된 바퀴 파츠 개수를 반환하는 함수
    public int GetWheelCount()
    {
        HashSet<PlacedPart> uniqueParts = new();

        for (int x = 0; x < width; x++)
        {
            PlacedPart part = GetCell(new Vector2Int(x, 0));
            if (part != null && part.data != null && (part.data.Key == WHEEL_KEY || part.data.Key == ENEMY_WHEEL_KEY))
                uniqueParts.Add(part);
        }

        return uniqueParts.Count;
    }

    // 일반 블럭 파츠가 현재 위치에 규칙상 배치 가능한지 확인하는 함수
    private bool CanPlaceBlock(List<Vector2Int> targets, PartData data)
    {
        foreach (var cell in targets)
        {
            if (cell.y < 1 || cell.y > maxBlockHeight)
                return false;
        }

        if (!CanSupportMoreBlocks(data))
            return false;

        foreach (var cell in targets)
        {
            if (HasAttackPartDirectlyBelow(cell))
                return false;
        }
        bool hasAnyBlock = HasAnyBlock();

        // 첫 블럭은 바퀴 바로 위에만
        if (!hasAnyBlock)
        {
            foreach (var cell in targets)
            {
                if (HasWheelDirectlyBelow(cell))
                {
                    if (IsAttackPart(data))
                    {
                        if (!HasPartDirectlyBelow(cell))
                            return false;

                        if (HasAdjacentAttackPart(cell))
                            return false;
                    }

                    return true;
                }
            }

            return false;
        }

        bool hasAnyAdjacentStructure = false;

        foreach (var cell in targets)
        {
            if (HasAdjacentStructure(cell))
            {
                hasAnyAdjacentStructure = true;
                break;
            }
        }

        if (!hasAnyAdjacentStructure)
            return false;

        // 공격형 추가 규칙
        if (IsAttackPart(data))
        {
            bool hasSupportBelow = false;

            foreach (var cell in targets)
            {
                if (HasPartDirectlyBelow(cell))
                {
                    hasSupportBelow = true;
                    break;
                }
            }

            // 공격형은 무조건 아래에 파츠가 있어야 함
            if (!hasSupportBelow)
                return false;

            // 공격형은 공격형끼리 붙을 수 없음
            foreach (var cell in targets)
            {
                if (HasAdjacentAttackPart(cell))
                    return false;
            }
        }

        return true;
    }
    //전달된 좌표 바로 아래 공격형 셀이 있는지 
    public bool HasAttackPartDirectlyBelow(Vector2Int pos)
    {
        Vector2Int below = pos + Vector2Int.down;

        if (!IsInside(below))
            return false;

        PlacedPart belowPart = GetCell(below);
        return belowPart != null &&
               belowPart.data != null &&
               IsAttackPart(belowPart.data);
    }

    // 전달된 좌표 바로 아래에 바퀴가 있는지 확인하는 함수
    public bool HasWheelDirectlyBelow(Vector2Int pos)
    {
        Vector2Int below = pos + Vector2Int.down;

        if (!IsInside(below)) return false;

        return IsWheelCell(below);
    }

    // 전달된 좌표에 있는 파츠가 바퀴인지 확인하는 함수
    public bool IsWheelCell(Vector2Int pos)
    {
        if (!IsInside(pos)) return false;

        PlacedPart part = GetCell(pos);
        return part != null && part.data != null && (part.data.Key == WHEEL_KEY || part.data.Key == ENEMY_WHEEL_KEY);
    }
    // 바퀴 수 대비 일반 블럭을 더 설치할 수 있는지 확인하는 함수
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
    
    // 보드에 일반 블럭이 하나라도 존재하는지 확인하는 함수
    public bool HasAnyBlock()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 1; y <= maxBlockHeight && y < height; y++)
            {
                PlacedPart part = GetCell(new Vector2Int(x, y));
                if (part != null && part.data != null && part.data.Key != WHEEL_KEY && part.data.Key != ENEMY_WHEEL_KEY)
                    return true;
            }
        }
        return false;
    }
    
    // 전달된 좌표 바로 아래에 어떤 파츠든 존재하는지 확인하는 함수
    public bool HasPartDirectlyBelow(Vector2Int pos)
    {
        Vector2Int below = pos + Vector2Int.down;

        if (!IsInside(below)) return false;

        return GetCell(below) != null;
    }
    
    // 전달된 좌표의 상하좌우에 공격형 파츠가 있는지 확인하는 함수
    public bool HasAdjacentAttackPart(Vector2Int pos)
    {
        Vector2Int[] dirs =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        foreach (var dir in dirs)
        {
            Vector2Int next = pos + dir;
            if (!IsInside(next)) continue;

            PlacedPart adjacentPart = GetCell(next);
            if (adjacentPart != null && adjacentPart.data != null && IsAttackPart(adjacentPart.data))
                return true;
        }

        return false;
    }

    // 전달된 좌표가 상하좌우 중 하나라도 기존 구조물과 연결되는지 확인하는 함수
    public bool HasAdjacentStructure(Vector2Int pos)
    {
        Vector2Int[] dirs =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        foreach (var dir in dirs)
        {
            Vector2Int next = pos + dir;
            if (!IsInside(next)) continue;

            if (GetCell(next) != null)
                return true;
        }

        return false;
    }
    
    // 전달된 데이터가 공격형 파츠인지 확인하는 함수
    public bool IsAttackPart(PartData data)
    {
        return data != null && data.UnitRoleType == UnitRoleType.Attack;
    }
    
    // 현재 보드에 배치된 일반 블럭 셀의 총 개수를 반환하는 함수
    public int GetBlockCellCount()
    {
        int count = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 1; y <= maxBlockHeight && y < height; y++)
            {
                PlacedPart part = GetCell(new Vector2Int(x, y));
                if (part != null && part.data != null && part.data.Key != WHEEL_KEY && part.data.Key != ENEMY_WHEEL_KEY)
                    count++;
            }
        }

        return count;
    }
    
    #endregion


    #region RemovePart

    // 코어와 연결되지 않은 파츠들만 찾아서 반환하는 함수
    public List<PlacedPart> GetDisconnectedParts()
    {
        HashSet<PlacedPart> allParts = GetAllParts();
        HashSet<PlacedPart> connectedParts = GetPartsConnectedToCore();

        List<PlacedPart> disconnected = new();

        foreach (var part in allParts)
        {
            if (part == null) continue;

            if (!connectedParts.Contains(part))
                disconnected.Add(part);
        }

        return disconnected;
    }

    // 현재 보드에 존재하는 모든 고유 파츠를 수집해서 반환하는 함수
    public HashSet<PlacedPart> GetAllParts()
    {
        HashSet<PlacedPart> result = new();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                PlacedPart part = GetCell(new Vector2Int(x, y));
                if (part != null)
                    result.Add(part);
            }
        }

        return result;
    }

    // 코어를 시작점으로 현재 연결되어 있는 모든 파츠를 찾는 함수
    public HashSet<PlacedPart> GetPartsConnectedToCore()
    {
        HashSet<PlacedPart> connectedParts = new();
        Queue<PlacedPart> queue = new();

        int targetCoreKey = boardOwner == BoardOwnerType.Player ? CORE_KEY : ENEMY_CORE_KEY;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                PlacedPart part = GetCell(new Vector2Int(x, y));
                if (part != null && part.data != null && part.data.Key == targetCoreKey)
                {
                    if (connectedParts.Add(part))
                        queue.Enqueue(part);
                }
            }
        }

        while (queue.Count > 0)
        {
            PlacedPart currentPart = queue.Dequeue();
            HashSet<PlacedPart> neighbors = GetAdjacentParts(currentPart);

            foreach (var neighbor in neighbors)
            {
                if (neighbor == null) continue;

                if (connectedParts.Add(neighbor))
                    queue.Enqueue(neighbor);
            }
        }

        return connectedParts;
    }

    // 특정 파츠와 상하좌우로 인접한 다른 파츠들을 반환하는 함수
    public HashSet<PlacedPart> GetAdjacentParts(PlacedPart part)
    {
        HashSet<PlacedPart> result = new();

        if (part == null) return result;

        Vector2Int[] dirs =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        foreach (var cell in part.occupiedCells)
        {
            foreach (var dir in dirs)
            {
                Vector2Int next = cell + dir;
                if (!IsInside(next)) continue;

                PlacedPart nextPart = GetCell(next);
                if (nextPart != null && nextPart != part)
                    result.Add(nextPart);
            }
        }

        return result;
    }
    #endregion


    #region External References
    // 파츠를 실제 보드 셀에 등록하는 함수
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

    // 전달된 좌표에 특정 파츠를 등록하는 함수
    public void SetCell(Vector2Int pos, PlacedPart part)
    {
        if (!IsInside(pos)) return;
        cells[pos.x, pos.y] = part;
    }

    // 전달된 파츠가 차지하고 있는 모든 셀을 보드에서 제거하는 함수
    public void RemovePart(PlacedPart part)
    {
        if (part == null) return;

        foreach (var cell in part.occupiedCells)
        {
            if (GetCell(cell) == part)
                ClearCell(cell);
        }
    }

    // 특정 파츠 위에 다른 파츠가 하나라도 있는지 확인하는 함수
    public bool HasPartAbove(PlacedPart part)
    {
        if (part == null) return false;

        foreach (var cell in part.occupiedCells)
        {
            Vector2Int up = cell + Vector2Int.up;

            if (!IsInside(up))
                continue;

            PlacedPart upperPart = GetCell(up);

            if (upperPart != null && upperPart != part)
                return true;
        }

        return false;
    }

    // 전달된 좌표의 파츠 정보를 비우는 함수
    public void ClearCell(Vector2Int pos)
    {
        if (!IsInside(pos)) return;
        cells[pos.x, pos.y] = null;
    }
    #endregion


    // 전달된 좌표에 있는 파츠를 반환하는 함수
    public PlacedPart GetCell(Vector2Int pos)
    {
        if (!IsInside(pos)) return null;
        return cells[pos.x, pos.y];
    }

}
