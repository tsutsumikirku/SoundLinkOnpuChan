using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIPush : MonoBehaviour,IPointerDownHandler, IPointerUpHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        NumericInput.IsInputting = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Reset the input state when the pointer is released
    }
}
