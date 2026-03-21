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
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        // タッチの時間とノードの状態を初期化する
        touchTime = 0;
        _nodeState = NodeState.None;
        _scaleTween?.Kill();
        _scaleTween = transform.DOScale(Vector3.one * 1.2f, 0.2f).SetEase(Ease.OutBack);
        //CursolChanger.Instance.CursorType = CursorType.Drag;
        if (_tween != null)
        {
            if (_tween.IsActive()) _tween.Kill();
        }
        if (transform.parent.TryGetComponent<Hand>(out var hand))
        {
            GameObject newnode = Instantiate(this.gameObject);
            newnode.transform.SetParent(this.transform.parent);
            newnode.transform.position = this.transform.position;
            var nodeMove = newnode.GetComponent<NodeMove>();
            nodeMove.NodeDataContainer.Init(NodeDataContainer.NodeData);
            newnode.transform.localScale = this.transform.localScale;
            nodeMove.SafeArea = this.SafeArea;
            nodeMove.Canvas = this.Canvas;
            transform.SetParent(SafeArea);
            transform.SetAsLastSibling();
        }
        if (transform.parent.TryGetComponent<TimeLine>(out var desk))
        {
            desk.RemoveNode();
            transform.SetParent(SafeArea);
            transform.SetAsLastSibling();
            desk.DeskUpdate();
        }
    }
    //ポインターがドラッグされているときのメソッド
    public void OnDrag(PointerEventData eventData)
    {
        ((RectTransform)transform).anchoredPosition = ((RectTransform)transform).anchoredPosition + eventData.delta / Canvas.scaleFactor;
    }
    //ポインターが離されたときのメソッド
    public void OnEndDrag(PointerEventData eventData)
    {
        _scaleTween = transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
        if (TryEventToTimeLine(eventData, out var deck))
        {
            deck.AddNode(this); // ドラッグがデッキにドロップされた場合は、デッキにノードを追加
            return; // ドラッグがデッキにドロップされた場合は何もしない
        }
        Destroy(this.gameObject); // ドラッグがデッキにドロップされなかった場合はノードを削除      
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
        }
        deck = null;
        return false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        CriSEManager.Instance.PlaySE(_nodeCatch);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
    }
}
public enum NodeState
{
    None,
    Select
}
