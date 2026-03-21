using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class LoadCommand : PlayerCommand
{
    public override PlayerState[] States => new[] { PlayerState.Load };

    public override void CommandFixedUpdate()
    {
        //実装する必要なし
    }

    public override UniTask Exit()
    {
        return UniTask.CompletedTask;
    }

    public override async UniTask Init(Player player, INode node = null, CancellationToken token = default,
        Action action = null)
    {
        player.Animator.SetTrigger("LoadStart");
        await UniTask.WaitForEndOfFrame(token);
        //ワープアニメーション開始
        var animClip = player.Animator.GetCurrentAnimatorClipInfo(0);
        await UniTask.WaitForSeconds(animClip[0].clip.length, cancellationToken: token);
        //ここでおんぷちゃんが消えるので座標をセーブポイントに移動
        player.transform.position = PlayerSavePos.Instance.GetSavePos();
        
        //着地アニメーション開始
        player.Animator.SetTrigger("LoadEnd");
        await UniTask.WaitForEndOfFrame(token);
        animClip = player.Animator.GetCurrentAnimatorClipInfo(0);
        await UniTask.WaitForSeconds(animClip[0].clip.length, cancellationToken: token);
        player.Animator.SetTrigger("LoadEndToIdle");
        player.ChangeState();
    }
}