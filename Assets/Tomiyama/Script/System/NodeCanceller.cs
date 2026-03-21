using System;
using UnityEngine;
/// <summary>
/// 中断処理をボタンが押されたときに実行するクラス。
/// </summary>
public class NodeCanceller : MonoBehaviour
{
    public static event Action OnCommandCancelled;
    public void CancelButton()
    {
        OnCommandCancelled?.Invoke();
    }
    private void OnDisable()
    {
        OnCommandCancelled = null;
    }
}
