using UnityEngine;

public class GridRenderer : MonoBehaviour
{
    public GridBoard board;
    public float cellSize = 1f; 
    public Transform originPos;

    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(
            originPos.position.x + gridPos.x * cellSize + cellSize * 0.5f,
            originPos.position.y + gridPos.y * cellSize + cellSize * 0.5f,
            0f
        );
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt((worldPos.x - originPos.position.x) / cellSize);
        int y = Mathf.FloorToInt((worldPos.y - originPos.position.y) / cellSize);
        return new Vector2Int(x, y);
    }

    private void OnDrawGizmos()
    {
        if (board == null) return;

        Gizmos.color = Color.gray;

        for (int x = 0; x <= board.width; x++)
        {
            Vector3 start = new Vector3(originPos.position.x + x * cellSize, originPos.position.y, 0f);
            Vector3 end = new Vector3(originPos.position.x + x * cellSize, originPos.position.y + board.height * cellSize, 0f);
            Gizmos.DrawLine(start, end);
        }

        for (int y = 0; y <= board.height; y++)
        {
            Vector3 start = new Vector3(originPos.position.x, originPos.position.y + y * cellSize, 0f);
            Vector3 end = new Vector3(originPos.position.x + board.width * cellSize, originPos.position.y + y * cellSize, 0f);
            Gizmos.DrawLine(start, end);
        }
    }
}
