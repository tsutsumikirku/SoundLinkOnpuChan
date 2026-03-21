#region

using System.Linq;
using UnityEngine;

#endregion

public class WeightTriggerSwitch : FieldSwitchBase
{
    [Header("当たり判定調整用パラメータ")] [SerializeField]
    private GameObject _colliderObject;

    [SerializeField] private Vector2 _offset = Vector2.zero;
    [SerializeField] private Vector2 _size = Vector2.zero;
    [SerializeField] private LayerMask _layerMask;

    [Space(2)] [Header("沈み込みの設定")] [SerializeField]
    private float _activationPressDepth;

    [SerializeField] private float _pushTime = 0.2f;
    private Vector2 _defaultPosition;
    private float _timer;


    private void Start()
    {
        _defaultPosition = _colliderObject.transform.position;
    }

    private void FixedUpdate()
    {
        if (Toggle) return;

        // if (CheckBox())
        // {
        //     OnActive();
        // }
        if (!CheckBox())
        {
            ;
            if (_timer <= 0) _timer = 0;
            else _timer -= Time.fixedDeltaTime;
        }
        else
        {
            _timer += Time.fixedDeltaTime;
        }

        SwitchAnimation(_timer / _pushTime);

        if (_timer >= _pushTime) OnActive();
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        var scaledSize = new Vector2(
            _size.x * Mathf.Abs(_colliderObject.transform.lossyScale.x),
            _size.y * Mathf.Abs(_colliderObject.transform.lossyScale.y)
        );
        var scaledOffset = _offset * new Vector2(scaledSize.x, scaledSize.y);
        if (Toggle) return;
        if (!_colliderObject) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube((Vector2)_colliderObject.transform.position + scaledOffset, scaledSize);
    }

    private void SwitchAnimation(float time)
    {
        if (_activationPressDepth == 0) return;
        //重量制御のスイッチの見た目部分
        time = Mathf.Clamp01(time);

        _colliderObject.transform.position = (Vector3)_defaultPosition - _colliderObject.transform.rotation *
            Vector3.Lerp(Vector3.zero,
                new Vector3(0, _activationPressDepth * Mathf.Abs(_colliderObject.transform.lossyScale.y)), time);
    }

    private bool CheckBox()
    {
        // オブジェクトのワールドスケールを反映させる
        var scaledSize = new Vector2(
            _size.x * Mathf.Abs(_colliderObject.transform.lossyScale.x),
            _size.y * Mathf.Abs(_colliderObject.transform.lossyScale.y)
        );
        var scaledOffset = _offset * new Vector2(scaledSize.x, scaledSize.y);
        var hits = Physics2D.OverlapBoxAll((Vector2)_colliderObject.transform.position + scaledOffset, scaledSize, 0f,
            _layerMask);
        return hits.Any(x => x.transform != transform);
    }

    protected override void ActiveEvent()
    {
        base.ActiveEvent();
    }

    protected override void InactiveEvent()
    {
        base.InactiveEvent();
        _timer = 0;
    }
}