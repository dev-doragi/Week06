using UnityEngine;

public class SpellMapConnector : MonoBehaviour
{
    void OnEnable()
    {
        PlacementManager.Instance.AddSpellGenerator();
    }

    void OnDestroy()
    {
        PlacementManager.Instance.SubtractSpellGenerator();
    }
}
