using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class GravitySwitchField : ExecutableCancellableBase
{
    [SerializeField] [Label("回転アニメーションの秒数")]
    private float playerRotateAnimationDuration = 0.5f;

    private readonly HashSet<Rigidbody2D> _insideOfFieldTargets = new();
    private bool _isFieldActive = true;
    private void OnTriggerExit2D(Collider2D other) => TrySwitchGravity(other);
    private void OnTriggerEnter2D(Collider2D other) => TrySwitchGravity(other);

    private void TrySwitchGravity(Collider2D other)
    {
        // OnTriggerの誤作動防止
        if (!_isFieldActive) return;
        if (!other.TryGetComponent(out Rigidbody2D rb)) return;
        if (rb.gravityScale == 0) return;
        if (rb.isKinematic) return;
        CriSEManager.Instance.PlaySE("GimmickSE",1,1,0,true);
        SwitchGravity(rb);
    }

    private void SwitchGravity(Rigidbody2D target)
    {
        // キャンセル時に重力を元に戻すため、既に重力を反転させているオブジェクトを保持
        var isTargetReversed = !_insideOfFieldTargets.Add(target);

        // 重力を反転させる
        target.gravityScale = -target.gravityScale;

        // プレイヤーの見た目部分をちょっと強引に取得
        var playerModel = target.transform.GetChild(0);

        if (playerModel != null)
        {
            // アニメーション部分
            playerModel.DOKill();
            
            // 通常時 -> 左右反転にする
            // 反転時 -> 上下反転にする（左右反転が二回かかり元に戻る）
            var rotateBegin = isTargetReversed ? Vector3.forward * 180f : Vector3.up * 180;
            playerModel.rotation = Quaternion.Euler(rotateBegin);
            
            // Z軸を中心に180度回転させる
            var rotateEnd = rotateBegin + Vector3.forward * 180f;
            var tween = playerModel.DORotate(rotateEnd, playerRotateAnimationDuration)
                .SetEase(Ease.InOutQuad);

            if (isTargetReversed)
            {
                // アニメーションがある場合、OnCompleteでリストから削除
                tween.OnComplete(() => _insideOfFieldTargets.Remove(target));
            }
        }
        else if (isTargetReversed)
        {
            // アニメーションがない場合の処理
            _insideOfFieldTargets.Remove(target);
        }
    }

    protected override void OnExecuteBegin() => _isFieldActive = true;

    protected override void OnExecuteCancelled()
    {
        _isFieldActive = false;

        // 重力、向きを元に戻す
        foreach (var player in _insideOfFieldTargets)
        {
            player.gravityScale = Mathf.Abs(player.gravityScale);
            var playerModel = player.transform.GetChild(0);
            if (playerModel != null)
            {
                playerModel.DOKill();
                playerModel.rotation = Quaternion.identity;
            }
        }

        _insideOfFieldTargets.Clear();
        _insideOfFieldTargets.TrimExcess();
    }
}