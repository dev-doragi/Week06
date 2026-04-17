using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager instance;

    [SerializeField] private AttackPartDataSO _attackPartSO;
    [SerializeField] private DefensePartDataSO _defensePartSO;

    public Dictionary<int, AttackPartData> attackPartDic = new Dictionary<int, AttackPartData>();
    public Dictionary<int, DefensePartData> defensePartDic = new Dictionary<int, DefensePartData>();

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
        attackPartDic.Clear();
        defensePartDic.Clear();
        partDic.Clear();

        if (_attackPartSO != null && _attackPartSO.AttackPartDic != null)
        {
            attackPartDic = new Dictionary<int, AttackPartData>(_attackPartSO.AttackPartDic);

            foreach (var pair in attackPartDic)
            {
                if (partDic.ContainsKey(pair.Key))
                {
                    continue;
                }

                partDic.Add(pair.Key, pair.Value);
            }
        }

        if (_defensePartSO != null && _defensePartSO.DefensePartDic != null)
        {
            defensePartDic = new Dictionary<int, DefensePartData>(_defensePartSO.DefensePartDic);

            foreach (var pair in defensePartDic)
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
