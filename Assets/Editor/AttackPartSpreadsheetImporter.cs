using Sirenix.OdinInspector;
using UnityEditor;
using System.Collections.Generic;

public class AttackPartSpreadsheetImporter : BasePartSpreadsheetImporter<AttackPartData, AttackPartDataSO>
{
    public AttackPartSpreadsheetImporter()
    {
        sheetId = "1hKl1IEjN-yBBtVp9oKZpYj0QEwS-MdO5pVNEdupAWCo";
    }
    [MenuItem("Tools/Attack Part Spreadsheet Importer")]
    private static void OpenWindow()
    {
        GetWindow<AttackPartSpreadsheetImporter>().Show();
    }

    protected override Dictionary<int, AttackPartData> GetOrCreateDictionary(AttackPartDataSO database)
    {
        if (database.AttackPartDic == null)
            database.AttackPartDic = new Dictionary<int, AttackPartData>();

        return database.AttackPartDic;
    }
}