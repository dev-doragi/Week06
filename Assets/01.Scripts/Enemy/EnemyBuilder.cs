using UnityEngine;

public class EnemyBuilder : MonoBehaviour
{

    [SerializeField] private GridBoard board;
    [SerializeField] private GridRenderer gridRenderer;
    [SerializeField] private Transform placedPartsRoot;
    public EnemyBlueprintSO blueprint;

    private void Start()
    {
        BuildFromBlueprint();
    }

    public void BuildFromBlueprint()
    {
        if (blueprint == null) return;

        foreach (var placement in blueprint.placements)
        {
            if (!GridManager.instance.partDic.TryGetValue(placement.partKey, out PartData partData))
            {
                Debug.LogWarning($"Dont find part key");
                continue;
            }

            GameObject partObj = new GameObject($"EnemyPart_{partData.PartName}");
            partObj.transform.SetParent(placedPartsRoot);

            PlacedPart placedPart = partObj.AddComponent<PlacedPart>();

            bool success = board.PlacePart(partData, placement.origin, placement.rotation, placedPart);

            if (!success)
            {
                Debug.LogWarning($"배치 실패: {partData.PartName}, {placement.origin}");
                Destroy(partObj);
                continue;
            }

            placedPart.BuildVisual(gridRenderer, placedPart.transform, Color.white);
        }
    }
}
