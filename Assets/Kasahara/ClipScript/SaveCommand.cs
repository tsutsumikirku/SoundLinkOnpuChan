using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class SaveCommand : PlayerCommand
{
    [SerializeField,Label("アニメーションの長さを調整するオフセット")] private float _clipLengthOffset = -1;
    public override PlayerState[] States => new[] { PlayerState.Save };

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
        player.Animator.SetBool("Memo", true);

        await UniTask.WaitForEndOfFrame(token);

        var animClip = player.Animator.GetCurrentAnimatorClipInfo(0);
        while (animClip[0].clip.name != "MemoMemo")
        {
            await UniTask.WaitForEndOfFrame(token);
            animClip = player.Animator.GetCurrentAnimatorClipInfo(0);
        }
        PlayerSavePos.Instance.Save(player.transform.position);
        // Debug.Log(animClip[0].clip.length + _clipLengthOffset);
        // Debug.Log(animClip[0].clip.name);
        await UniTask.WaitForSeconds(animClip[0].clip.length + _clipLengthOffset, cancellationToken: token);
        player.Animator.SetBool("Memo", false);
        player.ChangeState();
    }
}