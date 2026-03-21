using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class UIMove : MonoBehaviour,IUserInterface
{
    [SerializeField] RectTransform _rectTransform;
    [SerializeField] Vector2 _showPosition;
    [SerializeField] Vector2 _hidePosition;
    [SerializeField] float _showTime = 0.8f;

    public void UnPlay()
    {
        _rectTransform.DOAnchorPos(_showPosition, _showTime).SetEase(Ease.OutFlash);
    }

    public void Play(int timeScale)
    {
        _rectTransform.DOAnchorPos(_hidePosition, _showTime * timeScale).SetEase(Ease.OutFlash);
    }
}
