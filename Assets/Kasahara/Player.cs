using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cinemachine;
using Live2D.Cubism.Core;
using Live2D.Cubism.Framework;
using Live2D.Cubism.Rendering;
using UnityEngine;
using UnityEngine.Serialization;


public class Player : CancellableComponentBase, IDamageble, IPause //, TimeScaleManager.IChangeTimeScale
{
    [SerializeField] PlayerCommand[] _commandList;
    [SerializeField] CinemachineImpulseSource _impulseSource;
    [SerializeField] public LayerMask GroundLayer;
    [SerializeField] public LayerMask ForwardCheckLayer;
    [SerializeField] public Transform _visualTransform;
    [SerializeField] private Animator _animator;
    
    [SerializeField,Header("後ろ向きのポジションオフセット")] private Vector3 _flipVisualObjectOffset;
    private Vector3 _defaultVisualObjectOffset;
    //初期状態の向き
    [Header("初期状態のPlayerの向き")]
    [SerializeField] private bool _defaultFlip;

    [SerializeField, Header("接地と判断する地面との距離")]
    private float _groundRaycastRange = 0.3f;
    [SerializeField, Header("ギミックなどの起動に関するRaycastの距離")]
    private float _gimmickRaycastRange = 0.3f;
    [SerializeField]
    private PlayerDeathEffect _deathEffectPrefab;

    [SerializeField] private string _deathAudioCueName = "SE_InGame_Onpu_Death";
    [SerializeField] private CubismModel _cubismModel;

    public event Action OnPlayerDeath;
    //コンポーネント
    private Rigidbody2D _rigidbody;
    private Collider2D _collider;

    private Vector3 _startPos;
    private IPlayerCommand _command;
    private List<INode> _nodes = new();
    private INode _currentNode;
    //コライダーの大きさを保存する
    private Vector2 _defaultColliderSize;
    private Vector2 _defaultColliderOffset;

    private int _nodeCount;
    private Transform _defaultParent;

    //ポーズ機能用の変数
    [HideInInspector] public bool _pauseFlag; //一時的にノードのwait処理用にpublicにしています。pausableWaitforなどがあればそっちに変更します。
    private Vector2 _saveVelocity;

    private bool _isFlip;
    private float _lastLastMoveSpeed;

    //playerの1フレーム前の位置を保持する
    private Vector3 _beforePosition;
    
    private bool _isPlaing;
    private bool _isDead;

    //自身が生成したObjectを記憶しておく
    private List<GameObject> _instanceObjects = new();

    CancellationTokenSource _cancelTokenSource = new();

    /// <summary> キャラクターの向きによって1,-1が帰ってきます。</summary>
    public int MoveDirection => _isFlip ? -1 : 1;

    public float LastMoveSpeed => _lastLastMoveSpeed;
    public Vector3 BeforePosition => _beforePosition;
    public Animator Animator => _animator;
    public Rigidbody2D Rigidbody => _rigidbody;
    public Collider2D Collider => _collider;
    public Vector2 DefaultColliderSize => _defaultColliderSize;
    public Vector2 DefaultColliderOffset => _defaultColliderOffset;
    public float GimmickRaycastRange => _gimmickRaycastRange;
    
    private Dictionary<MeshRenderer, bool> _meshRenderers = new();

    protected override void Awake()
    {
        base.Awake();
        InitializeData();
    }

