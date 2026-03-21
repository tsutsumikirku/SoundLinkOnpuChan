using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

/// <summary>
/// 実行時にノードを読み取り、ノード状態をプレイヤーに送信するクラス。
/// </summary>
public class Executor : CancellableComponentBase
{
    [SerializeField] private Transform _nodeTimeline;
    private List<INode> _nodeList = new();
    private Action<PlayerState, float> OnPlayerStateChanged;
    private Action OnFloorSwitch;
    public static event Action OnExecuteBegin;
    private Player _player;

    CancellationTokenSource _cts;

    void Start()
    {
        _player = FindAnyObjectByType<Player>();
        if (_player != null)
        {
            OnPlayerStateChanged += _player.ChangeState;
        }

        var floorManager = FindAnyObjectByType<FloorSwitchManager>();
        if (floorManager != null)
        {
            OnFloorSwitch += floorManager.SwitchFloor;
        }
    }
    protected override void OnExecuteCancelled()
    {
        CancelNodes();
    }
    public void CancelNodes()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }
    /// <summary>
    /// 実行時のボタン
    /// </summary>
    public void ExecuteButton()
    {
        _cts = new();
        var token = _cts.Token;
        OnExecuteBegin?.Invoke();
        GetNodesFromUI();

        _player.Play(_nodeList);
        //既存のNode発火システムを使わないようにしています。
        //ExecuteNodes(token);
    }
    /// <summary>
    /// ノードをUIのタイムライン上から取得する
    /// </summary>
    private void GetNodesFromUI()
    {
        _nodeList.Clear();
        for (int i = 0; i < _nodeTimeline.childCount; i++)
        {
            _nodeList.Add(_nodeTimeline.GetChild(i).GetComponent<INode>());
        }

    }
    /// <summary>
    /// 取得したノードを順番に処理する。
    /// </summary>
    /// <param name="token"></param>
    private async void ExecuteNodes(CancellationToken token)
    {
        for (int i = 0; i < _nodeList.Count; i++)
        {
            bool isCancelled = false;
            var node = _nodeList[i];
            node.StartNode();

            var type = node.PlayerAction;
            if (type == PlayerState.SwitchVisibility)
            {
                OnFloorSwitch?.Invoke();
            }
            else
            {
                OnPlayerStateChanged?.Invoke(node.PlayerAction, node.Power);
                if (type == PlayerState.Jump)
                {
                    isCancelled = await UniTask.WaitUntil(() => default /*_player.IsGround*/, cancellationToken: token)
                        .SuppressCancellationThrow();
                }
                else
                {
                    isCancelled = await UniTask.Delay((int)(1000 * node.Second), cancellationToken: token)
                        .SuppressCancellationThrow();
                }
            }

            node.EndNode();

            if (isCancelled) break;
        }

        // ノードの処理をすべて終えたらプレイヤーを待機状態にする。
        OnPlayerStateChanged?.Invoke(PlayerState.Stop, 1);
    }
    private void OnDisable()
    {
        OnExecuteBegin = null;
    }
}
