using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class SwitchFloorCommand : PlayerCommand
{
    FloorSwitchManager _floorSwitch;
    public override PlayerState[] States => new []{PlayerState.SwitchVisibility};

    private void Awake()
    {
        _floorSwitch = FindAnyObjectByType<FloorSwitchManager>();
        if (_floorSwitch == null)
        {
            Debug.LogWarning("floorSwichManager is null");
        }
    }

    public override UniTask Init(Player player, INode node = null, CancellationToken token = default, Action action = null)
    {
        _floorSwitch?.SwitchFloor();
        player.ChangeState();
        return UniTask.CompletedTask;
    }

    public override void CommandFixedUpdate()
    {
        ;
    }
    public override UniTask Exit()
    {
        return UniTask.CompletedTask;
    }
}
