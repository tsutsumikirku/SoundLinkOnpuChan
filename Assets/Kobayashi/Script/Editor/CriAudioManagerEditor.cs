#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CriWare;
using CriWare.Editor;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CriSEManager))]
public class CriAudioManagerEditor : Editor
{
    const string StreamingAssetsPath = "Assets/StreamingAssets";
    public override void OnInspectorGUI()
    { 
        DrawPropertiesExcluding(
            serializedObject,
            "SeCueSheetData",
            "SeAudioData"
        );
        // 変数の描画
        serializedObject.Update();
        
        SerializedProperty seCueSheetData = serializedObject.FindProperty("SeCueSheetData");
        SerializedProperty seAudioData = serializedObject.FindProperty("SeAudioData");
        EditorGUILayout.PropertyField(seCueSheetData, true);
        EditorGUILayout.PropertyField(seAudioData, true);
        // その下にボタンを配置
        if (GUILayout.Button("Load CueSheet"))
        {
            CriSEManager src = (CriSEManager)target;
            AcbDataToAudioData(src.SeCueSheetData, src.SeAudioData);
            EditorUtility.SetDirty(src);
        }

        serializedObject.ApplyModifiedProperties();
        // base.OnInspectorGUI();
        //
        //
        // CriAudioManager src = (CriAudioManager)target;
        // if (GUILayout.Button("エクスポート"))
        // {
        //     AcbDataToAudioData(src);
        // }
    }

    void AcbDataToAudioData(
        CriSEManager.CueSheetData[] cueSheetData, 
        List<CriSEManager.AudioData> audioData)
    {
        if (!CriAtomPlugin.IsLibraryInitialized())
        {
            CriAtomPlugin.InitializeLibrary();
        }

        var oldList = new List<CriSEManager.AudioData>(audioData);
        audioData.Clear();
        for (int i = 0; i < cueSheetData.Length; i++)
        {
            var cueSheet = cueSheetData[i];
            //ファイル名からpathを取得
            var acbDataPath = FindAssetPathByName(cueSheet.acbFileName, StreamingAssetsPath,true);
            var awbDataPath = FindAssetPathByName(cueSheet.awbFileName, StreamingAssetsPath,true);
            
            //PathからAcbファイルを取得する
            var acb = CriAtomEditorUtilities.LoadAcbFile(null,
                acbDataPath, awbDataPath);
            var info = acb.GetCueInfoList();
            
            
            //acbをAudioDataに変更(Volumeとpitchを変更するため)
            audioData.AddRange(info.Select( (x,j) =>
            {
                var data = oldList.Find(oldData => oldData.AudioName == x.name);
                if (data == null)
                {
                    return new CriSEManager.AudioData()
                    {
                        CueSheetID = i,
                        AudioID = j,
                        CueSheetName = cueSheet.cueSheetName,
                        AudioName = x.name,
                    };
                }
                return data;
            }));
        }
    }
    
    private　static string FindAssetPathByName(string targetName, string searchFolder, bool fullPath = false)
    {
        //拡張子を除去する(Asset検索で不要なため)
        targetName = Path.GetFileNameWithoutExtension(targetName);
        string[] guids = AssetDatabase.FindAssets(targetName, new[] { searchFolder });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (System.IO.Path.GetFileNameWithoutExtension(path) == targetName)
            {
                if (fullPath)
                {
                    path = ToFullPath(path);
                }
                Debug.Log("Found asset at path: " + path);
                return path;
            }
        }
        Debug.Log("No asset at path: " + targetName);
        return null; // 見つからなかった場合
        
        //C:のパスを取得する
        string ToFullPath(string relativePath)
        {
            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            var path = Path.Combine(projectRoot, relativePath);
            path = path.Replace("\\", "/");
            return path;
        }
    }
}
#endif