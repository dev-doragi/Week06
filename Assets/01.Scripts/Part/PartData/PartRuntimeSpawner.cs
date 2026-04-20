using UnityEngine;

public class PartRuntimeSpawner : MonoBehaviour
{
    [SerializeField] private PartPrefabCatalog _partPrefabCatalog;

    public Sprite RatImage(int Key)
    {
        if (!_partPrefabCatalog.TryGetPrefab(Key, out GameObject prefab) || prefab == null)
        {
            return null;
        }
        return prefab.GetComponentInChildren<SpriteRenderer>().sprite;
    }
    public GameObject SpawnRuntime(
        PartData partData,
        PlacedPart placedPart,
        TeamType teamType,
        GridBoard board)
    {

        if (_partPrefabCatalog == null)
        {
            Debug.LogWarning($"{name}: PartPrefabCatalog가 연결되지 않았습니다.");
            return null;
        }

        if (partData == null)
        {
            Debug.LogError($"{name}: SpawnRuntime 실패 - partData가 null입니다.");
            return null;
        }

        if (placedPart == null)
        {
            Debug.LogError($"{name}: SpawnRuntime 실패 - placedPart가 null입니다.");
            return null;
        }

        if (!_partPrefabCatalog.TryGetPrefab(partData.Key, out GameObject prefab) || prefab == null)
        {
            Debug.LogWarning($"{name}: key={partData.Key}에 해당하는 프리팹이 없습니다.");
            return null;
        }
        GameObject runtimeInstance = Instantiate(
            prefab,
            placedPart.transform.position,
            Quaternion.identity,
            placedPart.transform);
        runtimeInstance.name = $"{prefab.name}_Runtime";
        runtimeInstance.transform.localPosition = Vector3.zero;
        runtimeInstance.transform.localRotation = Quaternion.identity;
        runtimeInstance.transform.localScale = Vector3.one;

        PartRuntimeBinder binder = runtimeInstance.GetComponent<PartRuntimeBinder>();
        if (binder == null)
        {

            binder = runtimeInstance.AddComponent<PartRuntimeBinder>();
        }

        PartRuntimeContext context = new PartRuntimeContext
        {
            PartData = partData,
            PlacedPart = placedPart,
            TeamType = teamType,
            Board = board
        };

        binder.Bind(context);
        return runtimeInstance;
    }
}
