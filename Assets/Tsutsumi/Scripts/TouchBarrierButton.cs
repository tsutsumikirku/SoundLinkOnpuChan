using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TouchBarrierButton : CancellableComponentBase
{
    [SerializeField] private Image _panel;

    private void Start()
    {
        Executor.OnExecuteBegin += () => _panel.raycastTarget = true;
    }
    protected override void OnExecuteCancelled()
    {
        _panel.raycastTarget = false;
    }
}
