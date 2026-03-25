using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class Scroll : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] private NewScrollView timeLineScript;

    public void OnDrag(PointerEventData eventData)
    {
        timeLineScript.OnDrag(eventData);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        timeLineScript.OnBeginDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        timeLineScript.OnEndDrag(eventData);
    }
}
