using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager instance;

    [SerializeField] private PartDataSO _partSO;

    public Dictionary<int, PartData> partDic = new Dictionary<int, PartData>();

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        InitializeData();
    }

    private void InitializeData()
    {
        partDic.Clear();

        if (_partSO != null && _partSO.partDic != null)
        {
            partDic = new Dictionary<int, PartData>(_partSO.partDic);

            foreach (var pair in partDic)
            {
                if (partDic.ContainsKey(pair.Key))
                {
                    continue;
                }

                partDic.Add(pair.Key, pair.Value);
            }
        }

    }
}
