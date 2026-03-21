using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SpikeMove : ExecutableCancellableBase
{
    Sequence _tween;
    [SerializeField] Transform _end;
    [SerializeField] float _moveTime;
    [SerializeField] Ease _easing;
    [SerializeField] private bool _firstActive; //はじめにとげを出すか
    [SerializeField] private float _interval;

    private Vector2 _start;
    private float _timer;

    private void Start()
    {
        _start = transform.position;
    }


    protected override void OnExecuteBegin()
    {
        StartSpikeCycle();
    }

    void StartSpikeCycle()
    {
        Vector3 startPos = _firstActive ? _start : _end.position;
        Vector3 nextPos = _firstActive ? _end.position : _start;

        transform.position = startPos; // 最初の位置を設定

        _tween = DOTween.Sequence();
        _tween.AppendInterval(_interval) // 最初の待機時間
                     .Append(transform.DOMove(nextPos, _moveTime).SetEase(_easing)) // 移動
                     .AppendInterval(_interval) // 次の待機時間
                     .SetLoops(-1, LoopType.Yoyo); // ループで繰り返し
    }

    private void OnDrawGizmos()
    {
        if (_end != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, _end.position);
        }
    }

    protected override void OnExecuteCancelled()
    {
        transform.position = _start;
        _tween.Kill();
    }
}
