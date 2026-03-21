using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutGameSystem : MonoBehaviour
{
    StageData _stageData;
    string path = "OutGameSystem";
    void Awake()
    {
        
    }
}
[System.Serializable]
public class StageData
{
    public int StageClearFlag = 0b0000000000000000000000000000000;
}
