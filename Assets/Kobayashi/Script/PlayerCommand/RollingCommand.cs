using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Serialization;

public class RollingCommand : PlayerCommand
{
    // [SerializeField] float _rollingMoveDistanceX = 2;
    // [SerializeField,Tooltip("0を入れないようにしてください")] float _rollingMoveTime = .5f;
    // [SerializeField] float _waitTime = 0.2f;
    //[SerializeField] float _moveSpeed;
    [Header("ローリング1の時の移動距離と時間")]
    [SerializeField,Tooltip("移動速度遷移が常に1の時の移動距離")] private float _rollingMoveDistanceX;
    [SerializeField] private float _rollingMoveTime;
    [Header("ローリングの移動速度遷移設定")]
    [SerializeField] private AnimationCurve _rollingCurve;
    [SerializeField] private float _waitTime = 0f;

    [Space, Header("変更後のColliderのサイズ")] [SerializeField]
    Vector2 _colliderSize;

    [SerializeField] Vector2 _colliderOffset;
    
    private Player _player;
    private Rigidbody2D _rb;

    private Vector2 _saveColliderSize;
    private Vector2 _saveColliderOffset;
    private Vector2 _saveColliderCenter;
    private float _direction;
    private bool _isHeadBlocked;
    public override PlayerState[] States => new[] { PlayerState.Rolling };
    //public float MovePower => _rollingMoveTime == 0 ? 0 : _rollingMoveDistanceX / _rollingMoveTime;

    //public override float GetMoveSpeed(float n = 0) => _rollingMoveDistanceX / _rollingMoveTime;
    public override float GetMoveSpeed(float n = 0) => _rollingMoveDistanceX / _rollingMoveTime;

    public override async UniTask Init(Player player, INode node = null, CancellationToken token = default,
        Action action = null)
    {
        if (node == null)
        {
            Debug.LogError("Error");
            return;
        }

        if (player.Collider is not BoxCollider2D box)
        {
            //boxの取得
            Debug.LogError("playerのBoxColliderが見つかりませんでした。");
            return;
        }

        _player = player;
        _rb = _player.Rigidbody;

        await UniTask.WaitUntil(() => _player.GroundCheck()||_player.GetNextNode().PlayerAction == PlayerState.Rolling, 
            cancellationToken: token);

        ChangeBoxColliderSize(box, _colliderOffset, _colliderSize);
        //反転していた場合の処理
        _direction = player.MoveDirection;
        _player.ChangeFlip(_direction <= 0);

        InitAnimation();
        //ローリングの発火
        for (var i = (int)node.Power; 0 < i; i--)
        {
            await UniTask.WaitUntil(() => _player.GroundCheck(), cancellationToken: token);
            await Rolling(token);
            if (i != 1)
            {
                await UniTask.WaitForSeconds(_waitTime, cancellationToken: token);
            }
        }
    
        await UniTask.WaitUntil(() => _player.GroundCheck(), cancellationToken: token);
        //念のためFixedフレームで切り替えを行う
        //頭が引っ掛かっている&次のコマンドがローリング以外ならここから抜け出せない
        await UniTask.WaitUntil(() => !_isHeadBlocked || _player.GetNextNode()?.PlayerAction == PlayerState.Rolling, cancellationToken: token);
        _player.ChangeState();
    }

    void ChangeBoxColliderSize(BoxCollider2D box, Vector2 offset, Vector2 size)
    {
        //もともとの状態を記録しておく
        _saveColliderSize = box.size;
        _saveColliderOffset = box.offset;

        box.size = size;
        //box.offset = new Vector2(box.offset.x, box.offset.y - _ColliderSize.y / 2); //あきらめの心、コード上でscaleに合わせてoffsetを変更したかったが上手く接地できなかった。
        box.offset = offset;
    }

    public override void CommandFixedUpdate()
    {
        _isHeadBlocked = CeilingCheck();
    }

    public override UniTask Exit()
    {
        var nextNode = _player.GetNextNode();
        if(nextNode?.PlayerAction != PlayerState.Rolling)
        {
            _player.Animator.SetBool("IsRolling", false);
            if (_player.Collider is BoxCollider2D box)
            {
                box.offset = _saveColliderOffset;
                box.size = _saveColliderSize;
            }
        }

        _rb.velocity = Vector2.zero;

        return UniTask.CompletedTask;
    }

    // async UniTask Rolling(float rollingPower, CancellationToken token = default)
    // {
    //     float timer = 0;
    //     var velocityX = _rollingMoveDistanceX / _rollingMoveTime;
    //     try
    //     {
    //         while (timer < _rollingMoveTime * rollingPower)
    //         {
    //             token.ThrowIfCancellationRequested();
    //             if (_player._pauseFlag) continue;
    //
    //
    //             _rb.velocity = new Vector2(velocityX * _direction, _rb.velocity.y);
    //
    //             timer += Time.deltaTime;
    //             await UniTask.WaitForFixedUpdate(cancellationToken: token);
    //         }
    //
    //         //設置するまでメソッドの終了を待機
    //         await UniTask.WaitUntil(() => _player.GroundCheck(), cancellationToken: token);
    //         _rb.velocity = new Vector2(0, _rb.velocity.y);
    //     }
    //     catch(Exception e)
    //     {
    //         Debug.LogError(e);
    //     }
    // }

    /// <summary>
    /// 1回ローリングをする
    /// </summary>
    /// <param name="token"></param>
    async UniTask Rolling(CancellationToken token = default)
    {
        _player.Animator.SetTrigger("Rolling");
        float timer = 0f;
        var velocityX = _rollingMoveDistanceX / _rollingMoveTime;
    
        try
        {
            while (timer < _rollingMoveTime)
            {
                token.ThrowIfCancellationRequested();
                
                float normalizedTime = timer / _rollingMoveTime;
                if (_player._pauseFlag) continue;
                var n = _rollingCurve.Evaluate(normalizedTime);
                //Debug.Log($"{normalizedTime}:{n}");
    
                _rb.velocity = new Vector2(velocityX * _direction * n, _rb.velocity.y);
    
                timer += Time.deltaTime;
                await UniTask.WaitForFixedUpdate(cancellationToken: token);
            }
    
            //接地するまでメソッドの終了を待機
            await UniTask.WaitUntil(() => _player.GroundCheck(), cancellationToken: token);
            _rb.velocity = new Vector2(0, _rb.velocity.y);
        }
        catch(OperationCanceledException)
        {
        }
    }
    void InitAnimation()
    {
        if (_rollingMoveTime != 0)
        {
            _player.Animator.SetFloat("RollingAnimSpeed", 1 / _rollingMoveTime);
        }
        else
        {
            Debug.LogWarning($"0除算が起きるためローリングAnimationの速度が1になりました。");
        }
        _player.Animator.SetBool("IsRolling", true);
    }

    RaycastHit2D CeilingCheck()
    {
        if (!_player) return default;
        var size = _saveColliderSize;
        var bounds = _player.Collider.bounds;
    
        return Physics2D.BoxCast(bounds.center, bounds.extents, 0, Vector2.up, size.y, _player.ForwardCheckLayer);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawWireCube((Vector2)transform.position + _colliderOffset, _colliderSize);
    }
}