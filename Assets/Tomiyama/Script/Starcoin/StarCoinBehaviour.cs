using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class StarCoinBehaviour : MonoBehaviour
{
    [SerializeField] [Label("Playerレイヤーを指定")]
    private LayerMask playerLayer;
    
    public event Action OnCollected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.IsInLayerMask(playerLayer))
        {
            OnCollected?.Invoke();
        }
    }
}