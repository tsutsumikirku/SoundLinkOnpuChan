using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class PlayerJumpCommand : PlayerCommand
{
    [Header("前回のコマンドによる移動速度に依存するかどうか")]
    [SerializeField] private bool _useLastMoveSpeedAsJumpPower;
    [Space, Header("上の変数がfalseの時のジャンプパワー")]
    [SerializeField] float _defaultJumpPower = 10;

    [SerializeField, Tooltip("1パワーのジャンプ最高到達点　2パワー以降は加算")]
    private float _jumpHeight = 1;

    [SerializeField] float _jumpInterval = 0.25f;
    [SerializeField] float _stopToJumpSpeedX = .2f;
    [SerializeField, Tooltip("X   ̑  x")] float _speed;
    [SerializeField] private float _landingWaitTime = 0.15f;
    Player _player;
    INode _node;

    private CancellationToken _ct;
    Rigidbody2D _rb;
    Animator _animator;
    float _moveSpeed;

    bool _isJumping;
    [SerializeField]float _colliderBuffar = 0.1f;


    //デバッグ用
    [Obsolete] Action _endAction;

    private string _log;

    public override PlayerState[] States => new[] { PlayerState.Jump };

    protected override void Start_S()
    {
    }

    public override async UniTask Init(Player player, INode node = null, CancellationToken token = default,
        Action action = null)
    {
        if (node == null || player == null)
        {
            _player.ChangeState();
            return;
        }

        _node = node;
        _player = player;
        _rb = _player.Rigidbody;
        _animator = _player.Animator;
        _isJumping = false;

        //前回の移動系ステートがどれかを判別する。
        if (_useLastMoveSpeedAsJumpPower)
        {
            _moveSpeed = player.LastMoveSpeed * player.MoveDirection;
        }
        else
        {
            _moveSpeed = _speed * (player.MoveDirection != 0 ? player.MoveDirection : _stopToJumpSpeedX);
        }

        _player.ChangeFlip(_moveSpeed < 0);

        if (_node.Power == 0)
        {
            _player.ChangeState();
            action?.Invoke();
            return;
        }

       
        //接地を待つ
        await UniTask.WaitUntil(() => _player.GroundCheck(out var _), cancellationToken: token);

        _ct = token;

        JumpStart(token).Forget();
    }

    public override void CommandFixedUpdate()
    {
        _animator?.SetFloat("JumpUpVelocity", _rb.velocity.y);
        if (!_isJumping) return;
        
        var gravityDirection = Mathf.Sign(_rb.gravityScale);
        // 変更点：重力の方向を考慮した計算式に修正
        //落下確認
        if (_rb.velocity.y * gravityDirection <= 0 && _player.GroundCheck())
        {
            _isJumping = false;
            JumpEnd(_ct).Forget();
            return;
        }

        //コライダーが分かれている壁と設置するときにコライダーが引っ掛かる問題が再発したため、前方チェック追加
        //引っ掛かり解消のため、斜め下にBoxCastをするように変更しました。
        Vector2 velocity = new Vector2(_moveSpeed, _rb.velocity.y);
        var bounds = _player.Collider.bounds;
        var hit = Physics2D.BoxCast((Vector2)bounds.center - new Vector2(0, _colliderBuffar), (Vector2)bounds.size + new Vector2
            (0,_colliderBuffar * 2 * gravityDirection), 0, new Vector2(_moveSpeed, 0), _player.GimmickRaycastRange, _player.ForwardCheckLayer);
        if (hit)
        {
            if (hit.transform.gameObject.TryGetComponent(out IPlayerPush playerPush))
            {
                var velocityX = velocity.x * playerPush.MoveRatio();
                var delta = velocityX * Time.fixedDeltaTime;
                //boxが押せなかった際は、velocityを0にする
                velocity.x = playerPush.TryPush(new Vector2(delta, 0))? velocityX : 0;
            }
            else
            {
                velocity = new Vector2(0, _rb.velocity.y);
            }
        }

        _rb.velocity = velocity;
    }

    public override UniTask Exit()
    {
        //コライダーの初期化とVelocityのリセットを行う
        if (_player.Collider is BoxCollider2D box)
        {
            box.size = _player.DefaultColliderSize;
        }

        _rb.velocity = new Vector2(0, _rb.velocity.y);
        return UniTask.CompletedTask;
    }

    private void OnDrawGizmos()
    {
        if (_player == null) return;
        if(_rb == null) return;
        
        var gravityDirection = Mathf.Sign(_rb.gravityScale);
        var bounds = _player.Collider.bounds;
        var velocity = new Vector2(_moveSpeed, 0);
        var gimmickRaycastRange = _player.GimmickRaycastRange;
        var buffar =_colliderBuffar;
        // BoxCastの開始位置
        Vector2 origin = (Vector2)bounds.center + Vector2.down * (gravityDirection * buffar);
        // BoxCastのサイズ
        Vector2 size = (Vector2)bounds.size + Vector2.up * buffar * 2;
        // 角度（今回は0）
        float angle = 0f;

        // Gizmo色
        Gizmos.color = Color.green;
        // 開始位置のボックスを描画
        Gizmos.DrawWireCube(origin, size);

        // 移動後の中心位置
        Vector2 end = origin + velocity.normalized * gimmickRaycastRange;

        // 移動先のボックスを描画
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(end, size);

        // 開始位置から終了位置への線
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(origin, end);
    }

    async UniTask JumpStart(CancellationToken token = default)
    {
        _animator.SetBool("Jump", true);
        //ジャンプ時の最高到達点を求める
        float gravity = Mathf.Abs(_rb.gravityScale * Physics2D.gravity.y);
        
        // 変更点：重力の方向を考慮した計算式に修正
        float velocity = Mathf.Sign(_rb.gravityScale) * Mathf.Sqrt(2 * gravity * _jumpHeight * _node.Power);

        await UniTask.WaitForSeconds(_jumpInterval, cancellationToken: token);
        await UniTask.WaitUntil(() => !_player._pauseFlag, cancellationToken: token); //TODO:HERE

        _rb.velocity = Vector2.zero;
        _rb.AddForce(new Vector2(0, velocity), ForceMode2D.Impulse);

        _isJumping = true;
    }

    async UniTask JumpEnd(CancellationToken token = default)
    {
        _animator.SetBool("Jump", false);
        _rb.velocity = Vector2.zero;
        await UniTask.WaitForSeconds(_landingWaitTime, cancellationToken: token);
        await UniTask.WaitUntil(() => !_player._pauseFlag); //TODO:HERE

        _player.ChangeState();
    }
}