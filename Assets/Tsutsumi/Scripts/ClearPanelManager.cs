using System;
using System.Threading;
using Coffee.UIExtensions;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ClearPanelManager : MonoBehaviour
{
    // ステージ設定用
    [Header("ボタンの設定")][SerializeField, Header("次のステージのボタンを設定して下さい")] UIButton _nextStageButton;
    [SerializeField, Header("SceneLoaderを設定してください")] SceneLoader _sceneLoader;
    [SerializeField, Header("ステージのテキストを指定してください")] TextMeshProUGUI _stageTex;
    [SerializeField, Header("ステージの吹き出しを設定してください")] Image _nextStageSerif;
    [SerializeField, Header("ステージの吹き出しのテキストを設定してください")] TextMeshProUGUI _nextStageText;
    [SerializeField, Header("ステージの吹き出しが出てくる時間を設定してください")] float _serifAnimationTime;
    [Space(10)]

    // 基本設定
    [Header("基本設定")]
    [SerializeField, Header("アニメーションの時間を設定してください")] float _animationTime = 1.0f;
    [SerializeField, Header("背景のImageを設定してください")] Image _backgroundImage;
    [SerializeField, Header("TMPを設定して下さい")] TextMeshProUGUI _text;
    [Space(10)]

    //アニメーション
    [Header("アニメーション関連")]
    [SerializeField, Header("クリアのアニメーターを設定してください")] Animator _clearAnimator;
    [SerializeField, Header("再生するSEのキューネームを設定してください")] string _seCueName = "";
    [SerializeField, Header("普通のクリアの際のBool名を設定してください")] string _normalClearBoolName = "NormalClear";
    [SerializeField, Header("完璧なクリアの際のBool名を設定してください")] string _perfectClearBoolName = "PerfectClear";
    [Space(10)]

    //画面左に表示されるおんぷちゃん
    [Header("おんぷちゃん関連")]
    [SerializeField, Header("おんぷちゃんのRectTransformを設定してください")] RectTransform _onpuRect;
    [SerializeField, Header("動き出す前のおんぷちゃんの位置")] Vector2 _beforeOnpuPos;
    [SerializeField, Header("おんぷちゃんを出力するImageを設定してください")] Image _onpuImage;
    [SerializeField, Header("普通のクリアの際のSpriteを設定してください")] Sprite _normalClearSprite;
    [SerializeField, Header("完璧なクリアの際のSpriteを設定してください")] Sprite _perfectClearSprite;

    [Space(10)]

    //画面右に表示されるパネル
    [Header("パネル関連")]
    [SerializeField, Header("パネルのImageを設定してください")] Image _panelImage;
    [SerializeField, Header("パネルのRectTransformを設定してください")] RectTransform _panelRect;
    [SerializeField, Header("パネルの移動前の位置を設定してください")] Vector2 _beforePanelPos;
    [Space(10)]

    // メダル関係
    [Header("メダル関連")]
    [SerializeField, Header("メダルを出力するImageを設定してください")] RawImage _medalImage;
    [SerializeField, Header("メダルのRectTransformを設定してください")] RectTransform _medalRect;
    [SerializeField, Header("メダルのSpriteを設定してください")] Texture _medalSprite;
    [SerializeField, Header("メダルを獲得している時のSpriteを設定してください")] Texture _haveMedalSprite;
    [SerializeField, Header("メダルを獲得したときのアニメーションの際のスケール")] float _medalGetAnimScale;
    [SerializeField, Header("メダルを獲得したといのアニメーションにかかる時間")] float _medalGetAnimTime;
    [SerializeField, Header("メダルをゲットしたときの光")] Image _medalGetLight;
    [SerializeField, Header("メダルのパーティクル")] UIParticle _medalParticle;
    [SerializeField, Header("メダルを取得しているときのチェック")] Image _chackImage;
    [SerializeField, Header("メダルを獲得しているときのチェックのRectTransform")] RectTransform _chackRect;
    [SerializeField, Header("メダルを獲得したときのチェックアニメーションの際のスケール")] float _chackAnimScale;
    [SerializeField, Header("メダルを獲得したときのチェックのアニメーションにかかる時間")] float _chackAnimTime;
    [SerializeField, Header("メダルをゲットしたときのテキスト")] TextMeshProUGUI _medalGetText;
    [Space(10)]
    [SerializeField] GameObject _panel;

    // ステージのクリア条件を満たしているか

    [SerializeField, Header("TimeLineの取得")] TimeLine _timeLine;

    StageSelectState _stageSelectDatas;
    CancellationTokenSource _cts;
    string nextScene;

    // 初期化処理にてこのステージのステージセレクトの状態を取得
    public void Start()
    {
        int index = JsonSave.Load<SceneNameDatas>("SceneNames")._sceneNames.IndexOf(SceneManager.GetActiveScene().name);
        index++;
        if (JsonSave.Load<SceneNameDatas>("SceneNames")._sceneNames.Count <= index)
        {
            nextScene = null;
        }
        else
        {
            nextScene = JsonSave.Load<SceneNameDatas>("SceneNames")._sceneNames[index];
        }
        if (nextScene == null)
        {
            _nextStageButton.gameObject.SetActive(false);
        }
        else
        {
            _nextStageButton.OnClick = () =>
            {
                _sceneLoader.LoadScene(nextScene);
            };
        }
        _stageSelectDatas = JsonSave.Load<StageSelectState>(SceneManager.GetActiveScene().name + "StageSelect");
        if (_stageSelectDatas == default)
        {
            _stageSelectDatas = StageSelectState.None;
        }
        _text.text = _text.text.Replace("〇", _timeLine._maxNodeCount.ToString());
        _cts = new CancellationTokenSource();
    }

    public async UniTask ClearSetUp()
    {
        TimeScaleManager.ChangeTimeScale(1);
        FindAnyObjectByType<SettingUIPanel>().Hide(false);
        // もしも完璧なクリアだったなら
        if (_timeLine.isParfect())
        {
            JsonSave.Save(SceneManager.GetActiveScene().name + "StageSelect", StageSelectState.StarClear);
            // 背景を描画する
            _backgroundImage.gameObject.SetActive(true);
            Color perfectcolor = _backgroundImage.color;
            perfectcolor.a = 0;
            float alpha = _backgroundImage.color.a;
            _backgroundImage.color = perfectcolor;
            _backgroundImage.DOFade(alpha, 0.5f);

            // クリアアニメーションを再生する
            _clearAnimator.SetBool("Complete", true);
            // クリアのSEを鳴らす
            if (_seCueName != "")
            {
                CriSEManager.Instance.PlaySE(_seCueName);
            }
            await UniTask.WaitForSeconds(1f, cancellationToken: _cts.Token);
            _clearAnimator.SetBool("Complete", false);
            await UniTask.WaitForSeconds(_animationTime, cancellationToken: _cts.Token);
            BGMManager.Instance.ResultBGM(true);
            // 音符ちゃんを表示し左から出す
            _onpuImage.enabled = true;
            _onpuImage.sprite = _perfectClearSprite;
            var defaultRect = _onpuRect.anchoredPosition;
            _onpuRect.anchoredPosition = _beforeOnpuPos + defaultRect;
            _onpuRect.DOAnchorPos(defaultRect, 1f);
            await UniTask.WaitForSeconds(0.5f, cancellationToken: _cts.Token);

            // パネルを表示し右から出す
            _panelImage.gameObject.SetActive(true);
            _stageTex.text = JsonSave.Load<StageNameData>(SceneManager.GetActiveScene().name + "StageName")._stageName;
            var defaultPanelPos = _panelRect.anchoredPosition;
            _panelRect.anchoredPosition = _beforePanelPos + defaultPanelPos;
            if (_stageSelectDatas == StageSelectState.StarClear)
            {
                _chackImage.enabled = true;
            }
            await _panelRect.DOAnchorPos(defaultPanelPos, 0.8f).AsyncWaitForCompletion();

            // チェックのアニメーションもしも初めて獲得したら
            if (_stageSelectDatas != StageSelectState.StarClear)
            {
                _chackImage.enabled = true;
                var defaultScale = _chackRect.localScale;
                _chackRect.localScale = defaultScale * _chackAnimScale;
                var color = _chackImage.color;
                color.a = 0;
                _chackImage.color = color;
                _chackRect.DOScale(defaultScale, _chackAnimTime);
                await _chackImage.DOFade(1f, _chackAnimTime).AsyncWaitForCompletion();
            }

            //メダルのアニメーション
            _medalImage.gameObject.SetActive(true);
            _medalParticle.Play();
            _medalImage.enabled = true;
            _medalImage.texture = _haveMedalSprite;
            Color medalColor = _medalImage.color;
            medalColor.a = 0;
            _medalImage.color = medalColor;
            _medalImage.DOFade(1, 0.5f);
            Vector2 defaultMedalScale = _medalRect.localScale;
            _medalRect.localScale = defaultMedalScale * _medalGetAnimScale;
            _medalImage.DOFade(1f, _medalGetAnimTime);
            await _medalRect.DOScale(defaultMedalScale, _medalGetAnimTime).AsyncWaitForCompletion();
            CriSEManager.Instance.PlaySE("SE_InGame_Result_Coin", playOneShot: true);
            _medalGetLight.enabled = true; await _medalGetLight.DOFade(0.8f, 0.2f)
                .SetEase(Ease.OutQuad)
                .AsyncWaitForCompletion();
            await UniTask.Delay(80, cancellationToken: _cts.Token);
            await _medalGetLight.DOFade(0, 0.2f)
                .SetEase(Ease.InQuad)
                .AsyncWaitForCompletion();
            _medalGetLight.enabled = false;
            if (_stageSelectDatas != StageSelectState.StarClear)
            {
                _medalGetText.enabled = true;
                _medalGetText.text = "";
                await UniTask.WaitForSeconds(0.5f, cancellationToken: _cts.Token);
                CriSEManager.Instance.PlaySE("SE_InGame_Node_Have", playOneShot: true);
                _medalGetText.text = "G";
                await UniTask.WaitForSeconds(0.5f, cancellationToken: _cts.Token);
                CriSEManager.Instance.PlaySE("SE_InGame_Node_Have", playOneShot: true);
                _medalGetText.text = "GE";
                await UniTask.WaitForSeconds(0.5f, cancellationToken: _cts.Token);
                CriSEManager.Instance.PlaySE("SE_InGame_Node_Have", playOneShot: true);
                _medalGetText.text = "GET";
            }
        }
        // もしも普通のクリアだったなら
        else
        {
            if (_stageSelectDatas == StageSelectState.None)
            {
                JsonSave.Save(SceneManager.GetActiveScene().name + "StageSelect", StageSelectState.Clear);
            }
            // 背景を描画する
            _backgroundImage.gameObject.SetActive(true);
            Color color = _backgroundImage.color;
            float alpha = _backgroundImage.color.a;
            color.a = 0;
            _backgroundImage.DOFade(alpha, 0.5f);

            // クリアアニメーションを再生する
            _clearAnimator.SetBool("NormalClear", true);
            // クリアのSEを鳴らす
            if (_seCueName != "")
            {
                CriSEManager.Instance.PlaySE(_seCueName);
            }
            await UniTask.WaitForSeconds(1f, cancellationToken: _cts.Token);
            _clearAnimator.SetBool("NormalClear", false);
            await UniTask.WaitForSeconds(_animationTime, cancellationToken: _cts.Token);
            BGMManager.Instance.ResultBGM(true);
            // 音符ちゃんを表示し左から出す
            _onpuImage.enabled = true;
            _onpuImage.sprite = _normalClearSprite;
            var defaultRect = _onpuRect.anchoredPosition;
            _onpuRect.anchoredPosition = _beforeOnpuPos + defaultRect;
            _onpuRect.DOAnchorPos(defaultRect, 1f);
            await UniTask.WaitForSeconds(0.5f, cancellationToken: _cts.Token);

            // パネルを表示し右から出す
            _panelImage.gameObject.SetActive(true);
            _stageTex.text = JsonSave.Load<StageNameData>(SceneManager.GetActiveScene().name + "StageName")._stageName;
            var defaultPanelPos = _panelRect.anchoredPosition;
            _panelRect.anchoredPosition = _beforePanelPos + defaultPanelPos;
            if (_stageSelectDatas == StageSelectState.StarClear)
            {
                _chackImage.enabled = true;
            }
            await _panelRect.DOAnchorPos(defaultPanelPos, 0.8f).AsyncWaitForCompletion();
            // すでにクリア済みの場合は金色メダル
            if (_stageSelectDatas == StageSelectState.StarClear)
            {
                 //メダルのアニメーション
            _medalImage.gameObject.SetActive(true);
            _medalParticle.Play();
            _medalImage.enabled = true;
            _medalImage.texture = _haveMedalSprite;
            Color medalColor = _medalImage.color;
            medalColor.a = 0;
            _medalImage.color = medalColor;
            _medalImage.DOFade(1, 0.5f);
            Vector2 defaultMedalScale = _medalRect.localScale;
            _medalRect.localScale = defaultMedalScale * _medalGetAnimScale;
            _medalImage.DOFade(1f, _medalGetAnimTime);
            await _medalRect.DOScale(defaultMedalScale, _medalGetAnimTime).AsyncWaitForCompletion();
            CriSEManager.Instance.PlaySE("SE_InGame_Result_Coin", playOneShot: true);
            _medalGetLight.enabled = true; await _medalGetLight.DOFade(0.8f, 0.2f)
                .SetEase(Ease.OutQuad)
                .AsyncWaitForCompletion();
            await UniTask.Delay(80, cancellationToken: _cts.Token);
            await _medalGetLight.DOFade(0, 0.2f)
                .SetEase(Ease.InQuad)
                .AsyncWaitForCompletion();
            _medalGetLight.enabled = false;
            }
        }
        if (nextScene == null) return;
            _nextStageSerif.gameObject.SetActive(true);
            var defaultSerifSize = _nextStageSerif.transform.localScale;
            _nextStageSerif.transform.localScale = Vector2.zero;
            _nextStageSerif.transform.DOScale(defaultSerifSize, _serifAnimationTime);
            _nextStageText.text = _nextStageText.text.Replace("◯", JsonSave.Load<StageNameData>(nextScene + "StageName")._stageName);
    }
    void OnDisable()
    {
        BGMManager.Instance.ResultBGM(false);
    }
}
