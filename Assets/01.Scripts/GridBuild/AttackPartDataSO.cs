using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AttackPartDatabase", menuName = "Game Data/Attack Part Database")]
public class AttackPartDataSO : SerializedScriptableObject
{
    [DictionaryDrawerSettings(KeyLabel = "이벤트 번호", ValueLabel = "파츠 상세 데이터")]
    public Dictionary<int, AttackPartData> AttackPartDic = new Dictionary<int, AttackPartData>();
}
