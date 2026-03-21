using System;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ShellEnemyBehaviour : EnemyBase, GoalDetector.IGoalDetectable
{
    private static readonly int MoveX = Animator.StringToHash("MoveX");
    [SerializeField] [Label("通常時に終点まで移動する秒数")]
    private float moveAroundSecond;

    [SerializeField] [Label("飛んでいくときの速度")]
    private float moveSpeedWhenAttacked;

    [SerializeField] [Label("反転判定を行う許容角度")] [Range(0f, 1f)]
    private float dotValue = 0.5f;

    [SerializeField] [Label("通常時のイージング")]
    private Ease moveEasing;

    [SerializeField] [Label("終点の位置")]
    private Transform moveEnd;

    [SerializeField] [Label("ダメージ判定のコライダー")]
    private Transform damageHitBox;

    [SerializeField] [Label("足場状態のコライダー")]
    private Transform sleepHitBox;

    [SerializeField] [Label("スライド移動のコライダー")]
    private Transform slideHitBox;

    [SerializeField] [Label("壁の衝突判定を取るレイヤー")]
    private LayerMask wallLayer;

    [SerializeField] [Label("Animatorの参照を入れる")]
    private Animator animator;

    private StayType _stayType;
    private Vector2 _startPos;
    private Vector2 _moveDirection;
    private Rigidbody2D _rb;
    private Transform _playerTransform;

    private void Start()
    {
        GoalDetector.Register(this);
        SwitchType(StayType.Paused);

        // 初期化時の位置を記憶
        _startPos = transform.position;

        // 物理演算を一時無効化
        _rb = GetComponent<Rigidbody2D>();
        _rb.isKinematic = true;

        // プレイヤーの位置をとれるように参照を取得
        _playerTransform = FindAnyObjectByType<Player>().transform;

        LookAtToMoveEnd();
    }

    private void LookAtToMoveEnd()
    {
        // moveEndの方に向くようにする
        transform.localScale = new(
            Mathf.Sign(transform.position.x - moveEnd.position.x) * Math.Abs(transform.localScale.x),
            transform.localScale.y,
            transform.localScale.z);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 攻撃状態以外は無視
        if (_stayType != StayType.Attack)
            return;

        // 衝突点が存在しないケースは無視
        if (collision.contactCount == 0)
            return;

        // 衝突したオブジェクトが壁レイヤーに属していない場合は無視
        if (!collision.gameObject.IsInLayerMask(wallLayer))
            return;

        // 最初の接触点のみで判定
        var normal = collision.GetContact(0).normal;

        // 衝突した方向と移動方向が同じ符号で、側面に衝突した時のみ移動方向を反転する
        if (Math.Sign(transform.position.x - normal.x) == Math.Sign(_moveDirection.x))
        {
            if (Mathf.Abs(normal.y) < dotValue)
            {
                _moveDirection = -_moveDirection;
                animator.SetFloat(MoveX, _moveDirection.x);
                _rb.linearVelocity = _moveDirection * moveSpeedWhenAttacked;
            }
        }

        // 減速が生じた場合、移動方向から速度を復元
        if (!Mathf.Approximately(_rb.linearVelocity.sqrMagnitude, moveSpeedWhenAttacked * moveSpeedWhenAttacked))
        {
            _rb.linearVelocity = _moveDirection * moveSpeedWhenAttacked;
        }
    }


    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.TryGetComponent<IDamageble>(out var component)) return;

        // 敵同士で攻撃が当たるかどうかの分岐
        if (!AllowFriendlyFire && component is EnemyBase) return;


        if (component is ShellEnemyBehaviour shellEnemy)
        {
            shellEnemy.DamageWithoutTypeChange();
        }
        else
        {
            component.HitDamage();
        }
    }

    private void DamageWithoutTypeChange()
    {
        // 状態変化以外のダメージ処理をする
        base.HitDamage();
    }

    protected override void OnExecuteBegin()
    {
        SwitchType(StayType.Awake);

        // 移動の開始
        transform.DOMove(moveEnd.position, moveAroundSecond)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(moveEasing)
            .OnStepComplete(() =>
            {
                // 左右の向きを反転させる
                transform.localScale = new(
                    -transform.localScale.x,
                    transform.localScale.y,
                    transform.localScale.z);
            });
    }

    protected override void OnExecuteCancelled()
    {
        SwitchType(StayType.Paused);
        Stop();

        // 初期位置に戻す
        gameObject.SetActive(true);
        transform.position = _startPos;
        LookAtToMoveEnd();
    }

    public void OnGoal() => Stop();

    protected override void OnDestroy()
    {
        base.OnDestroy();
        GoalDetector.Unregister(this);
    }

    /// <summary>
    /// 速度をリセットして動きを止める
    /// </summary>
    private void Stop()
    {
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = 0;
        _rb.isKinematic = true;
        DOTween.Kill(transform);
    }

    private void OnDrawGizmos()
    {
        const float Radius = 3f;
        const int SegmentCount = 30;
        Gizmos.color = Color.cyan;

        var center = transform.position;

        // 内積値から角度（片側）を求める
        var halfAngleRad = Mathf.Acos(Mathf.Clamp(dotValue, 0f, 1f));
        var step = halfAngleRad * 2 / SegmentCount;

        // 中心方向の角度を求める
        var forward = Vector2.right;
        var forwardAngle = Mathf.Atan2(forward.y, forward.x);

        var firstEdge = center + new Vector3(
            Mathf.Cos(forwardAngle - halfAngleRad),
            Mathf.Sin(forwardAngle - halfAngleRad)
        ) * Radius;

        // 扇形の側面ライン
        Gizmos.DrawLine(center, firstEdge);

        var lastPoint = firstEdge;
        for (var i = 1; i <= SegmentCount; i++)
        {
            var angle = forwardAngle - halfAngleRad + step * i;
            var nextPoint = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * Radius;
            Gizmos.DrawLine(lastPoint, nextPoint);
            lastPoint = nextPoint;
        }

        // 反対側の側面ライン
        Gizmos.DrawLine(center, lastPoint);

        if (moveEnd == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, moveEnd.position);
    }

    public override void HitDamage()
    {
        if (_stayType != StayType.Sleep)
        {
            SwitchType(StayType.Sleep);
            Stop();
        }
        else
        {
            SwitchType(StayType.Attack);

            // Rigidbodyの固定を外す
            _rb.isKinematic = false;

            var dir = transform.position - _playerTransform.position;

            // xはMathf.Signで符号のみ取り、他は不要なので0にする。
            dir.x = Mathf.Sign(dir.x);
            dir.y = 0;
            dir.z = 0;

            _moveDirection = dir;
            animator.SetFloat(MoveX, _moveDirection.x);
            _rb.AddForce(_moveDirection * moveSpeedWhenAttacked, ForceMode2D.Impulse);
        }
    }

    /// <summary>
    /// 敵の状態と有効化するコライダーを切り替える
    /// </summary>
    /// <param name="stayType">使用するタイプ</param>
    private void SwitchType(StayType stayType)
    {
        _stayType = stayType;
        animator.SetTrigger(Enum.GetName(typeof(StayType), stayType));
        AllowFriendlyFire = false;
        damageHitBox.gameObject.SetActive(false);
        sleepHitBox.gameObject.SetActive(false);
        slideHitBox.gameObject.SetActive(false);

        switch (stayType)
        {
            case StayType.Awake:
                damageHitBox.gameObject.SetActive(true);
                break;
            case StayType.Sleep:
                sleepHitBox.gameObject.SetActive(true);
                break;
            case StayType.Attack:
                damageHitBox.gameObject.SetActive(true);
                slideHitBox.gameObject.SetActive(true);
                AllowFriendlyFire = true;
                break;
        }
    }

    private enum StayType
    {
        Paused,
        Awake,
        Sleep,
        Attack,
    }
}