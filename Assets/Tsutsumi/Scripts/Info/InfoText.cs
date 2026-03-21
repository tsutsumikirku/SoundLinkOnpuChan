using System;
using Coffee.UIExtensions;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class InfoText : MonoBehaviour
{
    [SerializeField, Header("テキストコンポーネントをアタッチしてください")] Text _text;
    [SerializeField, Header("テキストが出現するときのパーティクルがあればアタッチしてください")] UIParticle _uIParticle;
    [SerializeField, Header("テキストがフェードで出てくるまでの時間を設定してください")] float _fadeTime;
    Vector2 _beforeSize;
    Tween _tween;
    void Awake()
    {
        _beforeSize = transform.localScale;
        InfoTextData.InfoUpdate = InfoTextUpdate;
    }
    public void InfoTextUpdate(string value)
    {
        _tween?.Kill();
        _text.text = value;
        transform.localScale = new Vector2(transform.localScale.x,0);
        _tween = transform.DOScale(_beforeSize, _fadeTime);
        if (_uIParticle)
		{
			UIParticle particle = Instantiate(_uIParticle,_uIParticle.transform.parent);
			particle.Play();
			Destroy(particle.gameObject, 3f);
		}
    }

}
public static class InfoTextData
{
    public static Action<string> InfoUpdate;
}