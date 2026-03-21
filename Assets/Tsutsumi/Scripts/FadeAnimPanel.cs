using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;

public class FadeAnimPanel : MonoBehaviour
{
    [SerializeField] float _animationTime;
    // Start is called before the first frame update
    void Start()
    {
        Image image = GetComponent<Image>();
        image.DOFade(0f, _animationTime);
    }
}
