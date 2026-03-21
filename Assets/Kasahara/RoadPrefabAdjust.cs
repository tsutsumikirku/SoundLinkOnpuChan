#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
#endif

#if UNITY_EDITOR
[ExecuteAlways]
public class RoadPrefabAdjust : MonoBehaviour
{
    [SerializeField] private float width;
    [SerializeField] private SpriteRenderer centerSprite;
    [SerializeField] private SpriteRenderer centerUnderSprite;
    [SerializeField] private SpriteRenderer leftSprite;
    [SerializeField] private SpriteRenderer leftUnderSprite;
    [SerializeField] private SpriteRenderer rightSprite;
    [SerializeField] private SpriteRenderer rightUnderSprite;
    [SerializeField] private BoxCollider2D boxCollider;

    // インスタンス単位で重複スケジュールを防ぐフラグ
    private bool _pendingApply = false;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube((Vector2)transform.position + boxCollider.offset, boxCollider.size);
    }

    private void OnValidate()
    {
        // プレイ中は即時適用（ランタイムでは問題にならない）
        if (Application.isPlaying)
        {
            ApplyChanges();
            return;
        }

        // Editor の OnValidate 中は遅延実行する（SendMessage 禁止タイミングを回避）
        if (!_pendingApply)
        {
            _pendingApply = true;
            EditorApplication.delayCall += () =>
            {
                // オブジェクトが既に消されてたら何もしない
                if (this == null)
                {
                    _pendingApply = false;
                    return;
                }

                _pendingApply = false;
                ApplyChanges();

                // 変更をエディタに伝える（必要ならシーンやプレハブが保存可能になる）
                EditorUtility.SetDirty(this);
            };
        }
    }

    private void ApplyChanges()
    {
        //必須フィールドが null なら何もしない
        if (centerSprite == null || centerUnderSprite == null ||
            leftSprite == null || leftUnderSprite == null ||
            rightSprite == null || rightUnderSprite == null ||
            boxCollider == null) return;

        // width の最低値を適当につける（0 や負は避ける）
        var w = Mathf.Max(0.01f, width);

        // Undo 対応
        Undo.RecordObject(centerSprite, "Adjust Road Prefab");
        Undo.RecordObject(centerUnderSprite, "Adjust Road Prefab");
        Undo.RecordObject(leftSprite.transform, "Adjust Road Prefab");
        Undo.RecordObject(leftUnderSprite.transform, "Adjust Road Prefab");
        Undo.RecordObject(rightSprite.transform, "Adjust Road Prefab");
        Undo.RecordObject(rightUnderSprite.transform, "Adjust Road Prefab");
        Undo.RecordObject(boxCollider, "Adjust Road Prefab");

        // サイズと位置を設定（draw mode が Tiled の想定）
        centerSprite.size = new Vector2(w, centerSprite.size.y);
        centerUnderSprite.size = new Vector2(w, centerUnderSprite.size.y);

        var halfCenter = w / 2f;
        var halfEdge = leftSprite.size.x / 2f;
        var halfRight = rightSprite.size.x / 2f;

        leftSprite.transform.localPosition =
            new Vector3(-(halfCenter + halfEdge), leftSprite.transform.localPosition.y,
                leftSprite.transform.localPosition.z);
        leftUnderSprite.transform.localPosition =
            new Vector3(-(halfCenter + halfEdge), leftUnderSprite.transform.localPosition.y,
                leftUnderSprite.transform.localPosition.z);
        rightSprite.transform.localPosition =
            new Vector3(halfCenter + halfRight, rightSprite.transform.localPosition.y,
                rightSprite.transform.localPosition.z);
        rightUnderSprite.transform.localPosition =
            new Vector3(halfCenter + halfRight, rightUnderSprite.transform.localPosition.y,
                rightUnderSprite.transform.localPosition.z);

        boxCollider.size = new Vector2(w + leftSprite.size.x, boxCollider.size.y);

        // Editor に変更を通知（シリアライズ更新）
        EditorUtility.SetDirty(centerSprite);
        EditorUtility.SetDirty(centerUnderSprite);
        EditorUtility.SetDirty(leftSprite);
        EditorUtility.SetDirty(leftUnderSprite);
        EditorUtility.SetDirty(rightSprite);
        EditorUtility.SetDirty(rightUnderSprite);
        EditorUtility.SetDirty(boxCollider);
    }
}
#endif