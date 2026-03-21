#region

using DG.Tweening;
using UnityEngine;

#endregion

[RequireComponent(typeof(BoxCollider2D))]
public class AttackTriggerSwitch : FieldSwitchBase, IDamageble
{
    void IDamageble.HitDamage()
    {
        OnActive();
    }

    protected override void OnExecuteCancelled()
    {
        base.OnExecuteCancelled();
    }
}