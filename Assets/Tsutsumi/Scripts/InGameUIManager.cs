using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class InGameUIManager : CancellableComponentBase
{
    [SerializeField, Header("NodeのPrefabを設定してください")] GameObject _nodePrefab;
    [SerializeField, Header("Nodeのデータを設定してください")] NodeData[] _nodeData;
    [SerializeField, Header("手札のゲームオブジェクトを設定してください")] GameObject _hand;
    [SerializeField, Header("タイムラインのノードの制限数を設定してください")]public  int _nodeCount;
    [SerializeField] int _timeScale = 1;
    public int TimeScale
    {
        get => _timeScale;
        set
        {
            _timeScale = value;
            TimeScaleChange?.Invoke(_timeScale);
            // Implement the abstract method from CancellableComponentBase
        }
    }
    public event Action<int> TimeScaleChange;
    [SerializeField] GameObject[] _inGameUI;
    IUserInterface[] _userInterfaces => _inGameUI
        .SelectMany(x => x.GetComponents<IUserInterface>())
        .ToArray();
    void Start()
    {
        BGMManager.Instance.InGameBGM(true);
        TimeScaleChange?.Invoke(_timeScale);
        FindAnyObjectByType<GoalBehaviour>().OnClear +=
        () =>
        {
            BGMManager.Instance.InGameBGM(false);
        };
        Executor.OnExecuteBegin += Hide;
        for (int i = 0; i < _nodeData.Length; i++)
        {
            // _handを親として指定
            var node = Instantiate(_nodePrefab, _hand.transform);

            // RectTransformのリセット
            var rectTransform = node.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.localScale = Vector3.one;
                rectTransform.anchoredPosition3D = Vector3.zero;
                rectTransform.anchorMin = new Vector2(0f, 0.5f);
                rectTransform.anchorMax = new Vector2(0f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
            }

            node.GetComponent<NodeDataContainer>().Init(_nodeData[i]);
            node.GetComponent<NodeMove>().Init(_hand.GetComponent<ITable>()).Forget();
        }
    }
    public void Show()
    {
        TimeScaleManager.ChangeTimeScale(1f);
        foreach (var ui in _userInterfaces)
        {
            ui.UnPlay();
        }
    }
    public void Hide()
    {
        foreach (var ui in _userInterfaces)
        {
            ui.Play(timeScale: _timeScale);
        }
    }
    protected override void OnExecuteCancelled()
    {
        Show();
    }
    void OnDisable()
    {
        BGMManager.Instance.InGameBGM(false);
        TimeScaleManager.ChangeTimeScale(1);
    }
}