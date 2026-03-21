using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class UISizeChange : MonoBehaviour,IUserInterface
{
    [SerializeField] RectTransform _rectTransform;
    [SerializeField] Vector2 _showSizeDelta;
    [SerializeField] Vector2 _hideSizeDelta;
    [SerializeField] float _showTime = 0.10f;
    public void UnPlay()
	{
		_rectTransform.DOSizeDelta(_showSizeDelta, _showTime).SetEase(Ease.OutFlash);
	}   

	public void Play(int timeScale)
	{
		_rectTransform.DOSizeDelta(_hideSizeDelta,_showTime * timeScale).SetEase(Ease.OutFlash);
	}
}
