using UnityEngine;

public class GridRenderer : MonoBehaviour
{
    public GridBoard board;
    public float cellSize = 0.5f;
    public Transform originPos;

    public Vector3 GridToLocal(Vector2Int gridPos)
    {
        Vector3 originLocal = Vector3.zero;

        if (originPos != null)
            originLocal = transform.InverseTransformPoint(originPos.position);

        return originLocal + new Vector3(gridPos.x * cellSize, gridPos.y * cellSize, 0f);
    }

    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        return transform.TransformPoint(GridToLocal(gridPos));
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        Vector3 local = transform.InverseTransformPoint(worldPos);

        Vector3 originLocal = Vector3.zero;
        if (originPos != null)
            originLocal = transform.InverseTransformPoint(originPos.position);

        Vector3 relative = local - originLocal;

        int x = Mathf.RoundToInt(relative.x / cellSize);
        int y = Mathf.RoundToInt(relative.y / cellSize);

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
