using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MovePlatform))]
public class MovePlatformEditor : Editor
{
    SerializedProperty waypointsProp;

    void OnEnable()
    {
        waypointsProp = serializedObject.FindProperty("localWaypoints");
    }
    void OnSceneGUI()
    {
        var mp = (MovePlatform)target;
        var tr = mp.transform;

        // localWaypoints の移動ハンドル（アンカーのうち origin を除く）
        for (int i = 0; i < mp.LocalWaypoints.Length; i++)
        {
            Vector3 localPos = mp.LocalWaypoints[i];
            Vector3 worldPos = tr.TransformPoint(localPos);

            EditorGUI.BeginChangeCheck();
            Vector3 newWorldPos = Handles.PositionHandle(worldPos, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(mp, "Move Waypoint");
                mp.LocalWaypoints[i] = tr.InverseTransformPoint(newWorldPos);
                EditorUtility.SetDirty(mp);
            }
        }

        // ハンドル編集（CubicBezier の場合）
        if (mp != null)
        {
            // Ensure arrays
            var propLeft = serializedObject.FindProperty("handleLeftOffsets");
            var propRight = serializedObject.FindProperty("handleRightOffsets");
            serializedObject.Update();
            int anchorsCount = 1 + mp.LocalWaypoints.Length;
            if (propLeft.arraySize != anchorsCount) propLeft.arraySize = anchorsCount;
            if (propRight.arraySize != anchorsCount) propRight.arraySize = anchorsCount;
            serializedObject.ApplyModifiedProperties();

            if (mp != null && mp.GetType() != null && mp != null && mp.GetType() != null)
            {
                if (mp != null && (mp.GetType() == typeof(MovePlatform)))
                {
                    // Draw and allow editing of handle offsets in world space
                    for (int i = 0; i < anchorsCount; i++)
                    {
                        Vector3 anchorLocal = (i == 0) ? Vector3.zero : mp.LocalWaypoints[i - 1];
                        Vector3 anchorWorld = tr.TransformPoint(anchorLocal);

                        // left handle
                        Vector3 leftWorld = tr.TransformPoint(anchorLocal + mp.HandleLeftOffsets[i]);
                        EditorGUI.BeginChangeCheck();
                        Vector3 newLeftWorld = Handles.PositionHandle(leftWorld, Quaternion.identity);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(mp, "Move Left Handle");
                            Vector3 newLocalOffset = tr.InverseTransformPoint(newLeftWorld) - anchorLocal;
                            mp.HandleLeftOffsets[i] = newLocalOffset;
                            EditorUtility.SetDirty(mp);
                        }

                        // right handle
                        Vector3 rightWorld = tr.TransformPoint(anchorLocal + mp.HandleRightOffsets[i]);
                        EditorGUI.BeginChangeCheck();
                        Vector3 newRightWorld = Handles.PositionHandle(rightWorld, Quaternion.identity);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(mp, "Move Right Handle");
                            Vector3 newLocalOffset = tr.InverseTransformPoint(newRightWorld) - anchorLocal;
                            mp.HandleRightOffsets[i] = newLocalOffset;
                            EditorUtility.SetDirty(mp);
                        }

                        // 小さなラベル
                        Handles.Label(anchorWorld + Vector3.up * 0.2f, $"A{i}");
                    }

                    // セグメント毎に右ハンドル -> 次のアンカーの左ハンドル を線で結ぶ
                    Handles.color = Color.gray;
                    int segCount = mp.ClosePath ? anchorsCount : anchorsCount - 1;
                    for (int s = 0; s < segCount; s++)
                    {
                        int j = (s + 1) % anchorsCount;
                        Vector3 anchorLocalS = (s == 0) ? Vector3.zero : mp.LocalWaypoints[s - 1];
                        Vector3 anchorLocalJ = (j == 0) ? Vector3.zero : mp.LocalWaypoints[j - 1];
                        Vector3 rightWorld = tr.TransformPoint(anchorLocalS + mp.HandleRightOffsets[s]);
                        Vector3 leftWorld = tr.TransformPoint(anchorLocalJ + mp.HandleLeftOffsets[j]);
                        Handles.DrawLine(rightWorld, leftWorld);
                    }
                }
            }
        }
    }
}