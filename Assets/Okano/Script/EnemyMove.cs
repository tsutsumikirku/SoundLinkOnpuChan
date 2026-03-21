using DG.Tweening;
using UnityEngine;

public class EnemyMove : EnemyBase
{
    [SerializeField] Transform _end;
    [SerializeField] float _time;
    [SerializeField] Ease _easing;
    Tween _tween;
    Animator _anim;

    private Vector2 _start;

    private void Start()
    {
        _start = transform.position;
        _anim = GetComponent<Animator>();
    }
    protected override void OnExecuteBegin()
    {
        _tween = transform.DOMove(_end.position, _time)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(_easing);

        _anim.Play("EnemyMove");
    }
    protected override void OnExecuteCancelled()
    {
        gameObject.SetActive(true);
        transform.position = _start;
        _tween.Pause();
        _anim.Play("EnemyIdle");
    }

    private void OnDrawGizmos()
    {
        if (_end != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, _end.position);
        }
    }
}
