using UnityEngine;
using UnityEditor;
using System.IO;

public class ShopItemGenerator : EditorWindow
{
    // private string ratDataPath = "Assets/08.Data/RatDatas"; // Where your RatData lives
    // private string xshopDataPath = "Assets/08.Data/ShopItems"; // Where items will be saved

    [MenuItem("Tools/Generate Shop Items")]
    public static void Generate()
    {
        // 1. Find all RatData files in the folder
        string[] guids = AssetDatabase.FindAssets("t:RatData", new[] { "Assets/08.Data/RatDatas" });
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            RatData rat = AssetDatabase.LoadAssetAtPath<RatData>(path);

            // 2. Define the name for the new ShopItemData
            string shopItemName = "ShopItem_" + rat.name + ".asset";
            string fullPath = Path.Combine("Assets/08.Data/ShopItems", shopItemName);

            // 3. Check if it already exists so we don't overwrite teammate's work
            if (!File.Exists(fullPath))
            {
                CreateNewShopItem(rat, fullPath);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Shop Item Generation Complete!");
    }



    [MenuItem("Tools/Cleanup Broken Shop Items")]
    public static void Cleanup()
    {
        const string SHOP_DATA_PATH = "Assets/Data/ShopItems";
        
        // 1. Find all ShopItemData files
        string[] guids = AssetDatabase.FindAssets("t:ShopItemData", new[] { SHOP_DATA_PATH });
        int deletedCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ShopItemData item = AssetDatabase.LoadAssetAtPath<ShopItemData>(path);

            // 2. Check if the link to RatData is missing/null
            // Or check if the file the link points to has been deleted from the project
            if (item.ratData == null)
            {
                Debug.Log($"Deleting broken ShopItem: {item.name} (Missing RatData link)");
                AssetDatabase.DeleteAsset(path);
                deletedCount++;
            }
        }

        if (deletedCount > 0)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Cleanup complete. Removed {deletedCount} ghost items.");
        }
        else
        {
            Debug.Log("No broken ShopItems found. Your project is clean!");
        }
    }



    [MenuItem("Tools/Sync to Data Base")]
    public static void SyncDatabase()
    {
        string dbPath = "Assets/08.Data/ShopDatabase/ShopDatabase.asset";
        ShopDataBase db = AssetDatabase.LoadAssetAtPath<ShopDataBase>(dbPath);

        if (db == null)
        {
            Debug.LogError("Database not found at " + dbPath);
            return;
        }

        // 2. Clear old references to start fresh
        db.attackItems.Clear();
        db.buildItems.Clear();
        db.supportItems.Clear();

        // 3. Find every ShopItemData asset in the project
        // "t:ShopItemData" tells Unity to look for that specific script type
        string[] guids = AssetDatabase.FindAssets("t:ShopItemData");

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            ShopItemData item = AssetDatabase.LoadAssetAtPath<ShopItemData>(assetPath);

            if (item != null)
            {
                // 4. Sort the item into the correct list based on its category
                switch (item.category)
                {
                    case ShopItemCategory.AttackStore:
                        db.attackItems.Add(item);
                        break;
                    case ShopItemCategory.BuildStore:
                        db.buildItems.Add(item);
                        break;
                    case ShopItemCategory.SupportStore:
                        db.supportItems.Add(item);
                        break;
                }
            }
        }

        // 5. IMPORTANT: Tell Unity the file has changed so it saves the data!
        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"Database Synced! Total items: {db.attackItems.Count + db.buildItems.Count + db.supportItems.Count}");
    }



    private static void CreateNewShopItem(RatData rat, string path)
    {
        ShopItemData newItem = ScriptableObject.CreateInstance<ShopItemData>();
        
        // Auto-fill the basics
        newItem.displayName = rat.name; 
        newItem.ratData = rat; // The crucial link!
        
        // Set defaults
        newItem.isLocked = true;
        newItem.cost = 10; 

        AssetDatabase.CreateAsset(newItem, path);
    }
    
}