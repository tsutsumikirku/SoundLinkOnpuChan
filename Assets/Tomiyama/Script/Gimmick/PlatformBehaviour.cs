using UnityEngine;

public class PlatformBehaviour : MonoBehaviour
{
    [SerializeField] [Header("Playerレイヤーを指定")]
    private LayerMask playerLayer;

    private Transform _onPlatformTarget;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.IsInLayerMask(playerLayer)) return;
        if (!collision.gameObject.activeSelf) return;
        if (collision.contacts.Length > 0)
        {
            if (collision.contacts[0].normal != -(Vector2)transform.up)
            {
                return;
            }
        }
        // プレイヤーを記憶して足場と親子関係にする
        _onPlatformTarget = collision.transform;
        _onPlatformTarget.SetParent(transform);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!collision.gameObject.IsInLayerMask(playerLayer)) return;
        if (!collision.gameObject.activeSelf) return;

        ResetPlatformTarget();
    }

    public void ResetPlatformTarget()
    {
        if (_onPlatformTarget != null)
        {
            // 親子付けを解除する。
            _onPlatformTarget.SetParent(null);
        }
    }
}

public static class LayerCheckExtensions
{
    /// <summary>
    /// GameObjectのレイヤーがLayerMaskに含まれるかどうか
    /// </summary>
    /// <param name="obj">比較元のGameObject</param>
    /// <param name="mask">フィルター</param>
    /// <returns>フィルターを通過したか</returns>
    public static bool IsInLayerMask(this GameObject obj, LayerMask mask) => (1 << obj.layer & mask.value) != 0;
}