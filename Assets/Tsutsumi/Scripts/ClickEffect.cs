using Coffee.UIExtensions;
using UnityEngine;
public class ClickEffect : MonoBehaviour
{
    [SerializeField,Header("クリックエフェクトのPrefabをアタッチしてください")]UIParticle _clickEffect;
    void LateUpdate()
    {
        if (!_clickEffect) return;
        if (Input.GetMouseButtonDown(0))
        {
            UIParticle effect = Instantiate(_clickEffect, FindAnyObjectByType<Canvas>().transform);
            effect.transform.SetAsLastSibling();
            effect.transform.position = Input.mousePosition;
            effect.Play();
            Destroy(effect.gameObject, 5f);
        }
    }
}