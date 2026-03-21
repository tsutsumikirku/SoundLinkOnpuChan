using UnityEngine;

public class CursolChanger : MonoBehaviour
{
    public static CursolChanger Instance { get; private set; }
    CursorType _cursolType = CursorType.Normal;
    public CursorType CursorType { get => _cursolType; set => ChangeCursor(value); }
    [SerializeField] Texture2D _normalCursors;
    [SerializeField] Texture2D _handCursors;
    [SerializeField] Texture2D _dragCursors;
    void ChangeCursor(CursorType cursorType)
    {
        switch (cursorType)
        {
            case CursorType.Normal:
                Cursor.SetCursor(_normalCursors, Vector2.zero, CursorMode.Auto);
                break;
            case CursorType.Hand:
                Cursor.SetCursor(_handCursors, Vector2.zero, CursorMode.Auto);
                break;
            case CursorType.Drag:
                Cursor.SetCursor(_dragCursors, Vector2.zero, CursorMode.Auto);
                break;
        }
    }
    void Awake()
    {
        CursorType = CursorType.Normal;
        Instance = this;
    }
}
public enum CursorType
{
    Normal,
    Hand,
    Drag
}
