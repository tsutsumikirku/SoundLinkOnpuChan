using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private Image _fadePanel;
    [SerializeField] private float _fadeSpeed;
    [SerializeField] private bool _center;
    [SerializeField] private bool _startFade = true;
    private Vector2 _defaultScale;
    private void Start()
    {
        if (_fadePanel == null)
        {
            Debug.LogWarning("FadePanelが未割当です。");
            return;
        }
        _defaultScale = _fadePanel.transform.localScale;
        if (!_startFade) return;
        _fadePanel.gameObject.SetActive(true);
        ((RectTransform)_fadePanel.transform).anchoredPosition = Vector2.zero;
        _fadePanel.transform.DOScale(0, _fadeSpeed).OnComplete(() => _fadePanel.gameObject.SetActive(false));
        // switch (sceneNmae)
        // // {
        // //     case "StageSelect":
        // //         SoundManager.Instance.PlayBGM(BGMSoundData.BGM.StageSelect);
        // //         break;
        // //     case "Title":
        // //         SoundManager.Instance.PlayBGM(BGMSoundData.BGM.Title);
        // //         break;
        // //     default:
        // //         SoundManager.Instance.PlayBGM(BGMSoundData.BGM.Ingame);
        // //         break;
        //}
    }

    public void LoadScene(string sceneName)
    {
        if (_fadePanel == null)
        {
            Debug.LogWarning("FadePanelが未割当です。");
            return;
        }
        //SoundManager.Instance.PlaySE(SESoundData.SE.Select);
        _fadePanel.gameObject.SetActive(true);
        //SoundManager.Instance.FadeBGMSound(_fadeSpeed);
        if (!_center)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)_fadePanel.transform.parent,   // 親のRectTransform
                Input.touches[0].position,                    // タッチ位置（スクリーン座標）
                null,                                         // カメラ（CanvasがScreen Space OverlayならnullでOK）
                out localPoint
            );

            ((RectTransform)_fadePanel.transform).anchoredPosition = localPoint;
        }
        _fadePanel.transform.localScale = Vector3.zero;
        _fadePanel.transform.DOScale(_defaultScale, _fadeSpeed).OnComplete(() => SceneManager.LoadScene(sceneName));
    }

    public void RetryCurrentScene()
    {
        var currentScene = SceneManager.GetActiveScene().name;
        LoadScene(currentScene);
    }
    public void ReloadCurrentScene()
    {
        var sceneName = SceneManager.GetActiveScene().name;
        LoadScene(sceneName);
    }
}
