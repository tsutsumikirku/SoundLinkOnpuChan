using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using DG.Tweening.Plugins.Core.PathCore;

[RequireComponent(typeof(PlatformBehaviour))]
public class MovePlatform : ExecutableCancellableBase
{
    [SerializeField,Label("経由地点")] private Vector3[] localWaypoints = new Vector3[] { Vector3.right };
    [SerializeField,Label("移動にかかる時間")] private float duration = 1f;
    [SerializeField,Label("移動の種類")] private PathType pathType;
    [SerializeField,Label("始点と終点を結ぶか")] private bool closePath;
    [SerializeField,Label("ループ回数")] private LoopTimes loopTimes;
    [SerializeField,Label("ループ設定")] private LoopType loopType;
    [SerializeField,Label("動き始めるタイミング")] private MoveTiming moveTiming;
    [SerializeField,Label("衝突時の対象")] private LayerMask targetLayer;
    // ハンドル（アンカーごとのローカルオフセット）
    [Tooltip("各アンカー（先頭は origin）に対する左ハンドルのローカルオフセット")] [SerializeField]
    private Vector3[] handleLeftOffsets;
    [Tooltip("各アンカー（先頭は origin）に対する右ハンドルのローカルオフセット")] [SerializeField]
    private Vector3[] handleRightOffsets;


    public Vector3[] HandleLeftOffsets => handleLeftOffsets;
    public Vector3[] HandleRightOffsets => handleRightOffsets;
    public bool ClosePath => closePath;
    private Vector3[] _waypoints; // DOTween に渡す pts 配列
    private PlatformBehaviour _platformBehaviour;
    private Tweener _tween;
    private bool _isBackwards;
    public Vector3[] LocalWaypoints => localWaypoints;
    [SerializeField, Range(2, 64)] private int pathResolution = 10; // 曲線の分割数（高いほど滑らか）
    private Vector3 _basePosition;

    private void OnEnable()
    {
        _basePosition = transform.position;
        EnsureHandleArrays();
    }

    private void Start()
    {
        _platformBehaviour = GetComponent<PlatformBehaviour>();
        TweenInitialize();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log(collision.gameObject.name);
        Debug.Log(collision.gameObject.IsInLayerMask(targetLayer));
        if (moveTiming == MoveTiming.OnTarget && collision.gameObject.IsInLayerMask(targetLayer) && collision.contacts[0].normal == -(Vector2)transform.up)
        {
            Move();
        }
    }

    protected override void OnExecuteCancelled()
    {
        _platformBehaviour.ResetPlatformTarget();
        _isBackwards = false;
        _tween?.Rewind();
    }

    protected override void OnExecuteBegin()
    {
        if (moveTiming == MoveTiming.OnExecuteBegin)
        {
            Move();
        }
    }

    private void TweenInitialize()
    {
        if (_tween != null)
        {
            _tween.Kill();
            _tween = null;
        }

        if (localWaypoints == null || localWaypoints.Length == 0)
        {
            Debug.LogWarning($"[{name}] localWaypoints is null/empty. Aborting TweenInitialize.");
            return;
        }

        if (duration <= 0f)
        {
            Debug.LogWarning($"[{name}] duration <= 0. Aborting TweenInitialize.");
            return;
        }

        // 基準位置（起動時にキャプチャした basePosition）
        Vector3 origin = _basePosition;
        var tr = transform;

        // anchors は world 空間のアンカー列（origin を 0 番に含む）
        List<Vector3> anchors = new List<Vector3> { origin };
        for (int i = 0; i < localWaypoints.Length; i++)
        {
            // localWaypoints は origin 基準のローカルオフセットを想定しているので
            // transform.TransformPoint を使ってワールド座標を得る（回転対応）
            Vector3 anchorWorld = tr.TransformPoint(localWaypoints[i]);
            anchors.Add(anchorWorld);
        }

        if (pathType == PathType.CubicBezier)
        {
            // DOTween の要件: 渡す配列は「3 の倍数」で、最初の開始点は自動追加されるため
            // ここでは origin を含めた anchors から「origin を除く各アンカーごと」に
            // [ waypoint, IN_control (prev out), OUT_control (curr in) ] の順で追加した配列を作る
            _waypoints = BuildCubicPtsFromAnchors(anchors.ToArray());
        }
        else
        {
            // Linear/Catmull 用は anchors をそのまま渡す（ただし DOPath は最初に transform.position を自動で扱う場合もあるが
            // non-cubic では transform.DOPath にそのまま与えるのが普通）
            _waypoints = anchors.ToArray();
        }

        _tween = transform.DOPath(_waypoints, duration, pathType, PathMode.Full3D, pathResolution, Color.green)
            .SetOptions(closePath)
            .SetEase(Ease.Linear)
            .SetLoops((int)loopTimes, loopType)
            .SetAutoKill(false);
        _tween.Rewind();
    }