    void InitializeData()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        _startPos = transform.position;
        _defaultColliderSize = _collider.bounds.size;
        _defaultColliderOffset = _collider.offset;
        _defaultVisualObjectOffset = _visualTransform.localPosition;
        var renderers = GetComponentsInChildren<MeshRenderer>();
        foreach (var mesh in renderers)
        {
            _meshRenderers.Add(mesh, mesh.enabled);
        }
        _defaultParent = transform.parent;
        //_timeScale = 1; //TimeScaleManager.InGameTimeScale;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        OnPlayerDeath = null;
    }

    private void Start()
    {
        FindAnyObjectByType<GoalBehaviour>().OnClear += () =>
        {
            ResetAllAnimatorParam(Animator);
            _nodes.Clear();
            _command = null;
            _currentNode = null;
        };
        
        ResetPlayerLive2d();
    }
    [Obsolete]
    public void ChangeState(PlayerState state, float input)
    {
        _command?.Exit(); //コマンドの終了処理
        _command = FindCommand(state);
        _command?.Init(this, token: _cancelTokenSource.Token);
    }

    //初期化用処理
    public void Init()
    {
        gameObject.SetActive(true);
        transform.position = _startPos;
        _beforePosition = _startPos;
        _rigidbody.linearVelocity = Vector2.zero;
        _cancelTokenSource = new CancellationTokenSource();
        _nodeCount = 0;
        _lastLastMoveSpeed = 0;
        _isDead = false;

        ChangeFlip(_defaultFlip);

        _currentNode?.EndNode();
        _currentNode = null;
        _command = null;

        if (_collider is BoxCollider2D box)
        {
            box.size = _defaultColliderSize;
            box.offset = _defaultColliderOffset;
        }
        
        foreach (var mesh in _meshRenderers)
        {
            if (mesh.Key != null)
                mesh.Key.enabled = mesh.Value;
        }
        ResetAllAnimatorParam(Animator);
        ResetPlayerLive2d();
    }
    //Animationのパラメータを初期化する
    void ResetAllAnimatorParam(Animator animator)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Trigger)
            {
                animator.ResetTrigger(param.name);
            }
            else if (param.type == AnimatorControllerParameterType.Bool)
            {
                animator.SetBool(param.name, false);
            }
        }

        _animator.Play("Idle");
        _animator.Update(0);
    }

    //再生終了時の処理
    protected override void OnExecuteCancelled()
    {
        _cancelTokenSource.Cancel();
        _cancelTokenSource.Dispose();
        _isPlaing = false;
        _command?.Exit();
        _command = null;
        foreach (var obj in _instanceObjects)
        {
            Destroy(obj);
        }

        //コンポーネントの初期化処理
        Init();
    }

    public void Play(List<INode> nodes)
    {
        _isPlaing = true;
        _nodes = nodes;
        ChangeState();
    }

    /// <summary>SpriteRenderer上でFlipを変更するためのメソッド </summary>
    /// <param name="flip">false:default  true:flipX</param>
    public void ChangeFlip(bool flip)
    {
        _visualTransform.localScale =
            new Vector3(flip ? MathF.Abs(_visualTransform.localScale.x) * -1 : MathF.Abs(_visualTransform.localScale.x),
                _visualTransform.localScale.y, _visualTransform.localScale.z);
        _visualTransform.localPosition = flip ? _flipVisualObjectOffset : _defaultVisualObjectOffset;
        _isFlip = flip;
    }

    #region CheckNode

    /// <summary>
    /// 指定のstateが最後に確認されたインデックスを取得する。
    /// </summary>
    /// <param name="state"></param>
    /// <param name="subState"></param>
    /// <returns></returns>
    public int FindNodeIndex(PlayerState state)
    {
        var temp = _nodes.GetRange(0, _nodeCount + 1).FindLastIndex(x => x.PlayerAction == state);
        return temp;
    }

    public INode GetNextNode()
    {
        //配列の範囲外かどうかの確認
        if (_nodes.Count <= _nodeCount || _nodeCount < 0)
        {
            return null;
        }

        return _nodes[_nodeCount];
    }

    #endregion

    public void ChangeState()
    {
        if (_isPlaing == false) return;
        _command?.Exit(); //コマンドの終了処理
        _currentNode?.EndNode();
        if (gameObject.activeSelf == false) return;

        var nextNode = GetNextNode();
        _nodeCount++;
        //Debug.Log(nextNode.PlayerAction);
        if (nextNode == null)
        {
            //次のノードが存在しない場合はPlayerを止める
            _command = null;
            return;
        }

        nextNode.StartNode();

        var tempCommand = FindCommand(nextNode.PlayerAction);

        if (tempCommand is null)
        {
            //Debug.Log("未設定のCommandを使用しようとしています。" + nextNode.PlayerAction);
            return;
        }

        _command = tempCommand;
        _currentNode = nextNode;


        //移動方向の変更処理　いい方法があったら変えます。
        // _moveDirection = nextNode.PlayerAction switch
        // {
        //     PlayerState.FowerdMove => 1,
        //     PlayerState.BackMove => -1,
        //     PlayerState.Stop => 0,
        //     _ => _moveDirection,
        // };
        _lastLastMoveSpeed = tempCommand.GetMoveSpeed(_lastLastMoveSpeed);

        //実行　これ移行にcountを進める処理がある場合、無限ループします。
        tempCommand.Init(this, nextNode, _cancelTokenSource.Token);
    }

    PlayerCommand FindCommand(PlayerState state)
    {
        var node = Array.Find(_commandList,
            command => command.States.Contains(state));
        return node;
    }

    private void FixedUpdate()
    {
        if (_pauseFlag != true)
        {
            _command?.CommandFixedUpdate();
        }

        _beforePosition = transform.position;
    }

    //TODO:HERE RayCastは前の方法で問題なさそうなので元に戻す

    #region RayCastCheck

    // 変更点：重複したオーバーロードを統合。
    public bool GroundCheck() => GroundCheck(out _);

    public bool GroundCheck(out RaycastHit2D hit)
    {
        // 変更点：重力の影響を考慮したRayCastを行うように修正
        var bounds = _collider.bounds;
        var gravitySign = Mathf.Sign(_rigidbody.gravityScale);
        var rayOrigin = bounds.center;
        var rayDir = Vector2.down * gravitySign;

        // 範囲は_groundRaycastRangeに限定
        hit = Physics2D.BoxCast(rayOrigin, bounds.size, 0, rayDir, _groundRaycastRange, GroundLayer);
        
        //Debug.Log("hitObject:" + hit.collider?.gameObject.name);
        return hit;
    }

    private void OnDrawGizmos()
    {
        var cl =GetComponent<BoxCollider2D>();
        var rb = GetComponent<Rigidbody2D>();

        var bounds = cl.bounds;
        var gravitySign = Mathf.Sign(rb.gravityScale);
        var rayOrigin = bounds.center;
        var rayDir = Vector2.down * gravitySign;

        // 実際の判定と同じBoxCast
        var hit = Physics2D.BoxCast(rayOrigin, bounds.size, 0, rayDir, _groundRaycastRange, GroundLayer);

        // ヒットしたら赤、してなければ黄
        Gizmos.color = hit ? Color.red : Color.yellow;
        Gizmos.DrawWireCube(rayOrigin, bounds.size);

        // キャスト範囲の終点位置
        var endPos = rayOrigin + (Vector3)(rayDir.normalized * _groundRaycastRange);
        Gizmos.DrawLine(rayOrigin, endPos);
        Gizmos.DrawWireCube(endPos, bounds.size);

        // ヒット地点
        if (hit)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(hit.point, Vector3.one * 0.05f);
        }
    }


    public RaycastHit2D RaycastCheck(Vector2 direction)
    {
        var h = Physics2D.BoxCast(_collider.bounds.center, _collider.bounds.size, 0,
            direction.normalized, _gimmickRaycastRange,
            ForwardCheckLayer);
        //Debug.Log("RaycastCheck:" + h.collider?.gameObject.name);
        return h;
    }

    /// <summary>
    /// 接地確認
    /// </summary>
    /// <param name="size">RayCastのBoxSize</param>
    /// <param name="hit"></param>
    /// <returns>hitかつ地面との距離が特定の距離以下だったらtrue</returns>
    //短めのrayを出して、hitしたら止まる方法だと誤差が出るため、長めにrayを飛ばしておきhitPosから逆算する。
    public bool GroundCheck(Vector3 size, out RaycastHit2D hit)
    {
        var bounds = _collider.bounds;
        hit = Physics2D.BoxCast(bounds.center + Vector3.up * bounds.size.y / 2, size, 0, Vector2.down, 1000,
            GroundLayer);
        if (hit && bounds.center.y - size.y / 2 - _groundRaycastRange <= hit.point.y)
        {
            return true;
        }

        return false;
    }

    #endregion


    void IDamageble.HitDamage()
    {
        if(_isDead) return;
        _isDead = true;
        var gravity = _rigidbody.gravityScale;
        //audio発火
        CriSEManager.Instance.PlaySE(_deathAudioCueName, playOneShot:false);
        //SoundManager.Instance.PlaySE(SESoundData.SE.GameOver);
        gameObject.SetActive(false);
        var deathEffect = Instantiate(_deathEffectPrefab);
        _instanceObjects.Add(deathEffect.gameObject);
        //Transformの初期化
        deathEffect.transform.position = _visualTransform.position;
        deathEffect.transform.localScale = _visualTransform.localScale;
        deathEffect.transform.rotation = _visualTransform.rotation;
        //死亡アニメーションの発火
        deathEffect.Init(MoveDirection, (int)Mathf.Sign(gravity));

        transform.parent = _defaultParent;
        //カメラシェイク発火
        _impulseSource?.GenerateImpulse();
        //死亡時のイベント発火
        OnPlayerDeath?.Invoke();
        //ResetPlayerLive2d();
    }

    [ContextMenu("Reset Player Live2d")]
    public void ResetPlayerLive2d()
    {
        foreach (var cubismModelParameter in _cubismModel.Parameters)
        {
            cubismModelParameter.Value = cubismModelParameter.DefaultValue;
        }
        _cubismModel.ForceUpdateNow();
    }
    
    [ContextMenu("Player pause")]
    public void Pause()
    {
        _animator.speed = 0;
        _saveVelocity = _rigidbody.linearVelocity;
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.isKinematic = true;
        _pauseFlag = true;
    }

    [ContextMenu("Player resume")]
    public void Resume()
    {
        _animator.speed = 1;
        _rigidbody.isKinematic = false;
        _rigidbody.linearVelocity = _saveVelocity;
        _pauseFlag = false;
    }
}

public abstract class PlayerCommand : MonoBehaviour, IPlayerCommand
{
    private void Start()
    {
        Start_S();
    }

    protected virtual void Start_S()
    {
    }

    /// <summary>
    /// 移動方向を設定するメソッド
    /// ジャンプの際に使用する予定
    /// </summary>
    /// <param name="n"></param>
    public virtual float GetMoveSpeed(float n = 0) => n;

    public abstract PlayerState[] States { get; }

    public abstract UniTask Init(Player player, INode node = null, CancellationToken token = default,
        Action action = null);

    public abstract void CommandFixedUpdate();
    public abstract UniTask Exit();
}

public interface IPlayerCommand
{
    public PlayerState[] States { get; }

    UniTask Init(Player player, INode node = default, CancellationToken token = default,
        Action action = null);

    public float GetMoveSpeed(float n);
    void CommandFixedUpdate();
    public UniTask Exit(); //TODO:HERE　引数入れてフィールド変数減らした方がよさそう
}

public interface IDamageble
{
    void HitDamage();
}


public enum PlayerState
{
    Stop,
    FowerdMove,
    BackMove,
    Jump,
    Attack,
    SwitchVisibility,
    Rolling,
    Save,
    Load,
}