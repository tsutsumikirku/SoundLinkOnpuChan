using UnityEngine;

/// <summary>
/// 中断処理を用いるための基底クラス。
/// </summary>
public abstract class CancellableComponentBase : MonoBehaviour
{
    protected virtual void Awake()
    {
        NodeCanceller.OnCommandCancelled += OnExecuteCancelled;
    }
    protected virtual void OnDestroy()
    {
        NodeCanceller.OnCommandCancelled -= OnExecuteCancelled;
    }
    /// <summary>
    /// 中断時に実行される関数。
    /// </summary>
    protected abstract void OnExecuteCancelled();
}