#region

using UnityEngine;

#endregion

public class AttackCollider : CancellableComponentBase
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IDamageble component) &&
            !collision.TryGetComponent(out Player _))
        {
            component.HitDamage();
        }
    }

    public void AttackStart()
    {
        gameObject.SetActive(true);
    }

    public void AttackEnd()
    {
        gameObject.SetActive(false);
    }

    protected override void OnExecuteCancelled()
    {
        gameObject.SetActive(false);
    }
}