using DG.Tweening;
using UnityEngine;

public class EnemyMoveJump : EnemyBase
{
    [SerializeField] Ease _easing;
    [SerializeField] Transform _end;
    [SerializeField] float _time;
    [Header("ジャンプ力")]
    [SerializeField] float _power;
    [Header("何回ジャンプするか")]
    [SerializeField] int _jump;
    Tween _tween;

    private Vector2 _start;
    private void Start()
    {
        _start = transform.position;
    }
    protected override void OnExecuteBegin()
    {
        _tween = transform.DOJump(_end.position, _power, _jump, _time)
            .SetLoops(-1, LoopType.Yoyo).SetEase(_easing);
    }

    protected override void OnExecuteCancelled()
    {
        gameObject.SetActive(true);
        transform.position = _start;
        _tween.Pause();
    }

    private void OnDrawGizmos()
    {
        if (_end != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, _end.position);
        }
    }
}
