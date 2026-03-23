using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using UnityEngine;

public class PlayerBackMoveCommand : PlayerCommand
{
    [SerializeField] float _defaultMoveSpeed = 5;

    Player _player;
    INode _node;
    Rigidbody2D _rb;
    Animator _animator;

    float _time;
    float _moveSpeed;


    public override PlayerState[] States => new[] { PlayerState.BackMove };

    public override UniTask Init(Player player, INode node, CancellationToken token = default,
        Action action = null)
    {
        if (player == null) return UniTask.CompletedTask;
        if (node == null)
        {
            _player.ChangeState();
            return UniTask.CompletedTask;
        }

        _player = player;
        _rb = _player.Rigidbody;
        _animator = _player.Animator;

        _node = node;
        _moveSpeed = _defaultMoveSpeed * -1;

        //数値のリセット
        _rb.linearVelocity = Vector3.zero;
        _time = 0;

        _animator.SetBool("Move", true);

        _player.ChangeFlip(true);

        return UniTask.CompletedTask;
    }

    public override void CommandFixedUpdate()
    {
        // if(_isFowardObj) return;
        //TODO:HERE 前方にオブジェクトが存在したときの処理
        if (_time < _node.Second)
        {
            var velocity = new Vector2(_moveSpeed, _rb.linearVelocity.y);
            _rb.linearVelocity = velocity;
        }

        _time += Time.deltaTime;
        //Debug.Log($"{_time} {_node.Second} {_player.GroundCheck(out var h1it)}");
        if (_time >= _node.Second && _player.GroundCheck(out var hit))
        {
            _player.ChangeState();
        }
    }

    public override UniTask Exit()
    {
        _animator.SetBool("Move", false);
        _rb.linearVelocity = Vector3.zero;
        _player._visualTransform.localScale = new Vector3(false ? MathF.Abs(_player.transform.localScale.x) * -1: MathF.Abs(_player.transform.localScale.x), _player.transform.localScale.y, _player.transform.localScale.z);
        return UniTask.CompletedTask;
    }
}