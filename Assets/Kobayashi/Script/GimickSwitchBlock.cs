#region

using UnityEngine;

#endregion

[RequireComponent(typeof(BoxCollider2D), typeof(SpriteRenderer))]
public class GimmickSwitchBlock : SwitchGimmickBase
{
    [SerializeField] [Tooltip("gimmick発動時のSprite")]
    private Sprite _activeSprite;

    [SerializeField] [Tooltip("gimmick未発動時のSprite")]
    private Sprite _inactiveSprite;

    [SerializeField] [Tooltip("初期のActive状態")] private bool _isDefaultActivate;
    private BoxCollider2D _boxCollider;
    private SpriteRenderer _renderer;

    private void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _boxCollider = GetComponent<BoxCollider2D>();
        Inactive();
    }

    public override void Activate()
    {
        _renderer.sprite = _isDefaultActivate ? _inactiveSprite : _activeSprite;
        _boxCollider.enabled = !_isDefaultActivate;
    }

    public override void Inactive()
    {
        _renderer.sprite = _isDefaultActivate ? _activeSprite : _inactiveSprite;
        _boxCollider.enabled = _isDefaultActivate;
    }
}

public abstract class SwitchGimmickBase : MonoBehaviour, ISwitchGimmick
{
    public abstract void Activate();
    public abstract void Inactive();
}

public interface ISwitchGimmick
{
    void Activate();
    void Inactive();
}