using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PartSupportStatData
{
    [SerializeField] private int _supportRangeRadius;
    [SerializeField] private List<PartSupportEffectData> _effects;

    public int SupportRangeRadius => _supportRangeRadius;
    public IReadOnlyList<PartSupportEffectData> Effects => _effects;

    public PartSupportStatData(int supportRangeRadius, List<PartSupportEffectData> effects)
    {
        // 지원 반경 저장
        _supportRangeRadius = supportRangeRadius;
        // 지원 효과 목록 저장
        _effects = effects;
    }
}