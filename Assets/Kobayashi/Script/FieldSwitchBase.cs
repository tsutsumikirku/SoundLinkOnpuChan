using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class FieldSwitchBase : CancellableComponentBase
{
    [Header("switch専用イベント用")]
    [SerializeField] private SwitchGimmickBase[] _switchGimmicks;
    [Header("その他イベント用")]
    [SerializeField] private UnityEvent _onActiveEvent = new UnityEvent();
    [SerializeField] private UnityEvent _onResetEvent = new();
    private bool _toggle;
    protected bool Toggle => _toggle;

    protected void OnActive()
    {
        if (_toggle == true)
        {
            return;
        }
        //CriSEManager.Instance.PlaySE("GimmickSE",0,1,0,true);
        ActiveEvent();
        _toggle = true;
        ActiveButtonAnimation();
    }

    protected override void OnExecuteCancelled()
    {
        OnInactive();
    }

    protected virtual void OnInactive()
    {
        _toggle = false;
        InactiveButtonAnimation();
        InactiveEvent();
    }
    protected virtual void ActiveButtonAnimation()
    {
        ;
    }
    protected virtual void ActiveEvent()
    {
        _onActiveEvent?.Invoke();
        foreach (var switchGimmick in _switchGimmicks)
        {
            switchGimmick.Activate();
        }
    }

    protected virtual void InactiveEvent()
    {
        _onResetEvent?.Invoke();
        foreach (var switchGimmick in _switchGimmicks)
        {
            switchGimmick.Inactive();
        }
    }

    protected virtual void InactiveButtonAnimation()
    {
        ;
    }

    protected virtual void OnDrawGizmos()
    {
        var count = _onActiveEvent.GetPersistentEventCount();
        for (var i =  0; i < count; i++)
        {
            var target = _onActiveEvent.GetPersistentTarget(i);
            if (target is Component comp)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, comp.transform.position);
            }
            else if (target is GameObject go)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, go.transform.position);
            }
        }
    }
}
