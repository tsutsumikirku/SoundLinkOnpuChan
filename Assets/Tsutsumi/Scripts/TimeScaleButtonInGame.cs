using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using R3;

public class TimeScaleButtonInGame : MonoBehaviour
{
    
    [SerializeField] int _timeScale = 1;
    [SerializeField] Sprite[] _sprite;
    Button _button;
    Image _image;
    void Awake()
    {
        _button = GetComponent<Button>();
        _image = GetComponent<Image>();
        InGameUIManager uiManager = FindAnyObjectByType<InGameUIManager>();
        uiManager.TimeScaleChange += TimeScaleChange;
        _button.OnClickAsObservable()
            .Subscribe(_ =>
            {
                CriSEManager.Instance.PlaySE("SE_InGame_Speed_Normal", playOneShot: true);
                TimeScaleManager.ChangeTimeScale(_timeScale);
                uiManager.TimeScale = _timeScale;
            })
            .AddTo(this);
    }
    void TimeScaleChange(int timeScale)
    {
        if (_timeScale == timeScale)
        {
            _image.sprite = _sprite[1];
            return;
        }
        _image.sprite = _sprite[0];
    }
}
