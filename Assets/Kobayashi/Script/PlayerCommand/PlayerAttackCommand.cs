using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

//子要素にAttackColliderを設置し、それを攻撃判定として扱う
public class PlayerAttackCommand : PlayerCommand
{
    AttackCollider _attackObject;
    public override PlayerState[] States => new[] { PlayerState.Attack };
    protected override void Start_S()
    {
        _attackObject = transform.GetComponentInChildren<AttackCollider>(true);
        if (_attackObject == null)
        {
            Debug.Log("Attack用のコライダーが見つかりません");
            return;
        }

        _attackObject.gameObject.SetActive(false);
    }

    public override async UniTask Init(Player player, INode node = null, CancellationToken token = default,
        Action action = null)
    {
        player.Animator.SetTrigger("Attack");
        await UniTask.WaitForEndOfFrame(token);
        var animClip = player.Animator.GetCurrentAnimatorClipInfo(0)[0].clip;
        await UniTask.WaitForSeconds(animClip.length, cancellationToken: token);

        player.ChangeState();
    }

    public override void CommandFixedUpdate()
    {
        
    }

    public override UniTask Exit()
    {
        return UniTask.CompletedTask;
    }

    public void AttackStart()
    {
        _attackObject?.AttackStart();
    }

    public void AttackEnd()
    {
        _attackObject?.AttackEnd();
    }
}