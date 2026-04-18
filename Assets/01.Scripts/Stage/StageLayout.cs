using UnityEngine;

public class StageLayout : MonoBehaviour
{
    [Header("Player (Ally) Point")]
    [Tooltip("아군 본진 위치")]
    [SerializeField] private Transform _allyBasePoint;

    [Header("Enemy Siege Point")]
    [Tooltip("적 공성병기 위치")]
    [SerializeField] private Transform _enemySiegePoint;

    public Transform AllyBasePoint => _allyBasePoint;
    public Transform EnemySiegePoint => _enemySiegePoint;

    private void Awake()
    {
        if (_enemySiegePoint == null)
            Debug.LogError("[StageLayout] 적 공성병기 소환 위치(_enemySiegePoint)가 할당되지 않았습니다.");

        if (_allyBasePoint == null)
            Debug.LogError("[StageLayout] 아군 본진 위치(_allyBasePoint)가 할당되지 않았습니다.");
    }

    /// <summary>
    /// StageManager가 프리팹을 씬에 생성한 직후에 호출하여 씬의 정보를 주입합니다.
    /// </summary>
    public void InitLayout(Vector3 gridOriginPos)
    {
        if (_allyBasePoint != null)
        {
            _allyBasePoint.position = gridOriginPos;
        }
    }
}