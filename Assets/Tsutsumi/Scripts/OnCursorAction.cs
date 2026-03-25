using UnityEngine;

public class OnCursorAction : MonoBehaviour
{
    [SerializeField]NewScrollView scrollView;
    [SerializeField] float scrollValue;

    public void OnCursorEnter()
    {
        scrollView.Scroll(scrollValue);
    }
}
