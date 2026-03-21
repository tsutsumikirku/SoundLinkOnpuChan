using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class Onpuni : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private RectTransform _rectTransform;
    private Vector3 _defaultScale;
    private bool _isPressed = false; // 入力受付フラグ

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _defaultScale = _rectTransform.localScale;
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
    }
}

