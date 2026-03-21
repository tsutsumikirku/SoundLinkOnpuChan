/// <summary>
/// 再生開始時と再生中断時の処理を用いるための基底クラス。
/// </summary>
public abstract class ExecutableCancellableBase : CancellableComponentBase
{
    protected override void Awake()
    {
        base.Awake();
        Executor.OnExecuteBegin += OnExecuteBegin;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Executor.OnExecuteBegin -= OnExecuteBegin;
    }
    /// <summary>
    /// 再生開始時に呼ばれる関数。
    /// </summary>
    protected abstract void OnExecuteBegin();
}
