using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.EventSystems;
/// <summary>
/// このクラスがアタッチされているオブジェクトは状態ごとのマウスカーソルの変更を設定することができます。
/// </summary>
public class UICursolChange : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler,IDragHandler
{
    [SerializeField,Header("このUIのマウスカーソルが乗った時")] CursorType _pointerEnter;
    [SerializeField, Header("このUIが押されたとき(マウスボタンを押している)")] CursorType _pointerDown;
    [SerializeField, Header("このUIがドラッグされているとき")] CursorType _onDrag;
    [SerializeField, Header("このUIが押されたとき(マウスボタンを離している)")] CursorType _pointerUp;

    //マウスが押されたとき
    public void OnPointerDown(PointerEventData eventData)
    {
        #if Windows || UNITY_STANDALONE
        CursolChanger.Instance.CursorType = _pointerDown;
        #endif 
    }
    //マウスがボタンが離れたとき
    public void OnPointerUp(PointerEventData eventData)
    {
        #if Windows || UNITY_STANDALONE
        CursolChanger.Instance.CursorType = _pointerUp;
        #endif
    }
    //ポインターがこのオブジェクトの上に載ったとき
    public void OnPointerEnter(PointerEventData eventData)
    {
        #if Windows || UNITY_STANDALONE
        CursolChanger.Instance.CursorType = _pointerEnter;
        #endif
    }
    //このポインターがこのオブジェクトから離れたとき

    public void OnPointerExit(PointerEventData eventData)
    {
        #if Windows || UNITY_STANDALONE
        CursolChanger.Instance.CursorType = CursorType.Normal;
        #endif
    }

    public void OnDrag(PointerEventData eventData)
    {
        #if Windows || UNITY_STANDALONE
        CursolChanger.Instance.CursorType = _onDrag;
        #endif
    }
}