    [ContextMenu("Move")]
    private void Move()
    {
        if (_tween == null) TweenInitialize();
        if (_tween.IsPlaying()) return;

        if (loopTimes == LoopTimes.OneWay && _isBackwards)
        {
            Debug.LogWarning("逆再生");
            _isBackwards = false;
            _tween.PlayBackwards();
        }
        else
        {
            _isBackwards = true;
            _tween.Restart();
        }
    }

    private enum LoopTimes
    {
        Infinite = -1,
        OneWay = 1,
        PingPong = 2,
    }

    private enum MoveTiming
    {
        OnExecuteBegin,
        OnTarget
    }

    private void OnValidate()
    {
        if (localWaypoints == null || localWaypoints.Length == 0)
            localWaypoints = new[] { Vector3.right };

        // 編集時は transform.position に合わせて base を更新
        _basePosition = transform.position;

        EnsureHandleArrays();

        // エディタで変更されたら再初期化はプレイ時のみ
        if (Application.isPlaying)
        {
            TweenInitialize();
        }
    }

    // OnDrawGizmosSelected の該当部分を丸ごと差し替え
private void OnDrawGizmosSelected()
{
    if (localWaypoints == null || localWaypoints.Length == 0) return;

    // 実行中は basePosition を起点に、エディタ時は transform.position を起点にする
    Vector3 origin = Application.isPlaying ? _basePosition : transform.position;
    Quaternion rot = transform.rotation; // 回転は transform.rotation を使う

    // anchors（world）: origin を先頭にして、localWaypoints は origin 基準のオフセットとして扱う
    Vector3[] anchors = new Vector3[1 + localWaypoints.Length];
    anchors[0] = origin;
    for (int i = 0; i < localWaypoints.Length; i++)
    {
        // ここを origin + local にしてたのが問題。回転を考慮して origin + rot * local にする。
        anchors[i + 1] = origin + rot * localWaypoints[i];
    }

    // waypoint 点を表示
    Gizmos.color = Color.yellow;
    for (int i = 0; i < anchors.Length; i++)
    {
        Gizmos.DrawSphere(anchors[i], 0.15f);
    }

    EnsureHandleArrays();

    if (pathType == PathType.CubicBezier)
    {
        // --- 既存の CubicBezier 描画処理（そのまま） ---
        Vector3[] leftWorld = new Vector3[anchors.Length];
        Vector3[] rightWorld = new Vector3[anchors.Length];

        for (int i = 0; i < anchors.Length; i++)
        {
            Vector3 anchorLocal = (i == 0) ? Vector3.zero : localWaypoints[i - 1];
            Vector3 anchorWorld = origin + rot * anchorLocal;

            leftWorld[i] = anchorWorld + handleLeftOffsets[i];
            rightWorld[i] = anchorWorld + handleRightOffsets[i];

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(anchorWorld, leftWorld[i]);
            Gizmos.DrawLine(anchorWorld, rightWorld[i]);

            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(leftWorld[i], 0.08f);
            Gizmos.DrawSphere(rightWorld[i], 0.08f);
        }

        int segCount = closePath ? anchors.Length : anchors.Length - 1;
        Gizmos.color = Color.gray;
        for (int s = 0; s < segCount; s++)
        {
            int i = s;
            int j = (s + 1) % anchors.Length;
            Gizmos.DrawLine(rightWorld[i], leftWorld[j]);
        }

        int resB = Mathf.Max(2, pathResolution);
        List<Vector3> wps = new List<Vector3>(anchors);
        List<ControlPoint> cps = new List<ControlPoint>();
        for (int s = 0; s < segCount; s++)
        {
            int j = (s + 1) % anchors.Length;
            ControlPoint cp = new ControlPoint { a = rightWorld[s], b = leftWorld[j] };
            cps.Add(cp);
        }

        Vector3[] wpsArr = wps.ToArray();
        ControlPoint[] controlPoints = cps.ToArray();

        List<Vector3> drawPoints = new List<Vector3>();
        int segments = closePath ? wpsArr.Length : wpsArr.Length - 1;
        for (int seg = 0; seg < segments; seg++)
        {
            for (int s = 0; s <= resB; s++)
            {
                float t = s / (float)resB;
                float perc = (seg + t) / segments;
                Vector3 pos = GetPoint(perc, wpsArr, controlPoints);
                if (drawPoints.Count == 0 || (drawPoints[^1] - pos).sqrMagnitude > 1e-6f)
                    drawPoints.Add(pos);
            }
        }

        if (drawPoints.Count >= 2)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < drawPoints.Count - 1; i++) Gizmos.DrawLine(drawPoints[i], drawPoints[i + 1]);
        }
    }
    else if (pathType == PathType.CatmullRom || pathType == PathType.CatmullRom) // PathType名に合わせて調整
    {
        // Catmull-Rom 曲線をサンプリングして描画
        int res = Mathf.Max(2, pathResolution);
        List<Vector3> samples = SampleCatmullRomPoints(anchors, closePath, res);

        if (samples.Count >= 2)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < samples.Count - 1; i++) Gizmos.DrawLine(samples[i], samples[i + 1]);
        }
    }
    else
    {
        // Linear / その他（既存の直線描画）
        List<Vector3> drawPoints = new List<Vector3>();
        drawPoints.AddRange(anchors);
        if (closePath) drawPoints.Add(anchors[0]);
        Gizmos.color = Color.green;
        for (int i = 0; i < drawPoints.Count - 1; i++) Gizmos.DrawLine(drawPoints[i], drawPoints[i + 1]);
    }
}

