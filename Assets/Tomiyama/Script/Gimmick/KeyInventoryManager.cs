using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 保持している鍵をまとめて管理するクラス。
/// </summary>
public class KeyInventoryManager : CancellableComponentBase
{
    [Header("プレイヤーに追従し、到達するまでの時間")] [SerializeField]
    private float _followSpeed;

    [Header("それぞれの鍵に設ける最大距離")] [SerializeField]
    private float _followDistance;

    [SerializeField] private Transform _playerTransform;
    private readonly Stack<KeyBehaviour> _keyStack = new();

    /// <summary>
    /// スタック配列に新しく鍵を追加。
    /// </summary>
    public void AddKey(KeyBehaviour key)
    {
        key.FollowSpeed = _followSpeed;
        key.FollowDistance = _followDistance;
        AdjustFollowTarget(key);
        _keyStack.Push(key);
    }

    /// <summary>
    /// スタック配列から、渡されたパスと合致する鍵を検索し、使用する。
    /// </summary>
    /// <param name="pass">合致させたいパス</param>
    /// <returns>見つかったかどうかの結果</returns>
    public bool SearchAndUseKey(KeyPass pass)
    {
        if (_keyStack == null) return false;

        var stackBuffer = new Stack<KeyBehaviour>();
        var isFound = false;
        while (_keyStack.Count > 0)
        {
            var currentKey = _keyStack.Pop();

            // パスが合致する鍵を探し、使用する。
            if (currentKey.Pass == pass)
            {
                currentKey.UseKey();
                isFound = true;
                continue;
            }

            stackBuffer.Push(currentKey);
        }

        while (stackBuffer.Count > 0)
        {
            var currentKey = stackBuffer.Pop();

            // 鍵が使用された場合、追従対象を修正する必要がある。
            if (isFound) AdjustFollowTarget(currentKey);

            _keyStack.Push(currentKey);
        }

        return isFound;
    }

    /// <summary>
    /// スタック配列の最前列に追従対象を変更する。
    /// </summary>
    /// <param name="key">追加する想定の鍵</param>
    private void AdjustFollowTarget(KeyBehaviour key)
    {
        if (_keyStack == null) return;
        // スタック配列が空かどうかで対象を決める。
        var target = _keyStack.TryPeek(out var existingKey) ? existingKey.transform : _playerTransform;
        key.FollowTarget = target;

        if (!target.TryGetComponent<SpriteRenderer>(out var spriteRenderer)) return;
        if (!key.TryGetComponent<SpriteRenderer>(out var keySpriteRenderer)) return;

        // 表示順を追従対象の真後ろにする。
        var sortingOrder = spriteRenderer.sortingOrder;
        keySpriteRenderer.sortingOrder = sortingOrder - 1;
    }

    protected override void OnExecuteCancelled()
    {
        _keyStack.Clear();
    }
}

public enum KeyPass
{
    A,
    B,
    C,
    D,
    E,
    F,
    G
}