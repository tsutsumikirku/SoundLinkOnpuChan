#region

using System.Collections;
using System.Linq;
using UnityEngine;

#endregion

[RequireComponent(typeof(BoxCollider2D))]
public class PushObjectBehaviour : CancellableComponentBase, IPlayerPush
{
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private LayerMask _wallLayer;
    [SerializeField] private float _gravity = -9.8f;
    [SerializeField] private bool _useGravity = true;
    [SerializeField] private float _raycastDistance = .2f;
    

    /// <summary>
    ///     Push中にかかる移動速度の比率
    /// </summary>
    [SerializeField] private float _moveRatio = 1;

    private BoxCollider2D _box;
    private Vector3 _defaultPosition;

    private float _timer;

    public bool UseGravity
    {
        get => _useGravity;
        set => _useGravity = value;
    }

    private void Start()
    {
        _box = GetComponent<BoxCollider2D>();
        _defaultPosition = transform.position;
        //FixedUpdateの後に実行したいため、コルーチンで実行する
        StartCoroutine(LateFixed());
        Init();
    }

    private IEnumerator LateFixed()
    {
        while (true)
        {
            yield return new WaitForFixedUpdate();
            Gravity();
        }
    }

    public float MoveRatio()
    {
        return _moveRatio;
    }
    
    /// <summary>
    /// オブジェクトを動かすことができるメソッド
    /// </summary>
    /// <param name="pushVecter">オブジェクトの移動量</param>
    bool IPlayerPush.TryPush(Vector2 pushVecter)
    {
        //下に足場がなければ落下を優先する
        if (!GroundCheck(out _))
        {
            //Debug.Log("Push Failed1");
            return false;
        }
        //前方にオブジェクトがあれば押せない
        if(TryRaycastAll(pushVecter, out var hits,_wallLayer))
        {
            
            //Debug.Log("Push Failed2"+$"{string.Join("\n", hits.Select(x=>x.transform.name))}");
            return false;
        }
        transform.position += (Vector3)pushVecter;
        return true;
    }

    private void Init()
    {
        transform.position = _defaultPosition;
        _timer = 0;
    }

    protected override void OnExecuteCancelled()
    {
        Init();
    }
    
    private void Gravity()
    {
        if (!_useGravity) return;

        //地面が存在する場合の処理
        if (GroundCheck(out var hits))
        {
            RaycastHit2D targetObj = hits.FirstOrDefault();
            //Debug.Log(targetObj.collider.name);
            //y座標を下のオブジェクトに合わせる
            if (targetObj)
            {
                var position = targetObj.point;
                position.x = transform.position.x;
                position.y += _box.bounds.size.y * 0.5f + _raycastDistance;
                transform.position = position;
                _timer = 0;
                return;
            }
        }
        //地面が存在しない場合の重力処理
        _timer += Time.deltaTime;
        var pos = new Vector3(0, _gravity * _timer * _timer / 2, 0);
        transform.position += pos;
    }

    bool GroundCheck(out RaycastHit2D[] hits)
    {
        hits = null;
        if (!TryRaycastAll(Vector2.down, out var allHits,_groundLayer)) return false;
        //自身より上のオブジェクトにあたらないようにする処理
        hits = allHits.Where(x => x.point.y < _box.bounds.center.y).ToArray();
        return hits.Length > 0;
    }

    /// <summary>
    /// BoxRaycastAllのUtility
    /// </summary>
    bool TryRaycastAll(Vector2 direction, out RaycastHit2D[] hits, LayerMask layerMask)
    {
        hits = null;
        if(_box == null) return false;
        
        var bounds = _box.bounds;
        hits = Physics2D.BoxCastAll(bounds.center, bounds.size, 0, direction,
            _raycastDistance, layerMask);
        // foreach (RaycastHit2D item in hits)
        // {
        //     Debug.Log($"transform.name:{item.transform.name} collider.transform.name:{item.collider.transform.name}");
        // }
        // Debug.Log("~~~~~~~~~");
        //自分にraycastが当たった場合の対策処理
        hits = hits.Where(x=>x.collider.transform != transform).ToArray();
        //Debug.Log(hits.FirstOrDefault().point + ":" + bounds.center);
        
        return hits.Length > 0;
    }

    private void OnDrawGizmos()
    {
        if (_box == null) return;
        
        //AIに投げました
        var bounds = _box.bounds;
        var center = bounds.center;
        var size = bounds.size;

        // 開始位置の描画（現在位置）
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(center, size);

        // 移動後の位置の描画
        var endCenter = center + Vector3.down * _raycastDistance;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(endCenter, size);

        // 中心線
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(center, endCenter);
    }
}

public interface IPlayerPush
{
    /// <summary>
    ///     移動速度にかかる倍率
    /// </summary>
    float MoveRatio();

    bool TryPush(Vector2 movement);
}