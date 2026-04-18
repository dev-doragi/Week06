using UnityEditor;
using UnityEngine;
using System.IO;

public class ShopItemGenerator : EditorWindow
{
    private PartDataSO _partDatabase; // No longer serialized!
    
    private const string SHOP_DATA_PATH = "Assets/08.Data/ShopItems";
    private const string DB_PATH = "Assets/08.Data/ShopDatabase/ShopDatabase.asset";

   [MenuItem("Tools/Store Pipeline Manager")]
    public static void ShowWindow()
    {
        GetWindow<ShopItemGenerator>("Store Pipeline");
    }

    // This runs automatically when the window opens
    private void OnEnable()
    {
        // Search the project for any file of type PartDataSO
        string[] guids = AssetDatabase.FindAssets("t:PartDataSO");
        
        if (guids.Length > 0)
        {
            // Grab the first one it finds and load it
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            _partDatabase = AssetDatabase.LoadAssetAtPath<PartDataSO>(path);
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("1. Database Status", EditorStyles.boldLabel);
        
        if (_partDatabase != null)
        {
            // Show a green success message
            GUI.color = Color.green;
            GUILayout.Label($"✓ Found Database: {_partDatabase.name}");
            GUI.color = Color.white; // Reset color
        }
        else
        {
            // Show a red warning if it couldn't find it
            GUI.color = Color.red;
            GUILayout.Label("✗ Could not find a PartDataSO in the project!");
            GUI.color = Color.white;
            return; // Stop drawing the rest of the window
        }

        EditorGUILayout.Space(10);
        GUILayout.Label("2. Actions", EditorStyles.boldLabel);

        if (GUILayout.Button("Generate Shop Items from Dictionary", GUILayout.Height(30)))
        {
            GenerateFromDictionary();
        }

        if (GUILayout.Button("Cleanup Broken Items", GUILayout.Height(30)))
        {
            Cleanup();
        }

        if (GUILayout.Button("Sync to Shop Database", GUILayout.Height(30)))
        {
            SyncDatabase();
        }
    }

    private void GenerateFromDictionary()
    {
        if (!Directory.Exists(SHOP_DATA_PATH))
            Directory.CreateDirectory(SHOP_DATA_PATH);

        int createdCount = 0;

        foreach (var entry in _partDatabase.partDic)
        {
            int id = entry.Key;
            PartData data = entry.Value;

            string shopItemName = $"ShopItem_{id}_{data.PartName}.asset";
            string fullPath = Path.Combine(SHOP_DATA_PATH, shopItemName);

            if (!File.Exists(fullPath))
            {
                CreateNewShopItem(id, data, fullPath);
                createdCount++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Generation Complete! Created {createdCount} new items.");
    }

    private void CreateNewShopItem(int partKey, PartData data, string path)
    {
        ShopItemData newItem = ScriptableObject.CreateInstance<ShopItemData>();

        newItem.displayName = data.PartName;
        newItem.partKey = partKey;
        newItem.cost = data.Cost; 
        newItem.isLocked = true;

        switch (data.UnitRoleType)
        {
            case UnitRoleType.Attack:
                newItem.category = ShopItemCategory.AttackStore;
                break;
            case UnitRoleType.Defense:
                newItem.category = ShopItemCategory.DefnseStore;
                break;
            case UnitRoleType.Support:
                newItem.category = ShopItemCategory.SupportStore;
                break;
            default:
                newItem.category = ShopItemCategory.BuildStore;
                break;
        }

        AssetDatabase.CreateAsset(newItem, path);
    }

    private void Cleanup()
    {
        string[] guids = AssetDatabase.FindAssets("t:ShopItemData", new[] { SHOP_DATA_PATH });
        int deletedCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ShopItemData item = AssetDatabase.LoadAssetAtPath<ShopItemData>(path);

            if (item.partKey <= 0)
            {
                AssetDatabase.DeleteAsset(path);
                deletedCount++;
            }
        }

        if (deletedCount > 0)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Removed {deletedCount} ghost items.");
        }
    }

    private void SyncDatabase()
    {
        ShopDataBase db = AssetDatabase.LoadAssetAtPath<ShopDataBase>(DB_PATH);

        if (db == null) return;

        db.attackItems.Clear();
        db.defenseItems.Clear();
        db.buildItems.Clear();
        db.supportItems.Clear();

        string[] guids = AssetDatabase.FindAssets("t:ShopItemData");

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            ShopItemData item = AssetDatabase.LoadAssetAtPath<ShopItemData>(assetPath);

            if (item != null)
            {
                switch (item.category)
                {
                    case ShopItemCategory.AttackStore: db.attackItems.Add(item); break;
                    case ShopItemCategory.DefnseStore: db.defenseItems.Add(item); break;
                    case ShopItemCategory.BuildStore: db.buildItems.Add(item); break;
                    case ShopItemCategory.SupportStore: db.supportItems.Add(item); break;
                }
            }
        }

        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
        Debug.Log("Database Synced!");
    }
}