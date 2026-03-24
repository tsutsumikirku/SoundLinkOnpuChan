using UnityEngine;
using UnityEngine.EventSystems;


// これは僕たちが最高のスクロールビューになるまでの物語だ --緑谷出久
public class NewScrollView : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    float parentWidth;
    float defaultY;
    RectTransform rectTransform;
    RectTransform parentRectTransform;

    // inertia variables
    bool dragging = false;
    float velocity = 0f; // pixels per second
    [SerializeField] float deceleration = 3000f; // pixels per second^2
    [SerializeField] float minVelocity = 20f; // below this we stop

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentRectTransform = transform.parent as RectTransform;
        UpdateParentWidth();
        defaultY = rectTransform.anchoredPosition.y;
        rectTransform.anchoredPosition = new Vector2(0, defaultY);
    }

    void Update()
    {
        if (!dragging)
        {
            if (Mathf.Abs(velocity) > 0f)
            {
                // apply inertia
                float delta = velocity * Time.unscaledDeltaTime;
                rectTransform.anchoredPosition += new Vector2(delta, 0);

                // decelerate
                float decel = deceleration * Time.unscaledDeltaTime;
                velocity = Mathf.MoveTowards(velocity, 0f, decel);

                // stop if very small
                if (Mathf.Abs(velocity) < minVelocity)
                    velocity = 0f;

                ClampToBounds();
            }
        }
    }

    void OnRectTransformDimensionsChange()
    {
        if (rectTransform == null)
            return;

        UpdateParentWidth();
        ClampToBounds();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragging = true;
        velocity = 0f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // move by drag delta
        rectTransform.anchoredPosition += new Vector2(eventData.delta.x, 0);

        // calculate instantaneous velocity (pixels per second)
        if (Time.unscaledDeltaTime > 0f)
            velocity = eventData.delta.x / Time.unscaledDeltaTime;

        ClampToBounds();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        dragging = false;
        // velocity already set in OnDrag; inertia will be applied in Update
    }

    void UpdateParentWidth()
    {
        if (parentRectTransform == null)
            parentRectTransform = transform.parent as RectTransform;

        parentWidth = parentRectTransform != null ? parentRectTransform.rect.width : 0f;
    }

    void ClampToBounds()
    {
        float minX = Mathf.Min(0f, parentWidth - rectTransform.rect.width);
        float clampedX = Mathf.Clamp(rectTransform.anchoredPosition.x, minX, 0f);

        if (!Mathf.Approximately(clampedX, rectTransform.anchoredPosition.x))
        {
            rectTransform.anchoredPosition = new Vector2(clampedX, defaultY);
            velocity = 0f;
        }
    }
}
