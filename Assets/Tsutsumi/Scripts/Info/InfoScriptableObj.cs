using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "NewInfo", menuName = "ScriptableObjects/Info/Tsutsumi")]
public class InfoScriptableObj : ScriptableObject
{
    public List<InfoData> InfoDataArray;
}
[System.Serializable]
public class InfoData
{
    public string Text;
    public Sprite Image;
}