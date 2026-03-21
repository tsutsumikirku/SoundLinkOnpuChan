using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SettingUIPanel : MonoBehaviour
{
    [SerializeField] UIButton _settingButton;
    [SerializeField] Image _background;
    [SerializeField] Button _closeButton;
    [SerializeField] private UIButton _closeUIButton;
    [SerializeField] Image _settingPanel;
    [SerializeField] float _panelVisibleTime = 0.5f;
    [SerializeField] Slider _bgmSlider;
    [SerializeField] Slider _seSlider;
    private Vector2 _defaultPos;
    private void Start()
    {
        _settingPanel.gameObject.SetActive(false);
        _defaultPos = ((RectTransform)_settingPanel.transform).anchoredPosition;
        _settingButton.OnClick += Show;
        _closeButton.onClick.AddListener(() => Hide(true));
        _closeUIButton.OnClick += () => Hide(true);
        Hide(false);
        SliderSet().Forget();
    }
    async UniTask SliderSet()
    {
        await UniTask.Yield();
        _bgmSlider.value = BGMManager.Instance.BGMVolume;
        _bgmSlider.onValueChanged.AddListener((value) => BGMManager.Instance.BGMVolume = value);
        _seSlider.value = BGMManager.Instance.SEVolume;
        _seSlider.onValueChanged.AddListener((value) => BGMManager.Instance.SEVolume = value);
    }

    public void SliderUpdate()
    {
        BGMManager.Instance.NotInit();
        _bgmSlider.value = BGMManager.Instance.BGMVolume;
        _seSlider.value = BGMManager.Instance.SEVolume;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_background.gameObject.activeSelf)
            {
                Hide(true);
            }
            else
            {
                Show();
            }
        }
    }
    public void Show()
    {
        _background.gameObject.SetActive(true);
        _settingPanel.gameObject.SetActive(true);
        ((RectTransform)_settingPanel.transform).DOAnchorPos(_defaultPos, _panelVisibleTime * TimeScaleManager.InGameTimeScale);
    }
    public void Hide(bool isPlaySE)
    {
        if(isPlaySE)
        CriSEManager.Instance.PlaySE("SE_InGame_InputScreen_Disapperar",playOneShot: true);
        _background.gameObject.SetActive(false);
        ((RectTransform)_settingPanel.transform).DOAnchorPosX(2500, _panelVisibleTime * TimeScaleManager.InGameTimeScale);
    }
}