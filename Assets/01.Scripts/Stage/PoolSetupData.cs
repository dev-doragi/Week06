using UnityEngine;

[System.Serializable]
public struct PoolSetupData
{
    public GameObject Prefab;
    [Tooltip("초기 생성 개수 (Prewarm)")]
    public int InitialSize;
    [Tooltip("최대 생성 허용 개수")]
    public int MaxSize;
}