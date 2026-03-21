using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIDisplay : MonoBehaviour
{
    [SerializeField]private Button _displayButton;
    [SerializeField]private Button _behindButton;
    private Vector2 _behindPos;
    void Start()
    {
        _behindPos = ((RectTransform)transform).anchoredPosition;
        _displayButton.onClick.AddListener(() =>
        {
            ((RectTransform)transform).DOAnchorPos(new Vector2(0, 0), 0.5f);
        });
        _behindButton.onClick.AddListener(() =>
        {
            ((RectTransform)transform).DOAnchorPos(_behindPos, 0.5f);
        });
    }
}
