using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class DebugButtons : MonoBehaviour
{
    [SerializeField] GameObject[] buttons;
    void Awake()
    {
        foreach (var button in buttons)
        {
            button.SetActive(true);
        }
    }

    public void StageAllClear()
    {
        var names = GetBuildSceneNames();
        for (int i = 0; i < names.Count; i++)
        {
            JsonSave.Save(names[i] + "StageSelect", StageSelectState.Clear);
        }
    }

    private static List<string> GetBuildSceneNames()
    {
        var list = new List<string>();
        int count = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < count; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i); // e.g. "Assets/Scenes/MyScene.unity"
            string name = Path.GetFileNameWithoutExtension(path);
            list.Add(name);
        }
        return list;
    }
}

