using System.Collections;
using System.Collections.Generic;
using R3;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(UIButton))]
[RequireComponent(typeof(Image))]
public class TimeScaleButton : MonoBehaviour
{
    [SerializeField] int _ChangeScale = 1;
    [SerializeField]Sprite _oneSprite;
    [SerializeField]Sprite _twoSprite;
    [SerializeField] TextMeshProUGUI _text;
    Image _image;
    InGameUIManager uiManager;
    string _originalText;
    void Awake()
    {
        _image = GetComponent<Image>();
        uiManager = FindAnyObjectByType<InGameUIManager>();
        uiManager.TimeScaleChange += TimeScaleChange;
        _originalText = _text.text;
        _text.text = _originalText.Replace("〇", "1");
    }
    public void Button()
    {
        if (uiManager.TimeScale == 1)
        {
            uiManager.TimeScale = _ChangeScale;
            CriSEManager.Instance.PlaySE("SE_InGame_Speed_Triple", playOneShot: true);
            _text.text = _originalText.Replace("〇", "3");
            return;
        }
        CriSEManager.Instance.PlaySE("SE_InGame_Speed_Normal", playOneShot: true);
        uiManager.TimeScale = 1;
        _text.text = _originalText.Replace("〇", "1");

    }
    void TimeScaleChange(int timeScale)
    {
        if (_ChangeScale == timeScale)
        {
            _image.sprite = _twoSprite;
            return;
        }
        _image.sprite = _oneSprite;
    }
}

