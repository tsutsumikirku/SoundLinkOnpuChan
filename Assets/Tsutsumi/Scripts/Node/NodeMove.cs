using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public class NodeMove : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler,IPointerDownHandler,IPointerUpHandler
{
    [SerializeField, Header("ノードの大きさの倍率を設定してください")] float _scale = 1.2f;
    [SerializeField, Header("ノードの移動時間を設定してください、0番目につかんだ時、1番目に放した時")] float[] _moveTime = { 0.2f, 0.5f };
    [SerializeField, Header("ノードのイージングを設定してください")] Ease _ease = Ease.OutBack;
    [SerializeField, Header("ノードをつかんだ時のSEのタグを設定してください")] string _nodeCatch;
    [SerializeField, Header("長押し判定時間（秒）")] float _holdDuration = 0.3f;
    [SerializeField, Header("静止判定の移動しきい値（px）")] float _stillThreshold = 16f;
    [SerializeField, Header("親へ横ドラッグを渡すしきい値（px）")] float _horizontalDragThreshold = 12f;
    [SerializeField, Header("長押し再判定の復帰半径（px）")] float _rearmRadius = 24f;
    NodeState _nodeState = NodeState.None;
    public NodeDataContainer NodeDataContainer;
    //ノードが所属するCanvasを取得します。
    public Canvas Canvas;
    //ノードのTweenを管理します。
    Tween _tween;
    Tween _scaleTween;
    //ノードのTweenを取得または設定します。
    bool have = false;
    float touchTime = 0f;
    float _dragBeginTime = 0f;
    bool _movedDuringHold = false;
    bool _holdCanceled = false;
    float _pointerDownTime = 0f;
    Vector2 _pointerDownPosition = Vector2.zero;
    bool _isPointerDown = false;
    bool _isHoldVisualApplied = false;
    int _holdVisualRequestId = 0;
    bool _isNodeDragActive = false;
    bool _isMoveModeEntered = false;
    Vector3 _defaultScale = Vector3.one;
    Vector3 _touchStartScale = Vector3.one;
    Transform _dragStartParent;
    IBeginDragHandler _parentBeginDragHandler;
    IDragHandler _parentDragHandler;
    IEndDragHandler _parentEndDragHandler;
    bool _isParentDragStarted = false;
    public Tween Tween { get => _tween; set => _tween = value; }
    public Transform SafeArea;
    public async UniTask Init(ITable table)
    {
        SafeArea = FindAnyObjectByType<Canvas>().transform;
        await UniTask.DelayFrame(1); // フレーム待機してから処理を実行
        Canvas = GetComponentInParent<Canvas>();
        //ノードは生まれたときに、手札のテーブルを検索してその子オブジェクトになります
        table.AddNode(this);
    }
    void Awake()
    {
        SafeArea = FindAnyObjectByType<Canvas>().transform;
        Canvas = GetComponentInParent<Canvas>();
        _defaultScale = transform.localScale;
    }

    async UniTaskVoid StartHoldVisualTimer(int requestId)
    {
        int delayMs = Mathf.Max(0, Mathf.RoundToInt(_holdDuration * 1000f));
        await UniTask.Delay(delayMs, DelayType.UnscaledDeltaTime, PlayerLoopTiming.Update);

        if (requestId != _holdVisualRequestId)
        {
            return;
        }

        if (_isPointerDown && !_isNodeDragActive && !_movedDuringHold && !_holdCanceled && !_isHoldVisualApplied)
        {
            EnterMoveMode();
            ApplyHoldScaleVisual();
        }
    }

    void ApplyHoldScaleVisual()
    {
        _isHoldVisualApplied = true;
        _scaleTween?.Kill();
        _scaleTween = transform.DOScale(_touchStartScale * _scale, _moveTime[0]).SetEase(_ease);
    }

    void EnterMoveMode()
    {
        if (_isMoveModeEntered)
        {
            return;
        }

        _isMoveModeEntered = true;

        if (transform.parent.TryGetComponent<Hand>(out var hand))
        {
            GameObject newnode = Instantiate(this.gameObject);
                NewNodeSetUp(newnode).Forget();
        }
        if (transform.parent.TryGetComponent<TimeLine>(out var desk))
        {
            desk.RemoveNode();
            transform.SetParent(SafeArea);
            transform.SetAsLastSibling();
            desk.DeskUpdate();
        }
    }
    async UniTask NewNodeSetUp(GameObject newnode)
    {
            await UniTask.Yield();
            newnode.transform.SetParent(this.transform.parent, worldPositionStays: true);
            newnode.transform.localPosition = this.transform.localPosition;
            var nodeMove = newnode.GetComponent<NodeMove>();
            nodeMove.NodeDataContainer.Init(NodeDataContainer.NodeData);
            nodeMove._defaultScale = _touchStartScale;
            nodeMove._touchStartScale = _touchStartScale;
            nodeMove._isMoveModeEntered = false;
            newnode.transform.localScale = _touchStartScale;
            nodeMove.SafeArea = this.SafeArea;
            nodeMove.Canvas = this.Canvas;
            transform.SetParent(SafeArea, worldPositionStays: true);
            transform.SetAsLastSibling();
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        // タッチの時間とノードの状態を初期化する
        touchTime = 0;
        _dragBeginTime = _isPointerDown ? _pointerDownTime : Time.unscaledTime;
        _nodeState = NodeState.None;
        _isNodeDragActive = false;
        _dragStartParent = transform.parent;

        _parentBeginDragHandler = null;
        _parentDragHandler = null;
        _parentEndDragHandler = null;
        _isParentDragStarted = false;

        if (_dragStartParent != null)
        {
            _parentBeginDragHandler = _dragStartParent.GetComponent<IBeginDragHandler>();
            _parentDragHandler = _dragStartParent.GetComponent<IDragHandler>();
            _parentEndDragHandler = _dragStartParent.GetComponent<IEndDragHandler>();
        }

        //CursolChanger.Instance.CursorType = CursorType.Drag;
        if (_tween != null)
        {
            if (_tween.IsActive()) _tween.Kill();
        }
    }

    void StartNodeDrag(PointerEventData eventData)
    {
        _isNodeDragActive = true;

        if (_isParentDragStarted)
        {
            _parentEndDragHandler?.OnEndDrag(eventData);
            _isParentDragStarted = false;
        }

        _scaleTween?.Kill();
        _scaleTween = transform.DOScale(_touchStartScale * _scale, _moveTime[0]).SetEase(_ease);

        EnterMoveMode();
    }

    void ForwardDragToParent(PointerEventData eventData)
    {
        if (_parentDragHandler == null)
        {
            return;
        }

        if (!_isParentDragStarted)
        {
            _parentBeginDragHandler?.OnBeginDrag(eventData);
            _isParentDragStarted = true;
        }

        _parentDragHandler.OnDrag(eventData);
    }

    void RearmHoldFromCurrentPointer(PointerEventData eventData)
    {
        _movedDuringHold = false;
        _holdCanceled = false;
        _dragBeginTime = Time.unscaledTime;

        if (_isParentDragStarted)
        {
            _parentEndDragHandler?.OnEndDrag(eventData);
            _isParentDragStarted = false;
        }

        _scaleTween?.Kill();
        _scaleTween = transform.DOScale(_touchStartScale, 0.1f).SetEase(Ease.OutSine);

        _isHoldVisualApplied = false;
        _holdVisualRequestId++;
        StartHoldVisualTimer(_holdVisualRequestId).Forget();
    }

    //ポインターがドラッグされているときのメソッド
    public void OnDrag(PointerEventData eventData)
    {
        if (!_isNodeDragActive)
        {
            float elapsed = Time.unscaledTime - _dragBeginTime;
            Vector2 totalDelta = eventData.position - _pointerDownPosition;
            float horizontalThreshold = Mathf.Max(_horizontalDragThreshold, 8f);
            float stillThreshold = Mathf.Max(_stillThreshold, 10f);
            float rearmThreshold = Mathf.Max(_rearmRadius, stillThreshold * 0.8f);
            bool movedHorizontal = Mathf.Abs(totalDelta.x) > horizontalThreshold && Mathf.Abs(totalDelta.x) > Mathf.Abs(totalDelta.y);
            bool movedTooMuch = totalDelta.magnitude > stillThreshold;
            bool returnedNearStart = totalDelta.magnitude <= rearmThreshold;

            if ((_holdCanceled || _movedDuringHold) && returnedNearStart)
            {
                RearmHoldFromCurrentPointer(eventData);
                return;
            }

            if (elapsed < _holdDuration)
            {
                if (movedHorizontal || movedTooMuch)
                {
                    _movedDuringHold = true;
                    _holdCanceled = true;
                    if (movedHorizontal)
                    {
                        ForwardDragToParent(eventData);
                    }
                }
                return;
            }

            if (_holdCanceled || _movedDuringHold)
            {
                if (Mathf.Abs(eventData.delta.x) > horizontalThreshold)
                {
                    ForwardDragToParent(eventData);
                }
                return;
            }

            StartNodeDrag(eventData);
        }

        ((RectTransform)transform).anchoredPosition = ((RectTransform)transform).anchoredPosition + eventData.delta / Canvas.scaleFactor;
    }
    //ポインターが離されたときのメソッド
    public void OnEndDrag(PointerEventData eventData)
    {
        if (_isParentDragStarted)
        {
            _parentEndDragHandler?.OnEndDrag(eventData);
            _isParentDragStarted = false;
        }
    }
    /// <summary>
    /// TimeLineが取得できた場合trueを返します。
    /// </summary>
    /// <param name="eventData"></param>
    /// <returns></returns>
    bool TryEventToTimeLine(PointerEventData eventData, out TimeLine deck)
    {
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        foreach (var item in results)
        {
            if (item.gameObject.TryGetComponent<TimeLine>(out var desk))
            {
                deck = desk;
                return true;
            }

            var parentDesk = item.gameObject.GetComponentInParent<TimeLine>();
            if (parentDesk != null)
            {
                deck = parentDesk;
                return true;
            }
        }
        deck = null;
        return false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isPointerDown = true;
        _isHoldVisualApplied = false;
        _movedDuringHold = false;
        _holdCanceled = false;
        _isNodeDragActive = false;
        _isMoveModeEntered = false;
        _touchStartScale = transform.localScale;
        _holdVisualRequestId++;
        _pointerDownTime = Time.unscaledTime;
        _pointerDownPosition = eventData.position;
        StartHoldVisualTimer(_holdVisualRequestId).Forget();
        CriSEManager.Instance.PlaySE(_nodeCatch);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        bool shouldFinalizeDrop = _isNodeDragActive || _isMoveModeEntered;

        _isPointerDown = false;
        _holdVisualRequestId++;
        _isHoldVisualApplied = false;
        _movedDuringHold = false;
        _holdCanceled = false;
        _isMoveModeEntered = false;

        _scaleTween?.Kill();
        _scaleTween = transform.DOScale(_touchStartScale, 0.2f).SetEase(Ease.OutBack);

        if (shouldFinalizeDrop)
        {
            if (TryEventToTimeLine(eventData, out var deck))
            {
                deck.AddNode(this);
            }
            else
            {
                Destroy(this.gameObject);
            }

            _isNodeDragActive = false;
        }

        if (_isParentDragStarted)
        {
            _parentEndDragHandler?.OnEndDrag(eventData);
            _isParentDragStarted = false;
        }
    }
}
public enum NodeState
{
    None,
    Select
}
