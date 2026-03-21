using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class WarpPortalSystem : MonoBehaviour
{
    [SerializeField] [Label("Playerレイヤーを指定")]
    private LayerMask playerLayer;
    
    [SerializeField] [Label("転送先のワープポータル")]
    private WarpPortalSystem targetPortalSystem;

    [SerializeField] [Label("侵入時のエフェクト")]
    private ParticleSystem enterEffect;

    private bool IsPortalActive { get; set; } = true;

    private void Awake()
    {
        // 転送先が正しく割り当てられているかの確認。
        if (targetPortalSystem == null)
        {
            Debug.LogWarning($"ワープポータルの転送先が未割当です: {gameObject.name}");
        }

        if (GetInstanceID() == targetPortalSystem.GetInstanceID())
        {
            Debug.LogWarning($"ワープポータルの転送先が自分自身になっています: {gameObject.name}");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.IsInLayerMask(playerLayer))
        {
            IsPortalActive = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.IsInLayerMask(playerLayer) || !IsPortalActive) return;
        
        // 転送直後に反対側のワープポータルが起動しないようにする。
        targetPortalSystem.IsPortalActive = false;
        Teleport(other.transform);
    }

    /// <summary>
    /// ターゲットをもう一方のワープポータルの位置に転送する。
    /// </summary>
    /// <param name="target">対象となるターゲット</param>
    private void Teleport(Transform target)
    {
        CriSEManager.Instance.PlaySE("GimmickSE",6,1,0,true);
        var pos = targetPortalSystem.transform.position;
        target.position = pos;
        if (enterEffect != null)
        {
            Instantiate(enterEffect, pos, Quaternion.identity);
        }
    }

    private void OnDrawGizmos()
    {
        if (targetPortalSystem == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, targetPortalSystem.transform.position);
    }
}