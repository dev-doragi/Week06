using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BlueprintExporter))]
public class BlueprintExporterEditor : Editor
{
    private const string blueprintFolderPath = "Assets/08.Data/EnemyBluePrint";
    private const string enemyPrefabFolderPath = "Assets/02.Prefabs/Enemy";
    private const string enemyTemplatePrefabPath = "Assets/02.Prefabs/Enemy/Template/EnemyTemplate.prefab";


    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);

        BlueprintExporter exporter = (BlueprintExporter)target;

        if (GUILayout.Button("Create Enemy Prefab Using Blueprint SO"))
        {
            CreateEnemyPrefab(exporter);
        }
    }

    private void CreateEnemyPrefab(BlueprintExporter exporter)
    {
        if (exporter == null)
        {
            Debug.LogWarning("Dont have BlueprintExporter");
            return;
        }

        List<PartPlacementData> placements = exporter.ExportCurrentBoard();

        if (placements == null || placements.Count == 0)
        {
            Debug.LogWarning("Dont have any saved parts");
            return;
        }

        placements.Sort((a, b) =>
        {
            int yCompare = a.origin.y.CompareTo(b.origin.y);
            if (yCompare != 0) return yCompare;

            int xCompare = a.origin.x.CompareTo(b.origin.x);
            if (xCompare != 0) return xCompare;

            return a.partKey.CompareTo(b.partKey);
        });

        string baseName = string.IsNullOrWhiteSpace(exporter.exportFileName)
            ? exporter.gameObject.name
            : exporter.exportFileName;

        EnsureFolderExists(blueprintFolderPath);
        EnsureFolderExists(enemyPrefabFolderPath);

        EnemyBlueprintSO blueprint = CreateBlueprintAsset(baseName, placements);
        if (blueprint == null)
            return;

        GameObject templatePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(enemyTemplatePrefabPath);
        if (templatePrefab == null)
        {
            Debug.LogError($"Dont find enemyTemplate");
            return;
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(templatePrefab);
        if (instance == null)
        {
            Debug.LogError("Cant create Prefab");
            return;
        }

        instance.name = baseName;

        EnemyBuilder enemyBuilder = instance.GetComponent<EnemyBuilder>();
        if (enemyBuilder == null)
        {
            enemyBuilder = instance.GetComponentInChildren<EnemyBuilder>();
        }

        if (enemyBuilder == null)
        {
            Debug.LogError("EnemyTemplate.prefab dont have EnemeyBuilder.cs");
            DestroyImmediate(instance);
            return;
        }

        enemyBuilder.blueprint = blueprint;
        EditorUtility.SetDirty(enemyBuilder);

        string prefabPath = AssetDatabase.GenerateUniqueAssetPath(
            Path.Combine(enemyPrefabFolderPath, $"{baseName}.prefab")
        );

        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
        DestroyImmediate(instance);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (savedPrefab != null)
        {
            Selection.activeObject = savedPrefab;
            Debug.Log($"Enemy Prefab 생성 완료: {prefabPath}");
        }
        else
        {
            Debug.LogError("Enemy Prefab 저장에 실패했습니다.");
        }
    }

    private EnemyBlueprintSO CreateBlueprintAsset(string baseName, List<PartPlacementData> placements)
    {
        EnemyBlueprintSO blueprint = ScriptableObject.CreateInstance<EnemyBlueprintSO>();
        blueprint.blueprintName = baseName;
        blueprint.placements = new List<PartPlacementData>(placements);

        string blueprintPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(blueprintFolderPath, $"{baseName}_Blueprint.asset"));

        AssetDatabase.CreateAsset(blueprint, blueprintPath);
        EditorUtility.SetDirty(blueprint);

        Debug.Log($"EnemyBlueprintSO 생성 완료");
        return blueprint;
    }

    private void EnsureFolderExists(string fullFolderPath)
    {
        if (AssetDatabase.IsValidFolder(fullFolderPath))
            return;

        string[] split = fullFolderPath.Split('/');
        if (split.Length < 2) return;

        string current = split[0];

        for (int i = 1; i < split.Length; i++)
        {
            string next = $"{current}/{split[i]}";

            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, split[i]);
            }

            current = next;
        }
    }
}
