using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using R3;
using Cysharp.Threading.Tasks.Triggers;
using Cysharp.Threading.Tasks;

public class UIVisible : MonoBehaviour,IUserInterface
{
    [SerializeField] RectTransform _rectTransform;
    [SerializeField] Image _image;
    [SerializeField] float _showTime = 0.5f;
    Vector2 _sizeDelta;
    void Start()
    {
        _sizeDelta = ((RectTransform)transform).sizeDelta;
        this.gameObject.SetActive(false);
    }
    public void UnPlay()
    {
        this.gameObject.SetActive(false);
    }
    public void Play(int timeScale)
    {
        PlayTask(timeScale);
    }
    public async UniTask PlayTask(int timeScale)
    {
        await UniTask.WaitForSeconds(_showTime * timeScale);
        this.gameObject.SetActive(true);
    }
}
