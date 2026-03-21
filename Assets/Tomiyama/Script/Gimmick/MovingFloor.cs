using DG.Tweening;
using UnityEngine;

public class MovingFloor : ExecutableCancellableBase
{
    private Tween _tween;
    [SerializeField] private Transform end;
    [SerializeField] private float time;
    [SerializeField] private Ease easing;
    [SerializeField] private PlatformBehaviour platformBehaviour;
    [SerializeField] private bool _isLoop = true;
    [SerializeField] private bool _isAutoMove = true;
    private Vector2 _start;

    private void Start()
    {
        _start = transform.position;
    }

    /// <summary>
    /// 手動で移動を開始したい場合のメソッド
    /// </summary>
    public void OnExecuteBeginEvent()
    {
        if(!_isAutoMove)
        {
            MoveStart();
        }
    }
    
    protected override void OnExecuteBegin()
    {
        if(_isAutoMove)
        {
            MoveStart();
        }
    }

    void MoveStart()
    {
        _tween = transform.DOMove(end.position, time).SetEase(easing);
        if(_isLoop)
        {
            _tween.SetLoops(-1, LoopType.Yoyo);
        }
    }

    private void OnDrawGizmos()
    {
        if (end != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, end.position);
        }
    }

    protected override void OnExecuteCancelled()
    {
        platformBehaviour?.ResetPlatformTarget();
        transform.position = _start;
        _tween?.Pause();
    }
}
