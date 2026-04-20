using UnityEngine;

public class GeneratorConnector : MonoBehaviour
{
    [SerializeField] private int addCount;

    void OnEnable()
    {
        PlacementManager.Instance.AddGenerator(addCount);
    }

    void OnDestroy()
    {
        PlacementManager.Instance.SubtractGenerator(addCount);
    }
}
