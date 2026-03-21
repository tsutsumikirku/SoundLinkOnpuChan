using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class RangedEnemyBehaviour : ExecutableCancellableBase, GoalDetector.IGoalDetectable
{
    [SerializeField] [Label("弾の発射間隔")]
    private float shootingIntervalSecond;

    [SerializeField] [Label("弾の発射速度")]
    private float projectileSpeed;

    [SerializeField] [Label("弾の寿命(秒)")]
    private float bulletLifeTime = 30;

    [SerializeField] [Label("弾のPrefab")]
    private RangedEnemyBullet bulletPrefab;

    [SerializeField] [Label("砲台の発射口")]
    private Transform cannonMuzzle;

    [SerializeField] [Label("砲台の向きを反転するか")]
    private bool isFlip;

    private CancellationTokenSource _cts;

    private void Start() => GoalDetector.Register(this);

    protected override void OnExecuteBegin()
    {
        // タイムライン開始時にトークン生成
        _cts = new();
        GenerateBulletAsync(_cts.Token).Forget();
    }

    private void OnValidate()
    {
        var v = transform.localScale;
        v.x = Mathf.Abs(v.x) * (isFlip ? -1 : 1);
        transform.localScale = v;
    }

    private async UniTask GenerateBulletAsync(CancellationToken token)
    {
        // トークンによってキャンセルされるまで弾を生成し続ける。
        while (!token.IsCancellationRequested)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(shootingIntervalSecond), cancellationToken: token);
            var bullet = Instantiate(bulletPrefab, cannonMuzzle.position, Quaternion.identity, transform.parent);
            CriSEManager.Instance.PlaySE("GimmickSE", 3, 1, 0, true);
            // isFlipがtrueなら左向き、falseなら右向きに方向とスケールを調整。
            var flipSign = isFlip ? -1 : 1;
            var v = bullet.transform.localScale;
            v.x = Mathf.Abs(v.x) * flipSign;
            bullet.transform.localScale = v;
            var dir = transform.right * flipSign;

            bullet.BeginMove(projectileSpeed, bulletLifeTime, dir);
            bullet.ParentBehaviour = this;
        }
    }

    protected override void OnExecuteCancelled() => StopGenerating();
    private void OnDisable() => StopGenerating();

    public void OnGoal() => StopGenerating();

    protected override void OnDestroy()
    {
        base.OnDestroy();
        GoalDetector.Unregister(this);
    }

    private void StopGenerating()
    {
        if (_cts == null) return;

        // トークンをキャンセルして初期化。
        _cts.Cancel();
        _cts.Dispose();
        _cts = null;
    }
}