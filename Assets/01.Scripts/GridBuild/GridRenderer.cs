using UnityEngine;

public class GridRenderer : MonoBehaviour
{
    public GridBoard board;
    public float cellSize = 1f;
    public Vector2 origin;

    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(
            origin.x + gridPos.x * cellSize + cellSize * 0.5f,
            origin.y + gridPos.y * cellSize + cellSize * 0.5f,
            0f
        );
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt((worldPos.x - origin.x) / cellSize);
        int y = Mathf.FloorToInt((worldPos.y - origin.y) / cellSize);
        return new Vector2Int(x, y);
    }

    private void OnDrawGizmos()
    {
        if (board == null) return;

        Gizmos.color = Color.gray;

        for (int x = 0; x <= board.width; x++)
        {
            Vector3 start = new Vector3(origin.x + x * cellSize, origin.y, 0f);
            Vector3 end = new Vector3(origin.x + x * cellSize, origin.y + board.height * cellSize, 0f);
            Gizmos.DrawLine(start, end);
        }

        for (int y = 0; y <= board.height; y++)
        {
            Vector3 start = new Vector3(origin.x, origin.y + y * cellSize, 0f);
            Vector3 end = new Vector3(origin.x + board.width * cellSize, origin.y + y * cellSize, 0f);
            Gizmos.DrawLine(start, end);
        }
    }
}
