using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PartData", menuName = "Game Data/Part Database")]
public class PartDataSO : SerializedScriptableObject
{
    [DictionaryDrawerSettings(KeyLabel = "이벤트 번호", ValueLabel = "파츠 상세 데이터")]
    public Dictionary<int, PartData> partDic = new Dictionary<int, PartData>();
}
