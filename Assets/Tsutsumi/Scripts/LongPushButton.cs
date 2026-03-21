using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using R3.Triggers;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 長押しするボタン
/// </summary>
public class LongPushButton : MonoBehaviour
{
    [SerializeField] int _longPushTime = 1; // 長押しする時間
    [SerializeField] Button button; // ボタン
    [SerializeField] Image image; // ボタンの画像
    CancellationTokenSource token; // キャンセルトークン
    void Start()
    {
        button.onClick.AddListener(OnPointerDown);
    }   
    void OnPointerDown()
    {
        token?.Cancel();
        token = new CancellationTokenSource();
        _ = LongPushRoutine(token);
    }
    async UniTask LongPushRoutine(CancellationTokenSource token)
    {
        image.fillAmount = 0f;
        while (image.fillAmount < 1f)
        {
            image.fillAmount += 0.1f;
            await UniTask.Delay(100, cancellationToken: token.Token);
        }
    }
}
