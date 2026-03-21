using UnityEngine;

public class SpikeHit : MonoBehaviour
{
    [SerializeField] private LayerMask playerLayer;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TakeDamageToTarget(collision.transform);
    }

    private void TakeDamageToTarget(Transform collision)
    {
        if (!collision.gameObject.IsInLayerMask(playerLayer)) return;
        if (collision.gameObject.TryGetComponent<IDamageble>(out var component))
        {
            component.HitDamage();
        }
    }
}