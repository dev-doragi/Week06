using UnityEngine;

[DefaultExecutionOrder(-100)]
public class PartDatabaseProvider : MonoBehaviour
{
    [SerializeField] private PartDataSO _partDataSO;

    private static PartDatabaseProvider _instance;

    public static PartDatabaseProvider Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("PartDatabaseProvider Instance is Null");
            }

            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;

        if (_partDataSO == null)
        {
            Debug.LogError($"{name}: PartDataSO가 할당되지 않았습니다.");
            return;
        }

        if (_partDataSO.partDic == null)
        {
            Debug.LogError($"{name}: PartDataSO.partDic가 Null입니다.");
        }
    }

    public bool TryGetPartData(int partKey, out PartData partData)
    {
        partData = null;

        if (_partDataSO == null)
        {
            Debug.LogError($"{name}: TryGetPartData 실패 - PartDataSO가 Null입니다.");
            return false;
        }

        if (_partDataSO.partDic == null)
        {
            Debug.LogError($"{name}: TryGetPartData 실패 - partDic이 Null입니다.");
            return false;
        }

        if (!_partDataSO.partDic.TryGetValue(partKey, out partData))
        {
            Debug.LogError($"{name}: TryGetPartData 실패 - key {partKey}에 해당하는 PartData가 없습니다.");
            return false;
        }

        return true;
    }

    public PartData GetPartData(int partKey)
    {
        if (!TryGetPartData(partKey, out var partData))
        {
            Debug.LogError($"{name}: GetPartData 실패 - key {partKey} 조회 실패");
            return null;
        }

        return partData;
    }
}