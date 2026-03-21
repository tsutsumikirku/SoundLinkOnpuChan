using UnityEngine;
using UnityEngine.EventSystems;


// これは僕たちが最高のスクロールビューになるまでの物語だ --緑谷出久
public class NewScrollView : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    float parentWidth;
    float defaultY;
    RectTransform rectTransform;

    // inertia variables
    bool dragging = false;
    float velocity = 0f; // pixels per second
    [SerializeField] float deceleration = 3000f; // pixels per second^2
    [SerializeField] float minVelocity = 20f; // below this we stop

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentWidth = ((RectTransform)transform.parent).rect.width;
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

                // clamp to bounds
                float minX = -rectTransform.rect.width + parentWidth;
                if (rectTransform.anchoredPosition.x > 0f)
                {
                    rectTransform.anchoredPosition = new Vector2(0f, defaultY);
                    velocity = 0f;
                }
                else if (rectTransform.anchoredPosition.x < minX)
                {
                    rectTransform.anchoredPosition = new Vector2(minX, defaultY);
                    velocity = 0f;
                }
            }
        }
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

        // clamp while dragging as before
        if (rectTransform.anchoredPosition.x > 0)
        {
            rectTransform.anchoredPosition = new Vector2(0, defaultY);
            velocity = 0f;
        }
        else if (rectTransform.anchoredPosition.x < -rectTransform.rect.width + parentWidth)
        {
            rectTransform.anchoredPosition = new Vector2(-rectTransform.rect.width + parentWidth, defaultY);
            velocity = 0f;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        dragging = false;
        // velocity already set in OnDrag; inertia will be applied in Update
    }
}
