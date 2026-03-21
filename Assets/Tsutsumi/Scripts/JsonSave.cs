using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

public static class JsonSave
{
    public static void Save<T>(string fileName, T data)
    {
        try
        {
            string path;
            path = Path.Combine(Application.persistentDataPath, fileName + ".json");
            string json = JsonUtility.ToJson(data, true); // trueでインデント付き保存
            File.WriteAllText(path, json);
            // もしもtrueの場合はSave.jsonに追加しない
            // Save.json の管理データを更新
            string saveListPath = Path.Combine(Application.persistentDataPath, "Save.json");
            JsonPath jsonPath;

            if (File.Exists(saveListPath))
            {
                jsonPath = JsonUtility.FromJson<JsonPath>(File.ReadAllText(saveListPath)) ?? new JsonPath();
            }
            else
            {
                jsonPath = new JsonPath();
            }

            if (!jsonPath.Path.Contains(fileName))
            {
                jsonPath.Path.Add(fileName);
                File.WriteAllText(saveListPath, JsonUtility.ToJson(jsonPath, true));
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Save failed: {e.Message}");
        }
    }

    public static T Load<T>(string fileName)
    {
        try
        {
            string path = Path.Combine(Application.persistentDataPath, fileName + ".json");
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                return JsonUtility.FromJson<T>(json);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Load failed: {e.Message}");
        }

        return default;
    }
    public static bool TryLoad<T>(string fileName, out T result)
    {
        result = default;

        try
        {
            string path = Path.Combine(Application.persistentDataPath, fileName + ".json");
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                result = JsonUtility.FromJson<T>(json);
                return true;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"TryLoad failed: {e.Message}");
        }

        return false;
    }

    public static void Reset()
    {
        try
        {
            string saveListPath = Path.Combine(Application.persistentDataPath, "Save.json");
            if (!File.Exists(saveListPath)) return;

            JsonPath jsonPath = JsonUtility.FromJson<JsonPath>(File.ReadAllText(saveListPath));
            if (jsonPath?.Path == null) return;

            foreach (var fileName in jsonPath.Path)
            {
                string path = Path.Combine(Application.persistentDataPath, fileName + ".json");
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }

            // Save.json も削除
            File.Delete(saveListPath);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Reset failed: {e.Message}");
        }
    }
}

[System.Serializable]
public class JsonPath
{
    public List<string> Path = new List<string>();
}
