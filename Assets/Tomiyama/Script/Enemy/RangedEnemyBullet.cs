using System;
using DG.Tweening;
using UnityEngine;

public class RangedEnemyBullet : EnemyBase, GoalDetector.IGoalDetectable
{
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private PlatformBehaviour platformBehaviour;
    public RangedEnemyBehaviour ParentBehaviour { set; private get; }

    private void Start()
    {
        GoalDetector.Register(this);
#if UNITY_EDITOR
        if (platformBehaviour == null)
        {
            Debug.LogError($"{nameof(platformBehaviour)} が未割り当てです。");
        }
#endif
    }

    public void BeginMove(float moveDistancePerSecond, float duration, Vector2 direction)
    {
        var endPos = (Vector2)transform.position + direction * moveDistancePerSecond * duration;
        transform.DOMove(endPos, duration)
            .SetEase(Ease.Linear)
            .OnComplete(StopMove);
    }

    protected override void OnExecuteBegin()
    {
        // タイムライン開始時にこのクラスは存在し得ないので空のままで良い。
    }

    protected override void OnExecuteCancelled() => StopMove();

    public void OnGoal() => Destroy(gameObject);

    protected override void OnDestroy()
    {
        base.OnDestroy();
        GoalDetector.Unregister(this);
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);

        // Obstacleレイヤーのうち、球の生産元である砲台のみヒット判定から除外する
        if (!collision.gameObject.IsInLayerMask(obstacleLayer)) return;
        if (collision.gameObject.TryGetComponent<RangedEnemyBehaviour>(out var behaviour) &&
            behaviour == ParentBehaviour) return;

        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        // 障害物に衝突したら動作を停止
        StopMove();
    }

    private void StopMove()
    {
        CriSEManager.Instance.PlaySE("GimmickSE", 2, 1, 0, true);
        // 足場の親子付けを解除してから自身を破壊。
        platformBehaviour.ResetPlatformTarget();
        DOTween.Kill(transform);
        Destroy(gameObject);
    }
}