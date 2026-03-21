using UnityEngine;

/// <summary>
/// SafeAreaクラスは、UIの安全領域を管理するためのクラスです。
/// </summary>

[RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour
{
    void Awake()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        Rect deviceSafeArea = Screen.safeArea;

        Vector2 anchorMin = deviceSafeArea.position;
        Vector2 anchorMax = deviceSafeArea.position + deviceSafeArea.size;

        anchorMin.x /= screenSize.x;
        anchorMax.x /= screenSize.x;
        anchorMin.y /= screenSize.y;
        anchorMax.y /= screenSize.y;

        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = Vector2.zero;

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
    }
}
