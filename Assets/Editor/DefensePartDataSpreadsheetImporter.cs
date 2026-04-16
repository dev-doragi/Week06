using Sirenix.OdinInspector;
using UnityEditor;
using System.Collections.Generic;

public class DefensePartDataSpreadsheetImporter : BasePartSpreadsheetImporter<DefensePartData, DefensePartDataSO>
{
    [MenuItem("Tools/Defense Part Spreadsheet Importer")]
    private static void OpenWindow()
    {
        GetWindow<DefensePartDataSpreadsheetImporter>().Show();
    }

    protected override Dictionary<int, DefensePartData> GetOrCreateDictionary(DefensePartDataSO database)
    {
        if (database.DefensePartDic == null)
            database.DefensePartDic = new Dictionary<int, DefensePartData>();

        return database.DefensePartDic;
    }
}