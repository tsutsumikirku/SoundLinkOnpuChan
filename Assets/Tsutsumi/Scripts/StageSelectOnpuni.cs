using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;

public class StageSelectOnpuni : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] TextMeshProUGUI _tmp;
    [SerializeField] private string _text;
    [SerializeField] private Image _textImage;
    [SerializeField] float _textSpeed = 0.1f;
    private string[] _texts;
    private RectTransform _rectTransform;
    private Vector3 _defaultScale;
    private bool _isPressed = false; // 入力受付フラグ
    CancellationTokenSource _cancellationTokenSource;

    void Awake()
    {
        _texts = _text.Split('*');
        _rectTransform = GetComponent<RectTransform>();
        _defaultScale = _rectTransform.localScale;
    }
    void Start()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        StartChat().Forget();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_isPressed) return; // すでに押されていたら無視
        _isPressed = true;
        _rectTransform.DOScale(_defaultScale * 0.9f, 0.1f).SetEase(Ease.OutQuad);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_isPressed) return; // 押されていなければ無視
        _isPressed = false;
        _rectTransform.DOScale(_defaultScale, 0.4f)
            .SetEase(Ease.OutElastic, overshoot: 3f)
            .OnKill(() => _rectTransform.localScale = _defaultScale);
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        Chat().Forget();
    }
    async UniTask Chat()
    {
        _tmp.text = null;
        if (!_textImage.gameObject.activeSelf)
        {
            _textImage.gameObject.SetActive(true);
            var defaultScale = _textImage.transform.localScale;
            _textImage.transform.localScale = Vector2.zero;
            _textImage.transform.DOScale(defaultScale, 0.5f);
        }
        var text = _texts[Random.Range(0, _texts.Length)];
        for (int i = 0; i < text.Length; i++)
        {
            _tmp.text += text[i];
            await UniTask.WaitForSeconds(_textSpeed, cancellationToken: _cancellationTokenSource.Token);
        }
    }
    async UniTask StartChat()
    {
        _tmp.text = null;
        if (!_textImage.gameObject.activeSelf)
        {
            _textImage.gameObject.SetActive(true);
            var defaultScale = _textImage.transform.localScale;
            _textImage.transform.localScale = Vector2.zero;
            _textImage.transform.DOScale(defaultScale, 0.5f);
        }
        var text = "ステージを選んでね♪";
        for (int i = 0; i < text.Length; i++)
        {
            _tmp.text += text[i];
            await UniTask.WaitForSeconds(_textSpeed, cancellationToken: _cancellationTokenSource.Token);
        }
    }
    void OnDisable()
    {
        _cancellationTokenSource?.Cancel();
    }
}