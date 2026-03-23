using System;
using TMPro;
using UnityEngine;
public class NodeDataContainer : MonoBehaviour, INode
{
    [SerializeField, Header("NodeVisualが実装されたクラスがアタッチされているオブジェクトを設定してください")] GameObject _nodeVisualObject;
    [SerializeField, Header("UIInfoSetをアタッチしてください")] UIInfoSet _uiInfoSet;
    [SerializeField, Header("Nodeの名前を記入するTMP")] TextMeshProUGUI _nodeText;
    private INodeVisual _nodeVisual => _nodeVisualObject.GetComponent<INodeVisual>();
    public float Power { get => _nodeData.Power; }
    public float Second { get => _nodeData.Second; }
    public PlayerState PlayerAction { get => _nodeData.PlayerAction; }
    public NodeData NodeData { get => _nodeData; set => _nodeData = value; }
    private NodeData _nodeData;

    // このメソッドはゲーム実行時にマネージャークラスが呼び出す
    // NodeDataを引数に取り、そのデータを元にノードを初期化する
    public void Init(NodeData nodeData)
    {
        float defaultPower = nodeData.PlayerStateType == PlayerStateType.Count
            ? Mathf.RoundToInt(nodeData.DefaultValue)
            : nodeData.DefaultValue;

        _nodeData = new NodeData
        {
            Power = defaultPower,
            Second = nodeData.DefaultValue,
            DefaultValue = defaultPower,
            PlayerAction = nodeData.PlayerAction,
            PlayerStateType = nodeData.PlayerStateType,
            Info = nodeData.Info,
            NodeName = nodeData.NodeName,
            MaxValue = nodeData.MaxValue,
            MinValue = nodeData.MinValue,
            NodePassGet = nodeData.NodePassGet
        };
        _uiInfoSet._text = _nodeData.Info;
        _nodeVisual.PlayerDataSet(nodeData.NodeSprite, nodeData.NodeName);
        switch (_nodeData.PlayerStateType)
        {
            case PlayerStateType.Power:
                _nodeVisual.NodeDataSet(_nodeData.Power.ToString());
                break;
            case PlayerStateType.Second:
                _nodeVisual.NodeDataSet(_nodeData.Second.ToString());
                break;
            case PlayerStateType.Count:
                _nodeVisual.NodeDataSet(Mathf.RoundToInt(_nodeData.Power).ToString());
                break;
        }
        _nodeVisual.OnPowerChange += (x) =>
        {
            UndoSetPower(_nodeData.Power);
            float next = (_nodeData.Power * 10) + x;
            next = next / 10f; // 小数点以下を1桁にする
            if (next > _nodeData.MaxValue)
            {
                next = _nodeData.MaxValue; // 最大値を超えないようにする
            }
            else if (next < _nodeData.MinValue)
            {
                next = _nodeData.MinValue; // 最小値を下回らないようにする
            }
            _nodeData.Power = next;
            _nodeVisual.NodeDataSet(_nodeData.Power.ToString());
            _nodeVisual.SetValue(_nodeData.Power, "パワー");
            NumericEntry.Instance.ValueSlider.value = _nodeData.Power;
        };
        _nodeVisual.OnSecondChange += (x) =>
        {
            UndoSetSecond(_nodeData.Second);
            float next = (_nodeData.Second * 10) + x;
            next = next / 10f; // 小数点以下を1桁にする
            if (next > _nodeData.MaxValue)
            {
                next = _nodeData.MaxValue; // 最大値を超えないようにする
            }
            else if (next < _nodeData.MinValue)
            {
                next = _nodeData.MinValue; // 最小値を下回らないようにする
            }
            _nodeData.Second = next;
            _nodeVisual.NodeDataSet(_nodeData.Second.ToString());
            _nodeVisual.SetValue(_nodeData.Second, "秒");
            NumericEntry.Instance.ValueSlider.value = _nodeData.Second;
        };
        _nodeVisual.OnCountChange += (x) =>
        {
            UndoSetPower(_nodeData.Power);
            float next = _nodeData.Power + x;
            if (next > _nodeData.MaxValue)
            {
                next = _nodeData.MaxValue;
            }
            else if (next < _nodeData.MinValue)
            {
                next = _nodeData.MinValue;
            }

            int intNext = Mathf.RoundToInt(next);
            _nodeData.Power = intNext;
            _nodeVisual.NodeDataSet(intNext.ToString());
            _nodeVisual.SetValue(intNext, "回");
            NumericEntry.Instance.ValueSlider.value = intNext;
        };
        _nodeVisual.ResetButton = () =>
        {
            _nodeData.Power = _nodeData.DefaultValue;
            _nodeData.Second = _nodeData.DefaultValue;
            switch (_nodeData.PlayerStateType)
            {
                case PlayerStateType.Second:
                    _nodeVisual.NodeDataSet(_nodeData.Second.ToString());
                    NumericEntry.Instance.ValueSlider.value = _nodeData.Second;
                    break;
                case PlayerStateType.Power:
                    _nodeVisual.NodeDataSet(_nodeData.Power.ToString());
                    NumericEntry.Instance.ValueSlider.value = _nodeData.Power;
                    break;
                case PlayerStateType.Count:
                    _nodeVisual.NodeDataSet(Mathf.RoundToInt(_nodeData.Power).ToString());
                    NumericEntry.Instance.ValueSlider.value = Mathf.RoundToInt(_nodeData.Power);
                    break;
            }
            Debug.Log("ノードのデータをリセットしました: " + _nodeData.PlayerAction.ToString());
        };
        _nodeVisual.PlayerTypeSet(_nodeData.PlayerStateType);
        _nodeText.text = _nodeData.NodeName;
    }
    public void CopyInit(NodeData nodeData)
    {
        _nodeData = nodeData;
        _uiInfoSet._text = _nodeData.Info;
        _nodeVisual.PlayerDataSet(_nodeData.NodeSprite, _nodeData.NodeName);
        switch (_nodeData.PlayerStateType)
        {
            case PlayerStateType.Power:
                _nodeVisual.NodeDataSet(_nodeData.Power.ToString());
                break;
            case PlayerStateType.Second:
                _nodeVisual.NodeDataSet(_nodeData.Second.ToString());
                break;
            case PlayerStateType.Count:
                _nodeVisual.NodeDataSet(Mathf.RoundToInt(_nodeData.Power).ToString());
                break;
        }
        _nodeVisual.OnPowerChange += (x) =>
        {
            if (transform.parent.gameObject.TryGetComponent<TimeLine>(out var timeLine))
            {
                timeLine.RemoveNode();
            }
            else
            {
                UndoSetPower(_nodeData.Power);
            }
            float next = (_nodeData.Power * 10) + x;
            next = next / 10f; // 小数点以下を1桁にする
            if (next > _nodeData.MaxValue)
            {
                next = _nodeData.MaxValue; // 最大値を超えないようにする
            }
            else if (next < _nodeData.MinValue)
            {
                next = _nodeData.MinValue; // 最小値を下回らないようにする
            }
            _nodeData.Power = next;
            _nodeVisual.NodeDataSet(_nodeData.Power.ToString());
            if(_nodeData.PlayerStateType == PlayerStateType.Power)
                _nodeVisual.SetValue(_nodeData.Power, "パワー");
            else if(_nodeData.PlayerStateType == PlayerStateType.Count)
                _nodeVisual.SetValue((int)_nodeData.Power, "回");
            NumericEntry.Instance.ValueSlider.value = _nodeData.Power;
        };
        _nodeVisual.OnSecondChange += (x) =>
        {
            if (transform.parent.gameObject.TryGetComponent<TimeLine>(out var timeLine))
            {
                timeLine.RemoveNode();
            }
            else
            {
                UndoSetSecond(_nodeData.Second);
            }
            float next = (_nodeData.Second * 10) + x;
            next = next / 10f; // 小数点以下を1桁にする
            if (next > _nodeData.MaxValue)
            {
                next = _nodeData.MaxValue; // 最大値を超えないようにする
            }
            else if (next < _nodeData.MinValue)
            {
                next = _nodeData.MinValue; // 最小値を下回らないようにする
            }
            _nodeData.Second = next;
            _nodeVisual.NodeDataSet(_nodeData.Second.ToString());
            _nodeVisual.SetValue(_nodeData.Second, "秒");
            NumericEntry.Instance.ValueSlider.value = _nodeData.Second;
        };
        _nodeVisual.OnCountChange += (x) =>
        {
            if (transform.parent.gameObject.TryGetComponent<TimeLine>(out var timeLine))
            {
                timeLine.RemoveNode();
            }
            else
            {
                UndoSetPower(_nodeData.Power);
            }

            float next = _nodeData.Power + x;
            if (next > _nodeData.MaxValue)
            {
                next = _nodeData.MaxValue;
            }
            else if (next < _nodeData.MinValue)
            {
                next = _nodeData.MinValue;
            }

            int intNext = Mathf.RoundToInt(next);
            _nodeData.Power = intNext;
            _nodeVisual.NodeDataSet(intNext.ToString());
            _nodeVisual.SetValue(intNext, "回");
            NumericEntry.Instance.ValueSlider.value = intNext;
        };
        _nodeVisual.ResetButton = () =>
        {
            _nodeData.Power = _nodeData.DefaultValue;
            _nodeData.Second = _nodeData.DefaultValue;
            switch (_nodeData.PlayerStateType)
            {
                case PlayerStateType.Second:
                    _nodeVisual.NodeDataSet(_nodeData.Second.ToString());
                    NumericEntry.Instance.ValueSlider.value = _nodeData.Second;
                    break;
                case PlayerStateType.Power:
                    _nodeVisual.NodeDataSet(_nodeData.Power.ToString());
                    NumericEntry.Instance.ValueSlider.value = _nodeData.Power;
                    break;
                case PlayerStateType.Count:
                    _nodeVisual.NodeDataSet(Mathf.RoundToInt(_nodeData.Power).ToString());
                    NumericEntry.Instance.ValueSlider.value = Mathf.RoundToInt(_nodeData.Power);
                    break;
            }
            Debug.Log("ノードのデータをリセットしました: " + _nodeData.PlayerAction.ToString());
        };
        _nodeVisual.PlayerTypeSet(_nodeData.PlayerStateType);
        _nodeText.text = _nodeData.NodeName;
    }
    void UndoSetPower(float value)
    {
        UndoUI.Add(value, x =>
        {
            if (_nodeData.PlayerStateType == PlayerStateType.Count)
            {
                _nodeData.Power = Mathf.RoundToInt((float)x);
                _nodeVisual.NodeDataSet(Mathf.RoundToInt(_nodeData.Power).ToString());
            }
            else
            {
                _nodeData.Power = (float)x;
                _nodeVisual.NodeDataSet(_nodeData.Power.ToString());
            }
            if(_nodeData.PlayerStateType == PlayerStateType.Power)
            _nodeVisual.SetValue(_nodeData.Power, "パワー");
            else if(_nodeData.PlayerStateType == PlayerStateType.Count)
                _nodeVisual.SetValue((int)_nodeData.Power, "回数");
        }, gameObject );    
    }
    void UndoSetSecond(float value)
    {
        UndoUI.Add(value, x =>
        {
            _nodeData.Second = (float)x;
            _nodeVisual.NodeDataSet(_nodeData.Second.ToString());
            _nodeVisual.SetValue(_nodeData.Second, "秒");
        }, gameObject);
    }
    public void SetSlider()
    {
        NumericEntry.Instance.ValueSlider.onValueChanged.RemoveAllListeners();
        switch (_nodeData.PlayerStateType)
        {
            case PlayerStateType.Power:
                NumericEntry.Instance.ValueSlider.maxValue = _nodeData.MaxValue;
                NumericEntry.Instance.ValueSlider.minValue = _nodeData.MinValue;
                NumericEntry.Instance.ValueSlider.value = _nodeData.Power;
                NumericEntry.Instance.ValueSlider.onValueChanged.RemoveAllListeners();
                NumericEntry.Instance.ValueSlider.onValueChanged.AddListener((float val) =>
                {
                    float copy = _nodeData.Power;
                    val = Mathf.Round(val * 10) / 10f;
                    _nodeData.Power = val;
                    _nodeVisual.SetValue(val, "パワー");
                    _nodeVisual.NodeDataSet(_nodeData.Power.ToString());
                });
                NumericEntry.Instance.OnSliderValueSet = val =>
                {
                    if (transform.parent.gameObject.TryGetComponent<TimeLine>(out var timeLine))
                    {
                        timeLine.RemoveNode();   
                    }
                    else
                    {
                        val = Mathf.Round(val * 10) / 10f;
                        UndoSetPower(val);
                    }
                };
                break;
            case PlayerStateType.Second:
                NumericEntry.Instance.ValueSlider.maxValue = _nodeData.MaxValue;
                NumericEntry.Instance.ValueSlider.minValue = _nodeData.MinValue;
                NumericEntry.Instance.ValueSlider.value = _nodeData.Second;
                NumericEntry.Instance.ValueSlider.onValueChanged.RemoveAllListeners();
                NumericEntry.Instance.ValueSlider.onValueChanged.AddListener((float val) =>
                {
                    val = Mathf.Round(val * 10) / 10f;
                    _nodeData.Second = val;
                    _nodeVisual.SetValue(val, "秒");
                    _nodeVisual.NodeDataSet(_nodeData.Second.ToString());
                });
                NumericEntry.Instance.OnSliderValueSet = val =>
                {
                    if (transform.parent.gameObject.TryGetComponent<TimeLine>(out var timeLine))
                    {
                        timeLine.RemoveNode();   
                    }
                    else
                    {
                        val = Mathf.Round(val * 10) / 10f;
                        UndoSetSecond(val);
                    }
                };
                break;
            case PlayerStateType.Count:
                NumericEntry.Instance.ValueSlider.maxValue = _nodeData.MaxValue;
                NumericEntry.Instance.ValueSlider.minValue = _nodeData.MinValue;
                NumericEntry.Instance.ValueSlider.value = _nodeData.Power;
                NumericEntry.Instance.ValueSlider.onValueChanged.RemoveAllListeners();
                NumericEntry.Instance.ValueSlider.onValueChanged.AddListener((float val) =>
                {
                    int intVal = Mathf.RoundToInt(val);
                    _nodeData.Power = intVal;
                    _nodeVisual.SetValue(intVal, "回");
                    _nodeVisual.NodeDataSet(_nodeData.Power.ToString());
                });
                NumericEntry.Instance.OnSliderValueSet = val =>
                {
                    if (transform.parent.gameObject.TryGetComponent<TimeLine>(out var timeLine))
                    {
                        timeLine.RemoveNode();   
                    }
                    else
                    {
                        int intVal = Mathf.RoundToInt(val);
                        UndoSetPower(intVal);
                    }
                };
                break;

        }
    }
    public void NodeDataInit(NodeData nodeData)
    {
        _nodeData = nodeData;
        _nodeVisual.PlayerTypeSet(nodeData.PlayerStateType);
        Debug.Log("ノードデータを設定しました: " + nodeData.PlayerAction.ToString());
    }
    public void StartNode()
    {
        _nodeVisual.StartVisual();
    }

    public void EndNode()
    {
        _nodeVisual.EndVisual();
    }
}

public interface INodeVisual
{
    Action<float> OnPowerChange { get; set; }
    Action<float> OnSecondChange { get; set; }
    Action<int> OnCountChange { get; set; }
    Action ResetButton { get; set; }
    void SetValue(float value, string lavel);
    void PlayerTypeSet(PlayerStateType playerState);
    void PlayerDataSet(Sprite sprite, string name);
    void NodeDataSet(string value);
    void StartVisual();
    void EndVisual();
}