using System;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField, Header("クリック時のイベントを設定してください")] UnityEvent onClick;
    [SerializeField, Header("マウスカーソルが押されたときか離されたときか設定してください")] ButtonType _buttonType = ButtonType.Down;
    [SerializeField, Header("マウスカーソルがUIの上に載っているときのカラーを指定してください")] Color _pointerEnterColor;
    [SerializeField, Header("UIが押されたときのカラーを指定してください")] Color _pointerDownColor;
    [SerializeField, Header("マウスカーソルがUIの上に載っているときのスケールを指定してください")] Vector2 _pointerEnterScale;
    [SerializeField, Header("UIが押されたときのスケールを指定してください")] Vector2 _pointerDownScale;
    [SerializeField, Header("UIのImageを選択してください設定されていない場合はこのコンポーネントがアタッチされているクラスから取得されます")] Image _image;
    [SerializeField, Header("UIのRecttransformを選択してください設定されてない場合はこのオブジェクトから取得されます")] RectTransform _rect;
    [SerializeField, Header("アニメーションの時間")] float _animTime;
    [SerializeField, Header("クリックした際に変わるSprite設定したら変わるよ")] Sprite _clickSprite;
    [SerializeField, Header("押した際になるSE")] string _queName;
    Sprite _defaultSprite;
    Color _defaultColor;
    Vector2 _defaultScale;
    public Action OnClick;
    bool _click;
    bool _entry;

    void Awake()
    {
        if (!_image)
        {
            TryGetComponent<Image>(out _image);
        }
        if (!_rect)
        {
            _rect = GetComponent<RectTransform>();
        }
        _defaultScale = _rect.localScale;
        _defaultColor = Color.white;
        if (!_image) return;
        _image.color = _defaultColor;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        _click = true;
        if (_clickSprite)
        {
            _defaultSprite = _image.sprite;
            _image.sprite = _clickSprite;
        }
        if (_buttonType == ButtonType.Down)
        {
            if (_queName != "")
                CriSEManager.Instance.PlaySE(_queName, playOneShot: true);
            onClick?.Invoke();
            OnClick?.Invoke();
        }
        _rect.DOScale(_defaultScale + _pointerDownScale, _animTime * TimeScaleManager.InGameTimeScale);
        if (!_image) return;
        _image.DOColor(_pointerDownColor, _animTime * TimeScaleManager.InGameTimeScale);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_clickSprite)
        {
            _image.sprite = _defaultSprite;
        }
        _click = false;
        if (_buttonType == ButtonType.Up)
        {
            if (_queName != "")
                CriSEManager.Instance.PlaySE(_queName, playOneShot: true);
            onClick?.Invoke();
            OnClick?.Invoke();
        }
        if (_entry)
        {
            _rect.DOScale(_defaultScale + _pointerEnterScale, _animTime * TimeScaleManager.InGameTimeScale);
            if (!_image) return;
            _image.DOColor(_pointerEnterColor, _animTime * TimeScaleManager.InGameTimeScale);
        }
        else
        {
            _rect.DOScale(_defaultScale, _animTime * TimeScaleManager.InGameTimeScale);
            if (!_image) return;
            _image.DOColor(_defaultColor, _animTime * TimeScaleManager.InGameTimeScale);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
#if windows || UNITY_STANDALONE
        _entry = true;
        if (!_click)
        {
            _rect.DOScale(_defaultScale + _pointerEnterScale, _animTime * TimeScaleManager.InGameTimeScale);
            if (!_image) return;
            _image.DOColor(_pointerEnterColor, _animTime * TimeScaleManager.InGameTimeScale);
        }
#endif
    }

    public void OnPointerExit(PointerEventData eventData)
    {
#if windows || UNITY_STANDALONE
        _entry = false;
        if (!_click)
        {
            _rect.DOScale(_defaultScale, _animTime * TimeScaleManager.InGameTimeScale);
            if (!_image) return;
            _image.DOColor(_defaultColor, _animTime * TimeScaleManager.InGameTimeScale);
        }
#endif
    }
    void OnDisable()
    {
        if (_rect != null)
        {
            _rect.localScale = _defaultScale;
        }
        if (_image != null)
        {
            _image.color = _defaultColor;
        }
    }
}
public enum ButtonType
{
    Down,
    Up
}
