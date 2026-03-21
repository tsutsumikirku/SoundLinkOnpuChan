using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIInfoSet : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField,Header("Infoに出力するテキストを設定してください")] public string _text;
    public void OnPointerEnter(PointerEventData eventData)
    {
        InfoTextData.InfoUpdate?.Invoke(_text);
    }
}
