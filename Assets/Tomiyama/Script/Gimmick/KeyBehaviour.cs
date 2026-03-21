using UnityEditor;
using UnityEngine;

/// <summary>
/// プレイヤーに追従する鍵の動作クラス。
/// </summary>
public class KeyBehaviour : CancellableComponentBase
{
    [SerializeField] [Header("扉と鍵の対応パス")]
    private KeyPass _keyPass;
    
    [SerializeField] [Header("鍵の種類ごとのスプライト設定")]
    private KeySpritePair[] keySpritePairs;
    
    [SerializeField] [Header("SpriteRendererの参照")]
    private SpriteRenderer spriteRenderer;
    
    public KeyPass Pass => _keyPass;
    public float FollowSpeed { private get; set; }
    public float FollowDistance { private get; set; }
    public Transform FollowTarget { get; set; }
    private Vector2 _initPos;
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
        if (_keyInvMgr == null)
        {
            Debug.LogWarning("KeyInventoryManagerが存在しません。鍵付き扉のシステムは正常に動作しません。");
        }
        _initPos = transform.position;
    }

    private void FixedUpdate()
    {
        if (!FollowTarget) return;
        if (FollowSpeed == 0)
        {
            transform.position = FollowTarget.position;
            return;
        }

        if ((FollowTarget.position - transform.position).magnitude < FollowDistance) return;
        var distPerFrame = (FollowTarget.position - transform.position) / FollowSpeed * Time.fixedDeltaTime;
        transform.position += distPerFrame;
    }

    protected override void OnExecuteCancelled()
    {
        FollowTarget = null;
        transform.position = _initPos;
        gameObject.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_keyInvMgr == null) return;
        if (FollowTarget != null) return;
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;
        CriSEManager.Instance.PlaySE("GimmickSE",5,1,0,true);
        _keyInvMgr.AddKey(this);
    }

    public void UseKey()
    {
        gameObject.SetActive(false);
    }
}