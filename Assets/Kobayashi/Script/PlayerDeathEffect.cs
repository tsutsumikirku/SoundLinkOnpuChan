#region

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

#endregion

public class PlayerDeathEffect : MonoBehaviour
{
    #region Curve設定

        [SerializeField] private AnimationCurve _fallCurve =
        new(
            new Keyframe(0f, 0f, 0f, 5f), // 開始点 (上昇の勢い)
            new Keyframe(0.3f, 1f, 0f, 0f), // 少し上に
            new Keyframe(1f, 0f, -5f, 0f) // 落下
        );

    [SerializeField] private AnimationCurve _xMoveCurve =
        new(
            new Keyframe(0f, 0f, 0f, 0f), //X軸の移動 
            new Keyframe(1f, 1f, 0f, 1f)
        );

    [SerializeField] private AnimationCurve _rotationCurve =
        new(
            new Keyframe(0f, 0f, 0f, 360f), // 開始点 (回転の勢い)
            new Keyframe(1f, 90, 0f, 0f)
        );

    #endregion
    [SerializeField] [Header("死亡時のジャンプの高さ")]
    private float _height = 3.0f;

    [SerializeField] [Header("アニメーションの時間")]
    private float _duration = 1.0f; // アニメーション時間

    [SerializeField] [Header("通常時のX軸吹っ飛びの向き")]
    private int _defaultDirection = -1;

    [SerializeField] private float _hitStopTime = 0.2f;
    [SerializeField] private GameObject _outLine;
    private Animator _animator;
    private CancellationTokenSource _cts;
    private int _direction = 1;
    private Vector3 _startPos;
    private float _time;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;
        AnimStart(token).Forget();
        if (_outLine != null) _outLine.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    }

    private void OnDestroy()
    {
        _cts.Cancel();
        _cts.Dispose();
    }

    public void Init(int directionX, int directionY)
    {
        _direction = directionX < 0 ? -1 : 1;
        directionY = directionY < 0 ? -1 : 1;
        //重力反転時にflipさせる
        transform.localScale = Vector3.Scale(transform.localScale, new Vector3(directionY, 1, 1));
    }

    private async UniTask AnimStart(CancellationToken token)
    {
        try
        {
            _startPos = transform.position;
            _animator.speed = 0;
            await UniTask.WaitForSeconds(_duration, cancellationToken: token);
            _animator.speed = 1;
            await fallAnim(token);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async UniTask fallAnim(CancellationToken token = default)
    {
        var defaultRotation = transform.rotation;
        try
        {
            while (_time < _duration)
            {
                token.ThrowIfCancellationRequested();
                _time += Time.deltaTime;
                var t = _time / _duration;

                // カーブの値を取得 (0～1)
                var curveValue = _fallCurve.Evaluate(t);
                var _rotationCurveValue = _rotationCurve.Evaluate(t);
                var xMoveCurveValue = _xMoveCurve.Evaluate(t) * _defaultDirection * _direction;
                // Y座標をカーブに従って変化させる
                transform.position = _startPos + Vector3.up * curveValue * _height + Vector3.right * xMoveCurveValue;
                transform.rotation = Quaternion.Euler(0f, 0f,
                    defaultRotation.eulerAngles.z + _rotationCurveValue　* _direction);
                await UniTask.WaitForFixedUpdate(token);
            }

            gameObject.SetActive(false);
        }
        catch (OperationCanceledException)
        {
        }
    }

    [ContextMenu("Reset")]
    private void AnimationReset()
    {
        _time = 0;
        gameObject.SetActive(true);
    }

    
}