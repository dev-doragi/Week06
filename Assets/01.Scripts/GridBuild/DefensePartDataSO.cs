using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DefensePartDatabase", menuName = "Game Data/Defense Part Database")]
public class DefensePartDataSO : SerializedScriptableObject
{
    [DictionaryDrawerSettings(KeyLabel = "이벤트 번호", ValueLabel = "파츠 상세 데이터")]
    public Dictionary<int, DefensePartData> DefensePartDic = new Dictionary<int, DefensePartData>();
}
