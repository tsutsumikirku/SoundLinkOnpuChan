/// <summary>
/// ノード全体を制御するインターフェイス
/// </summary>
public interface INodeControllable
{
    /// <summary>
    /// 指定タイプのノードを追加する。
    /// </summary>
    /// <param name="playerState">ノードの種類</param>
    public void AddNode(PlayerState playerState);
    
    /// <summary>
    /// 指定タイプのノードを削除する（使うか未定）
    /// </summary>
    /// <param name="playerState">ノードの種類</param>
    public void RemoveNode(PlayerState playerState);
}
