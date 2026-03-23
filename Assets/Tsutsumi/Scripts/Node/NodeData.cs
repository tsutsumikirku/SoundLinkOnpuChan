using System;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "NodeData", menuName = "ScriptableObjects/NodeData", order = 1)]
public class NodeData : ScriptableObject
{
    public NodeData DeepCopy()
    {
        var copy = ScriptableObject.CreateInstance<NodeData>();
        copy.Power = this.Power;
        copy.Second = this.Second;
        copy.MaxValue = this.MaxValue;
        copy.MinValue = this.MinValue;
        copy.DefaultValue = this.DefaultValue;
        copy.Info = this.Info;
        copy.NodeName = this.NodeName;
        copy._nodeSprite = this._nodeSprite;
        copy.PlayerAction = this.PlayerAction;
        copy.PlayerStateType = this.PlayerStateType;
        return copy;
    }
    [SerializeField, Header("PlayerStateTypeがPowerの場合設定してください")] private float _power;
    [SerializeField, Header("PlayerStateTypeがSecondの場合設定してください")] private float _second;
    [SerializeField, Header("ノードのパラメータのデフォルト値を入力してください")] private float _defaultValue = 0f;
    [SerializeField, Header("ノードの最大値を設定してください")] private float _maxValue = 10f;
    [SerializeField, Header("ノードの最小値を設定してください")] private float _minValue = 0f;
    [SerializeField, Header("ノードのインフォテキストを設定してください")] private string _info;
    [SerializeField, Header("ノードの名前を設定してください")] private string _nodeName;
    [SerializeField, Header("ノードの画像素材を設定してください")] private string _nodeSprite;
    [SerializeField, Header("ノードのアクションを選択してください")] private PlayerState _playerAction;
    [SerializeField, Header("ノードに設定するパラメータを選択してください")] private PlayerStateType _playerStateType;

    public float Power
    {
        get => _power;
        set
        {
            _power = value;
            OnPowerChange?.Invoke(_power);
        }
    }
    public float Second
    {
        get => _second;
        set
        {
            _second = value;
            OnSecondChange?.Invoke(_second);
        }
    }
    public float MaxValue { get => _maxValue; set => _maxValue = value; }
    public float MinValue { get => _minValue; set => _minValue = value; }
    public float DefaultValue { get => _defaultValue; set => _defaultValue = value; }
    public string Info { get => _info; set => _info = value; }
    public string NodeName { get => _nodeName; set => _nodeName = value; }

    public Action<float> OnPowerChange;
    public Action<float> OnSecondChange;

    public Sprite NodeSprite { get => Resources.Load<Sprite>(_nodeSprite); }
    public string NodePassGet { set => _nodeSprite = value;  get => _nodeSprite; }
    public PlayerState PlayerAction { get => _playerAction; set => _playerAction = value; }
    public PlayerStateType PlayerStateType { get => _playerStateType; set => _playerStateType = value; }
}
public enum PlayerStateType
{
    None,
    Second,
    Power,
    Count
}
