#region

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

#endregion

public class PlayerStopCommand : PlayerCommand
{
    private Player _player;
    private float _timer;
    private float _waitTime;
    public override PlayerState[] States => new[] { PlayerState.Stop };

    public override float GetMoveSpeed(float n = 0)
    {
        return 0;
    }

    public override UniTask Init(Player player, INode node, CancellationToken token = default,
        Action action = null)
    {
        _player = player;
        if (node == null || player == null)
            _waitTime = -1;
        else
            _waitTime = node.Second;
        _player.Animator.SetBool("Quietly", true);
        _timer = 0;
        return UniTask.CompletedTask;
    }

    public override void CommandFixedUpdate()
    {
        if (_waitTime < 0) return;
        if (_timer > _waitTime) _player?.ChangeState();

        _timer += Time.deltaTime;
    }

    public override UniTask Exit()
    {
        _player.Animator.SetBool("Quietly", false);
        return UniTask.CompletedTask;
    }
}