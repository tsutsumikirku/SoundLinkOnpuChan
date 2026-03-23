#region

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

#endregion

public class PlayerForwardMoveCommand : PlayerCommand
{
    [SerializeField] private float _defaultMoveSpeed = 5;

    [SerializeField] private float checkDistance = 0.5f; // 前方チェック距離
    [SerializeField] private float stepHeight = 0.5f; // 最大登れる高さ

    [SerializeField] [Tooltip("段差を登る際のbuffer。引っ掛かった際の対応用")]
    private float _buffer; //段差を登る際のbuffer

    [SerializeField] [Header("音符ちゃんが登れる角度の制限")]
    private float _angleLimit = 50f;

    private Animator _animator;
    private float _moveSpeed;
    private INode _node;

    private Player _player;
    private Rigidbody2D _rb;

    private float _time;

    public override PlayerState[] States => new[]
    {
        PlayerState.FowerdMove,
        PlayerState.BackMove
    };

    private void OnDrawGizmosSelected()
    {
        if (_rb == null) _rb = GetComponent<Rigidbody2D>();
        var bounds = GetComponent<BoxCollider2D>().bounds;
        var directionX = Mathf.Sign(_moveSpeed != 0 ? _moveSpeed : transform.localScale.x);
        var directionY = -Mathf.Sign(_rb != null ? _rb.gravityScale : -1);

        var footOrigin = new Vector2(
            bounds.center.x + bounds.extents.x * directionX + checkDistance * directionX,
            bounds.center.y + bounds.extents.y * directionY + 0.05f * directionY
        );
        var upperOrigin = footOrigin + Vector2.up * stepHeight * directionY;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(upperOrigin, upperOrigin + Vector2.down * directionY * (stepHeight + 0.1f));
    }

    public override float GetMoveSpeed(float n = 0)
    {
        //Debug.Log(_defaultMoveSpeed);
        return _defaultMoveSpeed;
    }

    public override UniTask Init(Player player, INode node, CancellationToken token = default,
        Action action = null)
    {
        if (player == null) return UniTask.CompletedTask;
        if (node == null)
        {
            player.ChangeState();
            return UniTask.CompletedTask;
        }

        //Debug.Log(node.PlayerAction);
        player.ChangeFlip(node.PlayerAction != PlayerState.FowerdMove);

        _player = player;
        _rb = _player.Rigidbody;
        _animator = _player.Animator;

        _node = node;
        _moveSpeed = _defaultMoveSpeed * _player.MoveDirection;

        //���l�̃��Z�b�g
        _rb.linearVelocity = Vector3.zero;
        _time = 0;

        _animator.SetBool("Move", true);

        return UniTask.CompletedTask;
    }

    public override void CommandFixedUpdate()
    {
        if (_time < _node.Second)
        {
            var velocity = new Vector2(_moveSpeed, _rb.linearVelocity.y);
            var hit = _player.RaycastCheck(new Vector2(_moveSpeed, 0));
            if (hit && hit.transform.gameObject.TryGetComponent(out IPlayerPush push))
            {
                var delta = Vector3.zero;
                //1フレームの移動量を求める
                delta.x = velocity.x * Time.fixedDeltaTime;
                var isPush = push.TryPush(delta);
                //移動速度を変更する
                velocity *= isPush ? push.MoveRatio() : 0;
            }
            else if (hit) //前方に壁が存在する場合は移動しない(引っ掛かり防止のため)
            {
                var normal = hit.normal;
                var angle = Vector2.Angle(normal, Vector2.up　* Mathf.Sign(_rb.gravityScale));
                if (_angleLimit < angle) velocity.x = 0;
            }

            _rb.linearVelocity = velocity;

            //step処理
            TryCheckStepAndClimb();
        }

        _time += Time.deltaTime;
        //ノードの終了条件
        //TODO:HERE　いつかメソッドに切り分ける
        if (_time >= _node.Second && _player.GroundCheck(out _)) _player.ChangeState();
    }

    public override UniTask Exit()
    {
        _animator.SetBool("Move", false);
        _rb.linearVelocity = Vector3.zero;
        return UniTask.CompletedTask;
    }

    private void TryCheckStepAndClimb()
    {
        if (_player == null) return;

        //設置していない場合はstep処理を走らせない
        var isGrouded = _player.GroundCheck(out var hitGround);
        if (!isGrouded) return;

        var bounds = _player.Collider.bounds;
        var directionX = Mathf.Sign(_moveSpeed);
        var directionY = Mathf.Sign(_rb.gravityScale);

        // 足元の原点
        var footOrigin = new Vector2(
            bounds.center.x + bounds.extents.x * directionX +
            (_player.GimmickRaycastRange + checkDistance) * directionX,
            bounds.center.y + bounds.extents.y * -directionY + 0.05f * directionY
        );

        // 段差の上の原点
        var upperOrigin = footOrigin + Vector2.up * (stepHeight * directionY);

        // Debug用Ray表示（青色）
        Debug.DrawRay(upperOrigin, Vector2.down * (directionY * (stepHeight + 0.1f)), Color.blue, 0.1f);

        // 段差の上から下向きにRaycast
        var hitStepHeight =
            Physics2D.Raycast(upperOrigin, Vector2.down * directionY, stepHeight + 0.1f, _player.GroundLayer);

        // 足元から下向きにRaycastして現在の地面を取得
        //Debug.Log($"{hitGround.collider.name} {hitStepHeight.collider.name}");
        // どちらも地面に当たっている場合のみ処理
        if (hitStepHeight.collider != null && hitGround.collider != null)
        {
            if (hitGround.collider == hitStepHeight.collider) return; //同一コライダーだった時は段差がないとみなす。

            var stepDiff = (hitStepHeight.point.y - hitGround.point.y) * directionY;
            if (stepDiff > 0.01f && stepDiff <= stepHeight)
            {
                var position = _rb.position;
                position.y += (stepDiff + _buffer) * directionY;

                _rb.MovePosition(position);
            }
        }
    }
}