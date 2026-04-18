using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyBlueprint", menuName = "Game Data/Enemy Blueprint")]
public class EnemyBlueprintSO : ScriptableObject
{
    public string blueprintName;
    public List<PartPlacementData> placements = new();
}

[Serializable]
public class PartPlacementData
{
    public int partKey;
    public Vector2Int origin;
    public int rotation;
}