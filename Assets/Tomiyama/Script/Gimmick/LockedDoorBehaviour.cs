using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 鍵付き扉の動作を管理するクラス。
/// </summary>
public class LockedDoorBehaviour : CancellableComponentBase
{
    [SerializeField] [Header("扉と鍵の対応パス")]private KeyPass _keyPass;
    
    [SerializeField] [Header("鍵の種類ごとのスプライト設定")]
    private KeySpritePair[] keySpritePairs;
    
    [SerializeField] [Header("SpriteRendererの参照")]
    private SpriteRenderer spriteRenderer;
    
    private KeyInventoryManager _keyInvMgr;
    private void OnValidate()
    {
#if UNITY_EDITOR
        // 遅延して処理させる
        EditorApplication.delayCall += UpdateKeySprite;
#endif
    }

    private void UpdateKeySprite()
    {
#if UNITY_EDITOR
        // 実行後に解除しないと、エディタを閉じるまで残り続ける
        EditorApplication.delayCall -= UpdateKeySprite;
#endif
        if (spriteRenderer == null) return;
        foreach (var pair in keySpritePairs)
        {
            if (pair.keyPass == _keyPass && pair.sprite != null)
            {
                spriteRenderer.sprite = pair.sprite;
                return;
            }
        }
        Debug.LogWarning("対応する鍵のスプライトが設定されていません。");
    }
    private void Start()
    {
        _keyInvMgr = FindAnyObjectByType<KeyInventoryManager>();
    }
    private void UnlockDoor()
    {
        CriSEManager.Instance.PlaySE("GimmickSE",4,1,0,true);
        // 開錠時のアニメーションがあればここに追加。
        gameObject.SetActive(false);
    }
    protected override void OnExecuteCancelled()
    {
        gameObject.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_keyInvMgr == null) return;
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;
        if (!_keyInvMgr.SearchAndUseKey(_keyPass)) return;
        
        UnlockDoor();
    }
}

    
[Serializable]
public struct KeySpritePair
{
    public KeyPass keyPass;
    public Sprite sprite;
}