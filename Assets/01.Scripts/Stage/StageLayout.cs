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
    }
}