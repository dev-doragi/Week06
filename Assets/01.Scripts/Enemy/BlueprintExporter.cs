using System.Collections.Generic;
using UnityEngine;

public class BlueprintExporter : MonoBehaviour
{
    [SerializeField] private GridBoard board;
    public string exportFileName = "NewEnemyBlueprint";

    public List<PartPlacementData> ExportCurrentBoard()
    {
        HashSet<PlacedPart> allParts = board.GetAllParts();
        List<PartPlacementData> result = new();

        foreach (var part in allParts)
        {
            if (part == null || part.data == null)
                continue;

            PartPlacementData placement = new PartPlacementData
            {
                partKey = part.data.Key,
                origin = part.origin,
                rotation = part.rotation
            };

            result.Add(placement);
        }

        return result;
    }
}
