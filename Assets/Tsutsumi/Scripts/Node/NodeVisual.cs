using System;
using Coffee.UIExtensions;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class NodeVisual : CancellableComponentBase, INodeVisual, IPointerDownHandler, IPointerUpHandler
{
	public Action<float, GameObject> OnNodePlay;
	Action<float> _onPowerChange;
	Action<float> _onSecondChange;
	Action _copyAction;
	Action _resetButton;
	public Action<float> OnPowerChange { get => _onPowerChange; set => _onPowerChange = value; }
	public Action<float> OnSecondChange { get => _onSecondChange; set => _onSecondChange = value; }
	public Action ResetButton { get => _resetButton; set => _resetButton = value; }
	public Action CopyAction { get => _copyAction; set => _copyAction = value; }
	[SerializeField, Header("アイコンのImage")] Image _iconImage;
	[SerializeField, Header("設定画面中のSprite")] Sprite _playSprite;
	[SerializeField, Header("通常時のSprite")] Sprite _defaultSprite;
	[SerializeField, Header("テキストを設定してください")] TextMeshProUGUI _text;
	[SerializeField, Header("文字変更時のエフェクトがあればアタッチしてください")] UIParticle _uiParticle;
	[SerializeField, Header("NodePanel")] GameObject _nodePanel;
	[SerializeField, Header("NodeDataContainer")] NodeDataContainer _nodeDataContainer;
	Image _nodeImage;
	PlayerStateType _playerStateType;
	string _playerStateName;
	Tween _tween;
	private void Start()
	{
		_nodeImage = GetComponent<Image>();
		_nodeImage.sprite = _defaultSprite;
	}
	public void OnPointerDown(PointerEventData eventData)
	{
		if (!NumericEntry.Instance) return;
		NumericEntryButton();
	}
	public void NumericEntryButton()
	{
		NumericEntry.Instance.ValueSlider.gameObject.SetActive(true);
		switch (_playerStateType)
		{
			case PlayerStateType.Power:
				NumericEntry.Instance.Set();
				NumericEntry.Instance.SetValue(_nodeDataContainer.Power, "パワー");
				NumericEntry.Instance.ValueChange = _onPowerChange;
				break;
			case PlayerStateType.Second:
				NumericEntry.Instance.Set();
				NumericEntry.Instance.SetValue(_nodeDataContainer.Second, "秒");
				NumericEntry.Instance.ValueChange = _onSecondChange;
				break;
			case PlayerStateType.None:
				NumericEntry.Instance.None();
				NumericEntry.Instance.CopyButton.SetActive(true);
				break;
		}
		NumericEntry.Instance.SetName(_playerStateName);
		if (_nodeImage != null)
		{
			_nodeImage.sprite = _playSprite;
		}
		NumericEntry.Instance.ChangeAction = () => {
			if (_nodeImage != null)
			{
				_nodeImage.sprite = _defaultSprite;
			}
		};
		NumericEntry.Instance.CopyAction = () =>
		{
			if (!this.transform.parent.TryGetComponent<TimeLine>(out var desk))
			{
				return;
			}
			desk.DeskUndoSave();
			var copyObj = Instantiate(this.gameObject, transform.parent);
			copyObj.GetComponent<NodeDataContainer>().CopyInit(_nodeDataContainer.NodeData.DeepCopy());
			desk.DeskUpdate();
		};
		NumericEntry.Instance.ResetAction = _resetButton;
		_nodeDataContainer.SetSlider();
	}
	public void SetValue(float value, string lavel)
	{
		if (!NumericEntry.Instance) return;
		NumericEntryButton();
		NumericEntry.Instance.SetValue(value, lavel);
	}
	private void OnDestroy()
	{
		if (!NumericEntry.Instance) return;
		NumericEntry.Instance.ValueChange = null;
		NumericEntry.Instance.CopyAction = null;
		NumericEntry.Instance.ResetAction = null;
		NumericEntry.Instance.ChangeAction = null;
		NumericEntry.Instance.SetValue(0, "");
		NumericEntry.Instance.SetName("");
		NumericEntry.Instance.ValueSlider.value = 0;
		NumericEntry.Instance.ValueSlider.onValueChanged.RemoveAllListeners();
		NumericEntry.Instance.ValueSlider.gameObject.SetActive(false);
		NumericEntry.Instance.OnNodeDestroy();
	}
	/// <summary>
	/// ノードのデータを設定します。
	/// </summary>
	/// <param name="value"></param>
	public void NodeDataSet(string value)
	{
		_text.text = value;
	}
	/// <summary>
	/// ノードのデータを更新します。
	/// </summary>
	/// <param name="type"></param>
	public void PlayerTypeSet(PlayerStateType type)
	{
		_playerStateType = type;
		switch (type)
		{
			case PlayerStateType.Power:
				break;
			case PlayerStateType.Second:
				break;
			case PlayerStateType.None:
				_nodePanel.SetActive(false);
				break;
		}
	}
	public void PlayerDataSet(Sprite sprite, string name)
	{
		_iconImage.sprite = sprite;
		_playerStateName = name;
	}
	/// <summary>
	/// ノードの再生中のビジュアルを開始します。
	/// </summary>
	public void StartVisual()
	{
		if (_playerStateType == PlayerStateType.Second)
		{
			_tween = DOTween.To(() => 0f, x => OnNodePlay?.Invoke(x, this.gameObject), 1f, _nodeDataContainer.Second).SetEase(Ease.Linear);
			return;
		}
		OnNodePlay?.Invoke(0, this.gameObject);
		return;
	}
	/// <summary>
	/// ノードの再生中のビジュアルを終了します。
	/// </summary>
	public void EndVisual()
	{
		OnNodePlay?.Invoke(1, this.gameObject);
	}

    public void OnPointerUp(PointerEventData eventData)
    {
    }

	protected override void OnExecuteCancelled()
	{
		if (this == null || gameObject == null) return;
		_tween?.Kill();
		OnNodePlay?.Invoke(-1, this.gameObject);
	}
}
public interface INumericEntry
{
	Action<float> ValueChange { get; set; }
	void ValueSet(string value);
}
