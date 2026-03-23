using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NumericEntry : MonoBehaviour, INumericEntry
{
    public static NumericEntry Instance;
    [SerializeField, Header("セッティングパネルに入っているもの")] GameObject[] objects;
    [SerializeField, Header("セットしない際に表示するText")] TextMeshProUGUI _notSetText;
    [SerializeField, Header("Textを設定してください")] TextMeshProUGUI _text;
    [SerializeField, Header("値を表示するTextを設定してください")] TextMeshProUGUI _valueText;
    [SerializeField, Header("値のテキストを表示するテキストを設定してください")] TextMeshProUGUI _valueLabelText;
    public GameObject CopyButton;
    public Slider ValueSlider;
    public Action<float> ValueChange { get => _valueChange; set => _valueChange = value; }
    public Action<int> ValueCountChange { get => _onCountChange; set => _onCountChange = value; }
    public Action<float> OnSliderValueSet { get => _onSliderValueSet; set => _onSliderValueSet = value; }
    public Action OnTouchAction { get => _touchAction; set => _touchAction = value; }
    public Action ResetAction { get => _resetAction; set => _resetAction = value; }
    public Action CopyAction { get => _copyAction; set => _copyAction = value; }
    public Action ChangeAction { get => _changeAction; set => _changeAction = value; }
    Action<float> _valueChange;
    Action<int> _onCountChange;
    Action<float> _onSliderValueSet;
    Action _touchAction;
    Action _copyAction;
    Action _resetAction;
    Action _changeAction;
    void Awake()
    {
        Instance = this;
    }
    public void Close()
    {
        NumericInput.IsInputting = false;
    }

    public void OnNodeDestroy()
    {
        Array.ForEach(objects, obj => obj.SetActive(false));
        CopyButton.SetActive(false);
        _notSetText.enabled = true;
        _notSetText.text = "クリップを選んでね♪";
        _text.text = "";
    }
    public void SetName(string name)
    {
        _changeAction?.Invoke();
        _text.text = name;
        if (!_notSetText.enabled) return;
        _notSetText.text = name + "は値を設定することができないよ♪";
    }
    public void SetValue(float value, string lavel)
    {
        _valueText.text = value.ToString();
        _valueLabelText.text = lavel;
    }
    public void SetValue(int value, string lavel)
    {
        _valueText.text = value.ToString();
        _valueLabelText.text = lavel;
    }
    public void None()
    {
        Array.ForEach(objects, x => x.SetActive(false));
        _notSetText.enabled = true;
    }
    public void Set()
    {
        Array.ForEach(objects, x => x.SetActive(true));
        CopyButton.SetActive(true);
        _notSetText.enabled = false;
    }
    public void Reset()
    {
        _resetAction?.Invoke();
    }
    public void Copy()
    {
        _copyAction?.Invoke();
    }   
    public void ValueSetButton(float value)
    {
        // Unity UI's button binding often resolves to float overload, so route Count updates here as well.
        if (_onCountChange != null && _valueChange == null)
        {
            int countDelta = 0;
            if (!Mathf.Approximately(value, 0f))
            {
                countDelta = value > 0f ? 1 : -1;
            }

            if (countDelta != 0)
            {
                _onCountChange.Invoke(countDelta);
            }
            return;
        }
        _valueChange?.Invoke(value);
    }
    public void ValueSetButton(int value)
    {
        if (_onCountChange != null)
        {
            _onCountChange.Invoke(value);
            return;
        }
        _valueChange?.Invoke(value);
    }
    void OnDisable()
    {
        Instance = null;
    }

    public void ValueSet(string value)
    {
    }

    public void SliderValueSet()
    {
        _onSliderValueSet?.Invoke(ValueSlider.value);
    }
    public void SliderHandOut()
    {
        
        CriSEManager.Instance.PlaySE("SE_InGame_InputScreen_SetSecond", playOneShot: true);
    }

    public void SliderTouch()
    {
        _touchAction?.Invoke();
    }
}
