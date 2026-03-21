
#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class TimeScaleDebug : MonoBehaviour
{
    //以下テスト用
    public float _testTimeScale;
    [ContextMenu("TimeScale")]
    public void ChangeTimeScale()
    {
        if (_testTimeScale <= 0) Debug.Log("Time Scaleに0以下の値が入力されました。");
        TimeScaleManager.ChangeTimeScale(_testTimeScale);
    }
}
[CustomEditor(typeof(TimeScaleDebug))]
public class TimeScaleDebugEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TimeScaleDebug script = (TimeScaleDebug)target;

        // ランタイム中（Playモード中）のみ実行
        if (GUILayout.Button("TimeScaleChange"))
        {
            if (Application.isPlaying)
            {
                Debug.Log($"Time scaleが{script._testTimeScale}に設定されました。");
                script.ChangeTimeScale();
            }
            else
            {
                Debug.LogWarning("この処理はPlayモード中にしか実行できません！");
            }
        }
    }
}
#endif
