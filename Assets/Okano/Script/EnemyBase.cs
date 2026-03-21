using UnityEngine;

public abstract class EnemyBase : ExecutableCancellableBase, IDamageble
{
    [SerializeField] protected GameObject deathEffect;
    protected bool AllowFriendlyFire;
    public virtual void HitDamage()
    {
        // すでに非アクティブ化されている場合は何もしない
        if (!gameObject.activeSelf) return;
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
        CriSEManager.Instance.PlaySE("GimmickSE",7,1,0,true);
        gameObject.SetActive(false);
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.TryGetComponent<IDamageble>(out var component)) return;
        
        // 敵同士で攻撃が当たるかどうかの分岐
        if (!AllowFriendlyFire && component is EnemyBase) return;
            
        component.HitDamage();
    }
}
