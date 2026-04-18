using System.Collections.Generic;
using UnityEngine;

public static class GridRangeUtility
{
    public static bool IsWithinCellRadius(
        IReadOnlyList<Vector2Int> sourceCells,
        IReadOnlyList<Vector2Int> targetCells,
        int radius)
    {
        if (sourceCells == null)
        {
            Debug.LogError("IsWithinCellRadius 실패 - sourceCells가 Null입니다.");
            return false;
        }

        if (targetCells == null)
        {
            Debug.LogError("IsWithinCellRadius 실패 - targetCells가 Null입니다.");
            return false;
        }

        if (radius < 0)
        {
            Debug.LogError($"IsWithinCellRadius 실패 - radius는 0 이상이어야 합니다. 입력값: {radius}");
            return false;
        }

        if (sourceCells.Count == 0 || targetCells.Count == 0)
        {
            return false;
        }

        for (int i = 0; i < sourceCells.Count; i++)
        {
            Vector2Int sourceCell = sourceCells[i];

            for (int j = 0; j < targetCells.Count; j++)
            {
                Vector2Int targetCell = targetCells[j];

                int dx = Mathf.Abs(sourceCell.x - targetCell.x);
                int dy = Mathf.Abs(sourceCell.y - targetCell.y);

                int chebyshevDistance = Mathf.Max(dx, dy);
                if (chebyshevDistance <= radius)
                {
                    return true;
                }
            }
        }

        return false;
    }
}