// Catmull-Rom のサンプリング関数
private List<Vector3> SampleCatmullRomPoints(Vector3[] anchors, bool closePath, int resolutionPerSegment)
{
    List<Vector3> outPts = new List<Vector3>();
    int n = anchors.Length;
    if (n < 2) return outPts;

    int segments = closePath ? n : n - 1;

    for (int seg = 0; seg < segments; seg++)
    {
        // セグメントは anchors[seg] -> anchors[(seg+1)%n]
        Vector3 p1 = anchors[seg];
        Vector3 p2 = anchors[(seg + 1) % n];

        // p0, p3 の選び方
        Vector3 p0, p3;
        if (seg == 0)
        {
            p0 = closePath ? anchors[(seg - 1 + n) % n] : anchors[seg]; // 開ループでは端点複製
        }
        else
        {
            p0 = anchors[seg - 1];
        }

        if (seg + 2 < n)
        {
            p3 = anchors[(seg + 2) % n];
        }
        else
        {
            p3 = closePath ? anchors[(seg + 2) % n] : anchors[(seg + 1) % n]; // 開ループでは端点複製
        }

        for (int i = 0; i <= resolutionPerSegment; i++)
        {
            float t = i / (float)resolutionPerSegment;
            Vector3 pos = CatmullRom(p0, p1, p2, p3, t);
            if (outPts.Count == 0 || (outPts[^1] - pos).sqrMagnitude > 1e-6f)
                outPts.Add(pos);
        }
    }

    return outPts;
}

