using UnityEngine;

public class GeneratorConnector : MonoBehaviour
{
    void OnEnable()
    {
        PlacementManager.Instance.AddGenerator();
    }

    void OnDestroy()
    {
        PlacementManager.Instance.SubtractGenerator();
    }
}
