using UnityEngine;

[CreateAssetMenu(
    fileName = "RatData_",
    menuName = "08.Data/Rat/Rat Data")]
public class RatData : ScriptableObject
{
    [SerializeField] private string _id;
    [SerializeField] private string _displayName;
    [SerializeField] private RatType _ratType;
    [SerializeField] private RatCommonStatData _commonStat;
    [SerializeField] private RatAttackStatData _attackStat;
    [SerializeField] private RatDefenceStatData _defenceStat;

    public string Id => _id;
    public string DisplayName => _displayName;
    public RatType RatType => _ratType;
    public RatCommonStatData CommonStat => _commonStat;
    public RatAttackStatData AttackStat => _attackStat;
    public RatDefenceStatData DefenceStat => _defenceStat;

    public bool HasAttackStat => _ratType == RatType.Attack;
    public bool HasDefenceStat => _ratType == RatType.Defence;

    private void OnValidate()
    {
        ValidateCommonStat();
        ValidateRoleStat();
    }

    private void ValidateCommonStat()
    {
        if(_commonStat == null)
        {
            Debug.LogError($"{name}: CommonStat이 비어 있습니다.");
        }
    }

    private void ValidateRoleStat()
    {
        if (_ratType == RatType.Attack && _attackStat == null)
        {
            Debug.LogError($"{name}: 공격형 쥐인데 AttackStat이 비어 있습니다.");
        }

        if (_ratType != RatType.Attack && _attackStat != null)
        {
            Debug.LogError($"{name}: 공격형이 아닌데 AttackStat이 설정되어 있습니다.");
        }

        if (_ratType == RatType.Defence && _defenceStat == null)
        {
            Debug.LogError($"{name}: 방어형 쥐인데 DefenceStat이 비어 있습니다.");
        }

        if (_ratType != RatType.Defence && _defenceStat != null)
        {
            Debug.LogError($"{name}: 방어형이 아닌데 DefenceStat이 설정되어 있습니다.");
        }
    }
}
