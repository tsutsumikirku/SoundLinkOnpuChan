using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TimeLine : MonoBehaviour, ITable
{
    [SerializeField, Header("タイムラインの進捗度を表示するイメージ")] RectTransform _progressImage;
    [SerializeField, Header("ノードの最大値を設定するテキスト")] TextMeshProUGUI _nodeMaxCountText;
    [SerializeField, Header("ノードの数を設定するテキスト")] TextMeshProUGUI _nodeCountText;
    [SerializeField, Header("ノードの間隔をこれで設定することができます")] float _nodeSpace;
    [SerializeField, Header("ノードの右端の間隔を調整できます")] float _location;
    [SerializeField, Header("ノードの最大数を設定します")]public int _maxNodeCount = 3;
    [SerializeField, Tooltip("ノードのprefacをアタッチしてください")] GameObject _nodePrefab;
    [SerializeField, Header("パーティクルをアタッチしてください")] ParticleSystem[] _particle;
    [SerializeField, Header("ノードの制限を設定します")]
    public float _defaultWidth;
    [SerializeField, Header("iPhone向け小刻みバイブの最小間隔(ms)")]
    int _vibrateMinIntervalMs = 120;
    float _lastVibrateTime = 0f;
    private void Awake()
    {
        Array.ForEach(_particle, x =>
        {
            var emmision = x.emission;
            emmision.rateOverTime = UnityEngine.Random.Range(0.98f, 1.3f);
        });
        _maxNodeCount = FindAnyObjectByType<InGameUIManager>()._nodeCount;
        DOTween.SetTweensCapacity(500, 50);
        _defaultWidth = ((RectTransform)transform).sizeDelta.x;
        _nodeMaxCountText.text = _maxNodeCount.ToString();
        _nodeCountText.text = (transform.childCount - 1).ToString();
        
        UndoUI.Add(null, x => DeskUndo((List<NodeData>)x), gameObject); // Undoに追加
        DeskUpdate();
        Executor.OnExecuteBegin += () => Array.ForEach(_particle, x =>
        {
            var emmision = x.emission;
            emmision.rateOverTime = UnityEngine.Random.Range(0.98f,1.3f);
        });
    }
    void Update()
    {
        if (_progressImage != null)
        {
            _progressImage.SetAsLastSibling();
        }
    }

    public void DeskUndoSave()
    {
        List<NodeData> nodeData = new List<NodeData>();
        for (int i = 0; i < transform.childCount; i++)
        {
            var data = transform.GetChild(i).GetComponent<NodeDataContainer>();
            if (data != null)
            {
                nodeData.Add(data.NodeData.DeepCopy());
            }
        }
        UndoUI.Add(nodeData, x => DeskUndo((List<NodeData>)x), gameObject);
    }
    public void AddNode(NodeMove node)
    {
        List<NodeData> nodeData = new List<NodeData>();
        for (int i = 0; i < transform.childCount; i++)
        {
            var data = transform.GetChild(i).GetComponent<NodeDataContainer>();
            if (data != null)
            {
                nodeData.Add(data.NodeData.DeepCopy());
            }
        }
        UndoUI.Add(nodeData, x => DeskUndo((List<NodeData>)x), gameObject);
        node.transform.SetParent(transform); // ノードを自身の子オブジェクトに設定
        if (node == null)
        {
            Debug.LogError("ノードがnullです。");
            return;
        }
        DeskUpdate(); // デスクを更新
    }
    public void RemoveNode()
    {
        List<NodeData> nodeData = new List<NodeData>();
        for (int i = 0; i < transform.childCount; i++)
        {
            var data = transform.GetChild(i).GetComponent<NodeDataContainer>();
            if (data != null)
            {
                nodeData.Add(data.NodeData.DeepCopy());
            }
        }
        UndoUI.Add(nodeData, x => DeskUndo((List<NodeData>)x), gameObject);
    }
    // 第一引数にノードの現在の進捗度、第二引数にノードのオブジェクトを渡します
    private void ProgressBarUpdate(float value, GameObject nodeObj)
    {
    if (_progressImage == null) return;
    if (nodeObj == null) return;
        if (value < 0)
        {
            _progressImage.anchoredPosition = new Vector2(0, _progressImage.anchoredPosition.y);
            foreach (var particle in _particle)
            {
                var emmision = particle.emission;
            }
            return;
        }
        _progressImage.anchoredPosition = new Vector2((((RectTransform)nodeObj.transform).anchoredPosition.x - (((RectTransform)nodeObj.transform).sizeDelta.x / 2)) + (((RectTransform)nodeObj.transform).sizeDelta.x * value), _progressImage.anchoredPosition.y);
    }
    /// <summary>
    /// デスクの更新を行います。
    /// ノードの位置を更新し、ノードのTweenを設定します。
    /// </summary>
    public void DeskUpdate()
    {
        DeskinUpdate().Forget();
    }
    public async UniTask DeskinUpdate()
    {
        _nodeCountText.text = (transform.childCount - 1).ToString(); // ノードの数を更新
        float location = _location; // ノードのxのロケーション
        var nodeBases = GetComponentsInChildren<NodeMove>(); // ノードのリストを取得
        var selfRectTransform = GetComponent<RectTransform>(); // 自身のRectTransformを取得
        var nodes = Array.ConvertAll(nodeBases, x => x.GetComponent<RectTransform>());
        nodes = System.Array.FindAll(nodes, node => node != selfRectTransform); // 自身のRectTransformを除外
        if (nodes.Length == 0)
        {
            DeskReset();
            return; // ノードがない場合は終了
        } // ノードがない場合は終了
        System.Array.Sort(nodes, (a, b) => a.anchoredPosition.x.CompareTo(b.anchoredPosition.x));
        // ノードのリストをソート#
        foreach (var node in nodes) // ノードのリストを実行
        {
            location += _nodeSpace; // ノードの間隔を設定
            location += node.rect.width; // ノードの幅を設定
            node.GetComponent<NodeVisual>().OnNodePlay = ProgressBarUpdate;
            node.gameObject.GetComponent<NodeMove>().Tween?.Kill();
            node.gameObject.GetComponent<NodeMove>().Tween = node.DOAnchorPos(new Vector2(location, 0), 0.5f); // ノードのTweenを設定
            node.SetAsLastSibling();
        }
        ((RectTransform)transform).sizeDelta = new Vector2((location + nodes[nodes.Length - 1].rect.width) > _defaultWidth ? location + nodes[nodes.Length - 1].rect.width : _defaultWidth, ((RectTransform)transform).sizeDelta.y); // 自身のRectTransformのサイズを設定
        // 親オブジェクトのサイズをこのオブジェクトが変更しないようにしました
        string path = SceneManager.GetActiveScene().name;
        await Task.Delay(500); // 0.5秒待機
        CriSEManager.Instance.PlaySE("SE_InGame_Node_Set", playOneShot: true); // ノードを追加したときにSEを再生
        TryMicroVibrate();
        
    }
    
    
    public void DeskUndo(object obj)
    {
        // すべての子ノードを削除
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (transform.GetChild(i).TryGetComponent<NodeDataContainer>(out var container))
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }

        var nodeDatas = obj as List<NodeData>;
        if (nodeDatas == null)
        {
            ((RectTransform)transform).sizeDelta = new Vector2(_defaultWidth, ((RectTransform)transform).sizeDelta.y);
            _nodeCountText.text = "0";
            return;
        }
        if (nodeDatas == null || nodeDatas.Count == 0)
        {
            // ノードデータが無い場合はリセット
            ((RectTransform)transform).sizeDelta = new Vector2(_defaultWidth, ((RectTransform)transform).sizeDelta.y);
            _nodeCountText.text = "0";
            return;
        }

        List<RectTransform> nodeRects = new List<RectTransform>();
        float location = _location;
        foreach (var nodeData in nodeDatas)
        {
            GameObject nodeObj = Instantiate(_nodePrefab, transform);
            var container = nodeObj.GetComponent<NodeDataContainer>();
            if (container != null)
                container.CopyInit(nodeData);
            var rect = nodeObj.GetComponent<RectTransform>();
            // アンカー・ピボットをLeft Centerに統一
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.localScale = Vector3.one;
            nodeObj.GetComponent<NodeVisual>().OnNodePlay = ProgressBarUpdate;
            // ノードの幅を取得（LayoutElement優先、なければrect.width）
            float nodeWidth = rect.rect.width;
            var layout = nodeObj.GetComponent<UnityEngine.UI.LayoutElement>();
            if (layout != null && layout.preferredWidth > 0)
                nodeWidth = layout.preferredWidth;

            // ノードの位置を設定
            location += _nodeSpace;
            location += nodeWidth;
            rect.anchoredPosition = new Vector2(location, 0);
            rect.SetAsLastSibling();
            nodeRects.Add(rect);
        }
        _nodeCountText.text = (nodeRects.Count).ToString();
        // ノードが1つも生成されなかった場合の安全対策
        float lastNodeWidth = (nodeRects.Count > 0) ? nodeRects[nodeRects.Count - 1].rect.width : 0f;
        float totalWidth = location + lastNodeWidth;
        float newWidth = (totalWidth > _defaultWidth) ? totalWidth : _defaultWidth;
        ((RectTransform)transform).sizeDelta = new Vector2(newWidth, ((RectTransform)transform).sizeDelta.y);
        // 親オブジェクトのサイズは変更しない
    }
    public void DeskReset()
    {
        var nodeBases = GetComponentsInChildren<NodeMove>();
        var selfRectTransform = GetComponent<RectTransform>(); // 自身のRectTransformを取得
        var nodes = Array.ConvertAll(nodeBases, x => x.GetComponent<RectTransform>());
        nodes = System.Array.FindAll(nodes, node => node != selfRectTransform); // 自身のRectTransformを除外
        List<NodeData> list = new List<NodeData>();
        foreach (var node in nodes)
        {
            if (!node.TryGetComponent<NodeDataContainer>(out var container)) return;
            list.Add(container.NodeData.DeepCopy()); // ノードデータを追加
            Destroy(node.gameObject); // ノードを削除
        }
        UndoUI.Add(list, x => DeskUndo((List<NodeData>)x), gameObject); // Undoに追加
        ((RectTransform)transform).sizeDelta = new Vector2(_defaultWidth, ((RectTransform)transform).sizeDelta.y); // 自身のRectTransformのサイズを設定
        // 親オブジェクトのサイズは変更しない
        string path = SceneManager.GetActiveScene().name;
        Debug.Log("ノードデータを保存しました");
        _nodeCountText.text = "0"; // ノードの数を更新
    }

    public bool isParfect()
    {
        if (transform.childCount - 1 > _maxNodeCount)
        {
            return false;
        }
        return true;
    }
    /// <summary>
/// iOS向けの小刻みバイブ（短い振動）を間隔制限付きで実行します
/// </summary>
void TryMicroVibrate()
{
        #if UNITY_IOS && !UNITY_EDITOR
        if (Time.unscaledTime - _lastVibrateTime < _vibrateMinIntervalMs / 1000f) return;
        Vibrate.VibrateDevice(1161);
        _lastVibrateTime = Time.unscaledTime;
        #endif
}
}

[System.Serializable]
public class NodeDatas
{
    public List<NodeData> NodeBases = new List<NodeData>();
}


