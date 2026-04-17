using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class BasePartSpreadsheetImporter : OdinEditorWindow
{
    [TitleGroup("GoogleSheetSettings", "1. 구글 시트 설정")]
    [LabelText("문서 ID")]
    public string sheetId = "1hKl1IEjN-yBBtVp9oKZpYj0QEwS-MdO5pVNEdupAWCo";

    [LabelText("대상 시트 GID")]
    public string selectedGid = "0";

    [TitleGroup("SaveSettings", "2. 저장 설정")]
    [LabelText("저장될 SO")]
    public PartDataSO partDataSO;

    [FolderPath]
    [LabelText("저장 경로")]
    public string savePath = "Assets/08.Data";

    [LabelText("저장 파일 이름")]
    public string fileName = "PartDatabase";

    [TitleGroup("ImageSettings", "3. 이미지 자동 설정")]
    [FolderPath]
    [LabelText("스프라이트 폴더 경로")]
    public string spriteFolderPath = "Assets/04.Art/PartImage";

    [LabelText("이미지 필드명")]
    [Tooltip("Data 안에서 Sprite를 담는 필드 이름 (예: Icon)")]
    public string iconFieldName = "Icon";

    [LabelText("이름 참조 필드명")]
    [Tooltip("Sprite 파일명을 담고 있는 string 필드 이름 (예: SpriteName)")]
    public string spriteNameFieldName = "SpriteName";

    [MenuItem("Tools/Defense Part Spreadsheet Importer")]
    private static void OpenWindow()
    {
        GetWindow<BasePartSpreadsheetImporter>().Show();
    }

    [Button("데이터 가져오기", ButtonSizes.Large), GUIColor(0.2f, 0.8f, 0.2f)]
    public void Import()
    {
        if (string.IsNullOrWhiteSpace(sheetId))
        {
            Debug.LogError("[오류] sheetId가 비어 있습니다.");
            return;
        }

        string url = $"https://docs.google.com/spreadsheets/d/{sheetId}/export?format=csv&gid={selectedGid}";

        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SendWebRequest();

        EditorUtility.DisplayProgressBar("다운로드 중", "구글 시트에서 데이터를 읽어오는 중...", 0.5f);
        while (!www.isDone) { }
        EditorUtility.ClearProgressBar();

        if (www.result == UnityWebRequest.Result.Success)
        {
            ParseData(www.downloadHandler.text);
        }
        else
        {
            Debug.LogError("[오류] 다운로드 실패: " + www.error);
        }
    }

    private void ParseData(string csvData)
    {
        if (!System.IO.Directory.Exists(savePath))
            System.IO.Directory.CreateDirectory(savePath);

        string fullPath = $"{savePath}/{fileName}.asset";

        PartDataSO database = partDataSO;
        if (database == null)
        {
            database = AssetDatabase.LoadAssetAtPath<PartDataSO>(fullPath);
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<PartDataSO>();
                AssetDatabase.CreateAsset(database, fullPath);
            }
        }

        if (database.partDic == null)
            database.partDic = new Dictionary<int, PartData>();

        database.partDic.Clear();

        string[] lines = csvData.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2)
        {
            Debug.LogWarning("[경고] 데이터가 없습니다.");
            return;
        }

        string[] headers = SplitCsvLine(lines[0]).Select(h => h.Trim().Trim('"')).ToArray();

        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        Type entityType = typeof(PartData);

        FieldInfo iconField = entityType.GetField(iconFieldName, flags);
        FieldInfo spriteNameField = entityType.GetField(spriteNameFieldName, flags);
        FieldInfo keyField = entityType.GetField("Key", flags);

        if (keyField == null)
        {
            Debug.LogError($"[오류] {entityType.Name}에 'Key' 필드가 없습니다.");
            return;
        }

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = SplitCsvLine(lines[i]);
            if (values.Length == 0)
                continue;

            PartData entity = new PartData();

            for (int j = 0; j < headers.Length && j < values.Length; j++)
            {
                string header = headers[j];
                string rawValue = values[j].Trim().Trim('"');

                FieldInfo field = entityType.GetField(header, flags);
                if (field == null)
                {
                    Debug.LogWarning($"[경고] {entityType.Name}에 '{header}' 필드가 없습니다. (행 {i + 1})");
                    continue;
                }

                object convertedValue = ConvertValue(rawValue, field.FieldType);
                field.SetValue(entity, convertedValue);
            }

            TryAssignSprite(entity, iconField, spriteNameField, i + 1);

            int key = (int)keyField.GetValue(entity);
            if (key == 0)
            {
                Debug.LogWarning($"[경고] 행 {i + 1}의 Key 값이 0이거나 비어 있습니다. 이 행은 건너뜁니다.");
                continue;
            }

            if (database.partDic.ContainsKey(key))
            {
                Debug.LogWarning($"[중복 ID] Key {key} 가 중복되어 기존 데이터를 덮어씁니다.");
                database.partDic[key] = entity;
            }
            else
            {
                database.partDic.Add(key, entity);
            }
        }

        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorGUIUtility.PingObject(database);

        Debug.Log($"<color=#00FF00><b>[완료]</b></color> {typeof(PartData).Name} 데이터 {database.partDic.Count}개 임포트 성공!");
    }

    private void TryAssignSprite(PartData entity, FieldInfo iconField, FieldInfo spriteNameField, int row)
    {
        if (iconField == null || spriteNameField == null)
            return;

        string spriteName = spriteNameField.GetValue(entity) as string;
        if (string.IsNullOrEmpty(spriteName))
            return;

        string[] guids = AssetDatabase.FindAssets($"{spriteName} t:Sprite", new[] { spriteFolderPath });

        if (guids.Length > 0)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            Sprite foundSprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);

            if (foundSprite != null)
                iconField.SetValue(entity, foundSprite);
            else
                Debug.LogWarning($"[이미지 누락] 행 {row}: '{spriteName}' 경로에서 Sprite 로드 실패 ({assetPath})");
        }
        else
        {
            Debug.LogWarning($"[이미지 누락] 행 {row}: '{spriteName}' 이름의 Sprite를 '{spriteFolderPath}'에서 찾을 수 없습니다.");
        }
    }

    private string[] SplitCsvLine(string line)
    {
        return Regex.Split(line, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
    }

    private object ConvertValue(string value, Type type)
    {
        if (type == typeof(string))
            return value.Replace(';', ',').Replace("\\n", "\n").Replace(". ", ".\n");

        if (string.IsNullOrWhiteSpace(value))
        {
            if (type == typeof(int)) return 0;
            if (type == typeof(float)) return 0f;
            if (type == typeof(bool)) return false;
            if (type == typeof(List<Vector2Int>)) return new List<Vector2Int>();
            return null;
        }

        try
        {
            if (type == typeof(int))
                return int.Parse(value, CultureInfo.InvariantCulture);

            if (type == typeof(float))
                return float.Parse(value, CultureInfo.InvariantCulture);

            if (type == typeof(bool))
            {
                string v = value.ToLowerInvariant();
                return v == "true" || v == "1" || v == "yes" || v == "on";
            }

            if (type.IsEnum)
                return Enum.Parse(type, value, true);

            if (type == typeof(List<Vector2Int>))
            {
                List<Vector2Int> result = new List<Vector2Int>();

                // 예: "0,0|1,0|1,1"
                string[] pairs = value.Split('|');

                foreach (string pair in pairs)
                {
                    if (string.IsNullOrWhiteSpace(pair))
                        continue;

                    string[] xy = pair.Split(',');

                    if (xy.Length != 2)
                    {
                        Debug.LogWarning($"[Vector2Int 파싱 실패] '{pair}'는 x,y 형식이 아닙니다.");
                        continue;
                    }

                    int x = int.Parse(xy[0].Trim(), CultureInfo.InvariantCulture);
                    int y = int.Parse(xy[1].Trim(), CultureInfo.InvariantCulture);

                    result.Add(new Vector2Int(x, y));
                }

                return result;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[변환 실패] value: {value}, targetType: {type.Name}, error: {e.Message}");
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        return value;
    }
}