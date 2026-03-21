using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StarCoinManager : CancellableComponentBase
{
    [SerializeField] [Tooltip("シーン内の全てのスターコインを登録する")]
    private StarCoinBehaviour[] starCoins;
    
    private static readonly Dictionary<StarCoinBehaviour, bool> CollectedStarCoinsDict = new();

    private void Start()
    {
        // 辞書の初期化
        CollectedStarCoinsDict.Clear();
        
        InitStarCoins();
    }

    // private void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.Space))
    //     {
    //         Debug.Log(IsAllCollected());
    //     }
    // }

    private void InitStarCoins()
    {
        foreach (var starCoin in starCoins)
        {
            // 全てのスターコインを未収集状態に設定し、収集イベントを登録
            CollectedStarCoinsDict[starCoin] = false;
            starCoin.OnCollected += () => UpdateStarCoin(starCoin, true);
        }
    }

    private static void UpdateStarCoin(StarCoinBehaviour starCoin, bool isCollected)
    {
        // スターコインの収集状態を更新し、収集済みなら非表示にする
        CollectedStarCoinsDict[starCoin] = isCollected;
        starCoin.gameObject.SetActive(!isCollected);
    }

    protected override void OnExecuteCancelled()
    {
        // 実行キャンセル時に全てのスターコインを未収集状態に戻す
        foreach (var starCoin in starCoins)
        {
            UpdateStarCoin(starCoin, false);
        }
    }

    private void OnDisable()
    {
        // 辞書の初期化
        CollectedStarCoinsDict.Clear();
    }

    /// <summary>
    /// スターコインを全て集めたかどうかを返す
    /// </summary>
    /// <returns>全て集めている場合はtrue、そうでない場合はfalseを返す</returns>
    public static bool IsAllCollected()
    {
        // 辞書が空の場合、警告を表示
        if (CollectedStarCoinsDict.Count == 0)
        {
            Debug.LogWarning("シーン上にスターコインが存在しないか、マネージャークラスが存在しません。");
        }
        
        return CollectedStarCoinsDict.All(kvp => kvp.Value);
    }
}