// 標準的な Catmull-Rom（tension = 0.5）
// p0,p1,p2,p3 のうち p1->p2 が補間対象
private Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
{
    float t2 = t * t;
    float t3 = t2 * t;
    return 0.5f * (
        (2f * p1) +
        (-p0 + p2) * t +
        (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
        (-p0 + 3f * p1 - 3f * p2 + p3) * t3
    );
}


    // ハンドル配列の長さを anchorsCount に合わせる
    private void EnsureHandleArrays()
    {
        int anchorsCount = 1 + (localWaypoints?.Length ?? 0);
        if (handleLeftOffsets == null || handleLeftOffsets.Length != anchorsCount)
        {
            Vector3[] newLeft = new Vector3[anchorsCount];
            for (int i = 0; i < anchorsCount; i++)
            {
                if (handleLeftOffsets != null && i < handleLeftOffsets.Length) newLeft[i] = handleLeftOffsets[i];
                else newLeft[i] = Vector3.left * 3f;
            }

            handleLeftOffsets = newLeft;
        }

        if (handleRightOffsets == null || handleRightOffsets.Length != anchorsCount)
        {
            Vector3[] newRight = new Vector3[anchorsCount];
            for (int i = 0; i < anchorsCount; i++)
            {
                if (handleRightOffsets != null && i < handleRightOffsets.Length) newRight[i] = handleRightOffsets[i];
                else newRight[i] = Vector3.right * 3f;
            }

            handleRightOffsets = newRight;
        }
    }

    // CubicBezier 用 pts 配列を作る（DOTween に渡す形式： p0, c0, c1, p1, c0, c1, p2, ...）
    private Vector3[] BuildCubicPtsFromAnchors(Vector3[] anchors)
    {
        var tr = transform;
        int anchorsCount = anchors.Length; // origin + N
        if (anchorsCount < 2) return Array.Empty<Vector3>();

        // Ensure handle arrays are ready (origin を含む長さ)
        EnsureHandleArrays();

        List<Vector3> pts = new List<Vector3>();

        // 通常セグメント: i は 1..anchorsCount-1 の各アンカー（origin を除く）
        for (int i = 1; i < anchorsCount; i++)
        {
            // waypoint = anchors[i] (world)
            Vector3 waypointWorld = anchors[i];

            // IN control = previous anchor の OUT(handleRight)
            // previous anchor のローカル位置
            Vector3 prevAnchorLocal = (i - 1 == 0) ? Vector3.zero : localWaypoints[i - 2];
            Vector3 prevOutWorld = tr.TransformPoint(prevAnchorLocal + handleRightOffsets[i - 1]);

            // OUT control = current anchor の IN(handleLeft)
            Vector3 currAnchorLocal = (i == 0) ? Vector3.zero : localWaypoints[i - 1];
            Vector3 currInWorld = tr.TransformPoint(currAnchorLocal + handleLeftOffsets[i]);

            // DOTween の期待順: [ waypoint, IN_control(on prev), OUT_control(on curr) ]
            pts.Add(waypointWorld);
            pts.Add(prevOutWorld);
            pts.Add(currInWorld);
        }

        // 閉ループのときは、origin についてのグループも追加して閉じる（これで 3 の倍数になる）
        if (closePath)
        {
            // waypoint = origin (anchors[0])
            Vector3 waypointWorld = anchors[0];

            // IN control = last anchor の OUT
            int lastIdx = anchorsCount - 1;
            Vector3 lastAnchorLocal = (lastIdx == 0) ? Vector3.zero : localWaypoints[lastIdx - 1];
            Vector3 lastOutWorld = tr.TransformPoint(lastAnchorLocal + handleRightOffsets[lastIdx]);

            // OUT control = origin の IN handle (handleLeftOffsets[0])
            Vector3 originInWorld = tr.TransformPoint(Vector3.zero + handleLeftOffsets[0]);

            pts.Add(waypointWorld);
            pts.Add(lastOutWorld);
            pts.Add(originInWorld);
        }

        // ここで pts.Count は常に 3 の倍数になるはず
        if (pts.Count % 3 != 0)
        {
            Debug.LogWarning($"[BuildCubicPtsFromAnchors] pts.Count % 3 != 0 : {pts.Count}. This should not happen.");
        }

        return pts.ToArray();
    }

    // DOTween の内部的な Bezier 点計算 (ControlPoint[] を受け取る)
    Vector3 GetPoint(
        float perc,
        Vector3[] wps,
        ControlPoint[] controlPoints)
    {
        int num1 = wps.Length - 1;
        int num2 = (int)Math.Floor(perc * (double)num1);
        int index = num1 - 1;
        if (index > num2)
            index = num2;
        float num3 = perc * num1 - index;
        Vector3 wp1 = wps[index];
        Vector3 a = controlPoints[index].a;
        Vector3 b = controlPoints[index].b;
        Vector3 wp2 = wps[(index + 1) % wps.Length];
        float num4 = 1f - num3;
        float num5 = num3 * num3;
        float num6 = num4 * num4;
        double num7 = num6 * (double)num4;
        float num8 = num5 * num3;
        Vector3 vector3 = wp1;
        return (float)num7 * vector3 + 3f * num6 * num3 * a + 3f * num4 * num5 * b + num8 * wp2;
    }
}