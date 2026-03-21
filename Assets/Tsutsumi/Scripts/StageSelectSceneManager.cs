using System;
using UnityEngine;
using DG.Tweening;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class StageSelectSceneManager : MonoBehaviour
{
    [SerializeField, Header("ステージセレクトのボタンのPrefabを設定してください")]
    private StageSelectButton _button;

    [SerializeField, Header("ステージのスクロールビューのコンテンツを設定してください")]
    private GameObject _scrollViewContent;

    [SerializeField, Header("ステージのデータを設定してください")]
    private StageSelectData[] _stageSelectDatas;

    [SerializeField, Header("スタートボタンを選択してください")]
    private UIButton _startButton;

    [SerializeField, Header("シーンローダーを設定してください")]
    private SceneLoader _sceneLoader;

    [SerializeField, Header("スターのテキストを設定してください")]
    private TextMeshProUGUI _starCountText;

    [Header("素材関連")]

    [SerializeField, Header("ステージのボタン(未開放)を設定してください")]
    private Sprite _stageButtonlock;

    [SerializeField, Header("ステージのボタン(開放)を設定してください")]
    private Sprite _stageButtonUnlock;

    [SerializeField, Header("ステージのボタン(選択中)を設定してください")]
    private Sprite _stageButtonSelect;
    private StageSelectState _beforeState;

    [SerializeField]private ScrollRect _scrollRect;

    private int _starCount = 0;
    private Action _openAction;

    // 開始時に
    // SceneNameDatasの参照するためのstringは SceneNameDatasである
    void Awake()
    {
        BGMManager.Instance.StageSelectBGM(true);
        _beforeState = StageSelectState.Clear;
        SceneNameDatas sceneNameDatas = new SceneNameDatas();
        for (int i = 0; i < _stageSelectDatas.Length; i++)
        {
            var button = Instantiate(_button, _scrollViewContent.transform);
            var state = JsonSave.Load<StageSelectState>(_stageSelectDatas[i].SceneName + "StageSelect");
            if (!_stageSelectDatas[i].CanPlayStage)
            {
                _beforeState = StageSelectState.None;
            }
            if (state == default)
            {
                state = StageSelectState.None;
            }
            switch (state)
            {
                // ステージがクリアされていない場合
                case StageSelectState.None:
                    if (_stageSelectDatas[i].CanPlayStage)
                    {
                        sceneNameDatas._sceneNames.Add(_stageSelectDatas[i].SceneName);
                    }
                    if (_beforeState == StageSelectState.None)
                    {
                        button.ButtonImage.sprite = _stageButtonlock;
                        var uiButton = button.GetComponent<UIButton>();
                        Destroy(uiButton);
                    }
                    else
                    {
                        button.ButtonImage.sprite = _stageButtonUnlock;
                        int a = i;
                        button.Button.OnClick = () =>
                        {
                            StartButtonSet(() => button.ButtonImage.sprite = _stageButtonUnlock, _stageSelectDatas[a].SceneName);
                            button.ButtonImage.sprite = _stageButtonSelect;
                        };
                    }
                    _beforeState = StageSelectState.None;
                    break;
                // ステージがクリアされている場合
                case StageSelectState.Clear:
                    sceneNameDatas._sceneNames.Add(_stageSelectDatas[i].SceneName);
                    button.ButtonImage.sprite = _stageButtonUnlock;
                    int ab = i;
                    button.Button.OnClick = () =>
                    {
                        StartButtonSet(() => button.ButtonImage.sprite = _stageButtonUnlock, _stageSelectDatas[ab].SceneName);
                        button.ButtonImage.sprite = _stageButtonSelect;
                    };
                    _beforeState = StageSelectState.Clear;
                    break;
                // スターを獲得してクリアされている場合
                case StageSelectState.StarClear:
                    sceneNameDatas._sceneNames.Add(_stageSelectDatas[i].SceneName);
                    _starCount++;
                    button.ButtonImage.sprite = _stageButtonUnlock;
                    int abc = i;
                    button.Button.OnClick = () =>
                    {
                        StartButtonSet(() => button.ButtonImage.sprite = _stageButtonUnlock, _stageSelectDatas[abc].SceneName);
                        button.ButtonImage.sprite = _stageButtonSelect;
                    };
                    button.StarCoinImage.enabled = true;
                    _beforeState = StageSelectState.StarClear;
                    break;
            }
            string name;
            if (_stageSelectDatas[i].StageName == "Lv")
            {
                name = "Lv." + (i + 1);
            }
            else name = _stageSelectDatas[i].StageName;
            button.Text.text = name;
            var stageName = name;
            StageNameData stage = new StageNameData() { _stageName = stageName };
            JsonSave.Save(_stageSelectDatas[i].SceneName + "StageName", stage);
        }
        _starCountText.text = $"{_starCount}";
        JsonSave.Save("SceneNames", sceneNameDatas);

        if (JsonSave.TryLoad<ScrollData>("StageSelect", out var value))
        {
            _scrollRect.verticalNormalizedPosition = value._scrrollValue;
        }
        
    }
    public void ScrollUpdate(Vector2 vector)
    {
        float value = _scrollRect.verticalNormalizedPosition;
        ScrollData scrollData = new ScrollData() { _scrrollValue = value };
        JsonSave.Save("StageSelect",scrollData);
    }

    void StartButtonSet(Action openAction, string sceneName)
    {
        _openAction?.Invoke();
        _openAction = openAction;
        _startButton.gameObject.SetActive(true);
        var defaultSize = _startButton.transform.localScale;
        _startButton.transform.localScale = Vector3.zero;
        _startButton.transform.DOScale(defaultSize, 0.2f);
        _startButton.OnClick = () =>
        {
            _sceneLoader.LoadScene(sceneName);
        };
    }

    /// <summary>
    /// データをリセットする
    /// </summary>
    public void DataReset()
    {

    }
    void OnDisable()
    {
        BGMManager.Instance.StageSelectBGM(false);
    }
}

public enum StageSelectState
{
    None,
    Clear,
    StarClear
}

[System.Serializable]
public class StageSelectData
{
    public string SceneName;
    public string StageName;
    public bool CanPlayStage = true;
}
[System.Serializable]
public class StageNameData
{
    public string _stageName;
}
[System.Serializable]
public class SceneNameDatas
{
    public List<string> _sceneNames = new List<string>();
}

[System.Serializable]
public class ScrollData
{
    public float _scrrollValue;
}