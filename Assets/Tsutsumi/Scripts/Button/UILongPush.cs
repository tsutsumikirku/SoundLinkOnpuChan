using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEditor;
using DG.Tweening;

public class UILongPush : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField, Header("UIを長押ししなければならない時間")] float _longPushTime;
    [SerializeField, Header("UIが長押し中に発火するEvent")] UnityEvent<float> _pushing;
    [SerializeField, Header("UI長押しが完了したときに発火するEvent")] UnityEvent _onPush;
    [SerializeField, Header("UIのImageを選択してください設定されていない場合はこのコンポーネントがアタッチされているクラスから取得されます")] Image _image;
    [SerializeField, Header("UIのRecttransformを選択してください設定されてない場合はこのオブジェクトから取得されます")] RectTransform _rect;
    [SerializeField, Header("UIの上にマウスカーソルが載ったのときの色を設定してください")] Color _pointerEnterColor;
    [SerializeField, Header("UIが押されているときの色を設定してください")] Color _pointerDownColor;
    [SerializeField, Header("UIの上にマウスカーソルが載った時のスケールの変更する量を設定してください")] Vector2 _pointerEnterScale;
    [SerializeField, Header("UIのが押されているときのスケールの変更量を設定してください")] Vector2 _pointerDownScale;
    [SerializeField, Header("UIの上にマウスカーソルが載った時のアニメーション完了までの時間")] float _pointerEnterAnimTime = 0.2f;
    [SerializeField, Header("UIが押されたときのアニメーション完了までの時間")] float _pointerDownAnimTime = 0.2f;
    [SerializeField, Header("UIが押されたときの効果音")] string _pushSound; 
    public Color NormalColor { get => _normalColor; set => _normalColor = value; }
    Color _normalColor;
    Vector2 _beforeScale;
    float _pushTime;
    bool _entry;
    bool _push;
    private void Awake()
    {
        if (_image == null)
        {
            _image = GetComponent<Image>();
        }
        _normalColor = _image.color;
        if (_rect == null)
        {
            _rect = GetComponent<RectTransform>();
        }
        _beforeScale = _rect.sizeDelta;
    }
    private void Update()
    {
        if (!_push)
        {
            _pushTime = 0f;
            _pushing?.Invoke(0);
            return;
        }
        _pushTime += Time.deltaTime;
        _pushing?.Invoke(_pushTime / _longPushTime);
        if (_pushTime > _longPushTime)
        {
            CriSEManager.Instance.PlaySE(_pushSound, playOneShot: true);
            _onPush?.Invoke();
            _push = false;
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        _push = true;
        _image.DOColor(_pointerDownColor,_pointerDownAnimTime);
        _rect.DOSizeDelta(_beforeScale + _pointerDownScale,_pointerDownAnimTime);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        _push = false;
        switch (_entry)
        {
            case true:
                _image.DOColor(_pointerEnterColor,_pointerDownAnimTime);
                _rect.DOSizeDelta(_beforeScale + _pointerEnterScale,_pointerDownAnimTime);
                break;
            case false:
                _image.DOColor(_normalColor, _pointerEnterAnimTime);
                _rect.DOSizeDelta(_beforeScale,_pointerEnterAnimTime);
                break;
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        #if windows || UNITY_STANDALONE
        _image.DOColor(_pointerEnterColor,_pointerEnterAnimTime);
        _rect.DOSizeDelta(_beforeScale + _pointerEnterScale,_pointerEnterAnimTime);
        _entry = true;
        #endif
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        #if windows || UNITY_STANDALONE
        if (!_push)
        {
            _image.DOColor(_normalColor, _pointerEnterAnimTime);
            _rect.DOSizeDelta(_beforeScale, _pointerEnterAnimTime);
        }
        _entry = false;
        #endif
    }
}
