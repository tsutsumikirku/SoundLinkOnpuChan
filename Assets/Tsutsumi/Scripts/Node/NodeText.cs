using UnityEngine;
using TMPro;
public class NodeText : MonoBehaviour
{
    [SerializeField, Header("テキスト")] TextMeshProUGUI _text;
    [SerializeField, Header("yoffset")] float _yAligine;
    string _nameText;
    GameObject _nodeObject;
    public void Init(GameObject nodeObj, string text)
    {
        _nodeObject = nodeObj;
        _nameText = text;
        _text.text = _nameText;
    }
    private void LateUpdate()
    {
        _text.enabled = true;
        // Canvasの取得を親から
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null || _nodeObject == null) return;
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        // nodeObjectがRectTransformを持つか確認
        Vector3 worldPos = _nodeObject.transform.position;
        RectTransform nodeRect = _nodeObject.GetComponent<RectTransform>();
        if (nodeRect != null)
        {
            worldPos = nodeRect.position;
        }
        // スクリーン座標に変換
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            worldPos
        );
        // Canvasローカル座標に変換
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, screenPos,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out localPoint
        );
        // UIオブジェクトの位置を設定
        ((RectTransform)transform).anchoredPosition = localPoint + new Vector2(0, _yAligine);
        if (_nodeObject.transform.GetComponentInParent<TimeLine>())
        {
            _text.enabled = false;
        }
    }
}
