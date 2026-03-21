using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ViewPortChanger : MonoBehaviour,IUserInterface
{
    [SerializeField]RawImage _rawImage = null;
    [SerializeField]RenderTexture _beforeViewportSize;
    [SerializeField]RenderTexture _afterViewportSize;

    public void UnPlay()
    {
        _rawImage.texture = _afterViewportSize;
    }

    public void Play(int timeScale)
    {
        _rawImage.texture = _beforeViewportSize;
    }
}
