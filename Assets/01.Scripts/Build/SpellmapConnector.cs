using UnityEngine;

public class SpellmapConnector : MonoBehaviour
{
    [SerializeField] private int addCount;

    void OnEnable()
    {
        PlacementManager.Instance.AddSpellGenerator(addCount);
    }

    void OnDestroy()
    {
        PlacementManager.Instance.SubtractSpellGenerator(addCount);
    }
}
