using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

[DefaultExecutionOrder(-97)]
public class PoolManager : Singleton<PoolManager>
{
    // 프리팹 이름(또는 ID)을 키로 사용하여 ObjectPool을 관리하는 딕셔너리
    private readonly Dictionary<string, IObjectPool<GameObject>> _pools = new();

    [Header("Pool Setup")]
    [SerializeField] private Transform _poolRoot; // 씬 하이어라키를 깔끔하게 관리하기 위한 부모 객체

    protected override void Init()
    {
        if (_poolRoot == null)
        {
            GameObject rootObj = new GameObject("PoolRoot");
            DontDestroyOnLoad(rootObj);
            _poolRoot = rootObj.transform;
        }
    }

    /// <summary>
    /// 스테이지 셋업 시 호출하여 필요한 오브젝트를 미리 풀에 생성합니다. (Prewarm)
    /// </summary>
    public void CreatePool(GameObject prefab, int initialSize, int maxSize = 100)
    {
        if (prefab == null)
        {
            Debug.LogError("[PoolManager] 생성할 프리팹이 null입니다.");
            return;
        }

        string key = prefab.name;
        if (_pools.ContainsKey(key)) return; // 이미 존재하는 풀이면 스킵

        IObjectPool<GameObject> pool = new ObjectPool<GameObject>(
            createFunc: () =>
            {
                GameObject obj = Instantiate(prefab, _poolRoot);
                obj.name = prefab.name;
                return obj;
            },
            actionOnGet: obj => obj.SetActive(true),
            actionOnRelease: obj => obj.SetActive(false),
            actionOnDestroy: obj => Destroy(obj),
            collectionCheck: false,
            defaultCapacity: initialSize,
            maxSize: maxSize
        );

        _pools.Add(key, pool);

        if (initialSize > 0)
        {
            GameObject[] prewarmObjects = new GameObject[initialSize];
            for (int i = 0; i < initialSize; i++)
            {
                prewarmObjects[i] = pool.Get();
            }
            for (int i = 0; i < initialSize; i++)
            {
                pool.Release(prewarmObjects[i]);
            }
        }
    }

    public GameObject Spawn(string prefabName, Vector3 position, Quaternion rotation)
    {
        if (!_pools.TryGetValue(prefabName, out var pool))
        {
            Debug.LogError($"[PoolManager] '{prefabName}'에 해당하는 풀이 존재하지 않습니다. 먼저 CreatePool을 호출하세요.");
            return null;
        }

        GameObject obj = pool.Get();
        obj.transform.SetPositionAndRotation(position, rotation);
        return obj;
    }

    public void Despawn(GameObject obj)
    {
        string key = obj.name;
        if (!_pools.TryGetValue(key, out var pool))
        {
            Debug.LogError($"[PoolManager] '{key}' 오브젝트를 반환할 풀이 없습니다. 파괴 처리합니다.");
            Destroy(obj);
            return;
        }

        pool.Release(obj);
        obj.transform.SetParent(_poolRoot);
    }

    /// <summary>
    /// 스테이지가 끝났을 때 현재 풀링된 객체들을 모두 파괴하고 메모리를 확보합니다.
    /// </summary>
    public void ClearAllPools()
    {
        foreach (var pool in _pools.Values)
        {
            pool.Clear(); // Unity의 ObjectPool.Clear()는 내부 객체들을 Destroy 합니다.
        }
        _pools.Clear();
    }
}