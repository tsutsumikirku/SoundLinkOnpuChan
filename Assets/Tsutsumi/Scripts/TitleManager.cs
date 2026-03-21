using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class TitleManager : MonoBehaviour
{
    [SerializeField] private UIButton _button;
    [SerializeField] private UIButton _button2;
    [SerializeField] private GameObject[] _views;
    [SerializeField] private float _animationTime = 0.2f;
    [SerializeField] private float _waitTime = 0.5f;
    [SerializeField] SceneLoader _sceneLoader;
    async UniTask Start()
    {
        await UniTask.WaitForSeconds(_waitTime);
        BGMManager.Instance.TitleBGM(true);
        _button.OnClick = () =>
        {
            _sceneLoader.LoadScene("StageSelect");
        };
        await UniTask.WaitForSeconds(_animationTime);
        _button.gameObject.SetActive(true);
        _button2.gameObject.SetActive(true);
        Array.ForEach(_views,x => x.SetActive(true));
        var rect = _button.GetComponent<RectTransform>();
        var scale = rect.localScale;
        rect.localScale = Vector3.zero;
        rect.DOScale(scale, 0.3f).SetEase(Ease.InOutSine);
    }

    void OnDisable()
    {
        BGMManager.Instance.TitleBGM(false);    
    }
}
