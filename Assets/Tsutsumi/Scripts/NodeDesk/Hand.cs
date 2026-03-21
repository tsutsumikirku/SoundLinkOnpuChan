using UnityEngine;
using DG.Tweening;
using System;

public class Hand : MonoBehaviour,ITable
{
    //
    [SerializeField, Tooltip("ノードの間隔をこれで設定することができます")] float _nodeSpace;
    float _defaultWidth;
    public void AddNode(NodeMove node)
    {
        if (node == null)
        {
            Debug.LogError("ノードがnullです。");
            return;
        }
        node.transform.SetParent(transform); // ノードを自身の子オブジェクトに設定
        DeskUpdate(); // デスクを更新
    }
    /// <summary>
    /// デスクを更新します。
    /// このメソッドは、ノードの位置を更新し、デスクのサイズを調整します。
    /// </summary>  
    public void DeskUpdate()
    {
        float location = 0f; // ノードのxのロケーション
        var nodeBases = GetComponentsInChildren<NodeMove>(); // ノードのリストを取得
        var selfRectTransform = GetComponent<RectTransform>(); // 自身のRectTransformを取得
        var nodes = Array.ConvertAll(nodeBases, x => x.GetComponent<RectTransform>());
        nodes = Array.FindAll(nodes, node => node != selfRectTransform); // 自身のRectTransformを除外
        foreach (var node in nodes) // ノードのリストを実行
        {
            location += _nodeSpace; // ノードの間隔を設定
            location += node.rect.width; // ノードの幅を設定
            node.GetComponent<NodeMove>().Tween = node.DOAnchorPos(new Vector2(location, 0), 0.5f);
            node.SetAsLastSibling(); // ノードを最後尾に設定
        }
        if (nodes.Length < 1) return;
        ((RectTransform)transform).sizeDelta = new Vector2((location + nodes[nodes.Length - 1].rect.width) > _defaultWidth ? location + nodes[nodes.Length - 1].rect.width : _defaultWidth, ((RectTransform)transform).sizeDelta.y); // 自身のRectTransformのサイズを設定
        transform.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(location + nodes[nodes.Length - 1].rect.width > _defaultWidth ? location + nodes[nodes.Length - 1].rect.width : _defaultWidth, ((RectTransform)transform).sizeDelta.y);
    }
}
