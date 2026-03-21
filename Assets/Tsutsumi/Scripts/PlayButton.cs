using UnityEngine;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using TMPro;
public class PlayButton : CancellableComponentBase, IPointerDownHandler, IPointerUpHandler, IUserInterface
{
    [SerializeField, Header("アニメーターをアタッチしてください")] Animator _animator;
    [SerializeField] TextMeshProUGUI _playText;
    [SerializeField] float _animationTime = 0.5f;
    [SerializeField] CanvasGroup[] _canvasGroup;
    [SerializeField] TextMeshProUGUI _text;
    Executor _excutor;
    NodeCanceller _nodeCanceller;
    System.Action _action;
    bool _isGet = false;
    ProgressGetter _progressGetter;
    CancellationTokenSource _cancellationToken;
    CancellationTokenSource _stopCancellation;

    void Start()
    {
        _progressGetter = FindAnyObjectByType<ProgressGetter>();
        _nodeCanceller = FindAnyObjectByType<NodeCanceller>();
        _excutor = FindAnyObjectByType<Executor>();
        _action = StartClick;
        FindFirstObjectByType<Player>().OnPlayerDeath += () => Stop().Forget();
    }
    async UniTask Stop()
    {
        _stopCancellation = new CancellationTokenSource();
        await UniTask.WaitForSeconds(_animationTime, cancellationToken: _stopCancellation.Token);
        StopClick();
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        _action?.Invoke();
    }
    public void OnPointerUp(PointerEventData eventData)
    {

    }
    public void StartClick()
    {
        _isGet = true;
        _animator.SetBool("Start", true);
        CriSEManager.Instance.PlaySE("SE_InGame_PlayButton", playOneShot: true);
        Debug.Log("PlayButton Clicked");
        _excutor.ExecuteButton();
        _action = StopClick;
        _cancellationToken = new CancellationTokenSource();
        OnPlay().Forget();
        Array.ForEach(_canvasGroup, x => x.alpha = 0.5f);
        _text.enabled = true;
        _playText.text = "ス ト ッ プ";

    }
    public void StopClick()
    {
        _stopCancellation?.Cancel();
        if (!_isGet) return;
        _nodeCanceller.CancelButton();
        CriSEManager.Instance.PlaySE("SE_InGame_PlayButton", playOneShot: true);
        _isGet = false;
        BGMManager.Instance.Edit();
        _cancellationToken?.Cancel();
        Array.ForEach(_canvasGroup, x => x.alpha = 1f);
        _text.enabled = false;
        _playText.text = "ス タ ー ト";
    }
    protected override void OnExecuteCancelled()
    {
        _animator.SetBool("Start", false);
        _action = StartClick;
        CriSEManager.Instance.PlaySE("SE_InGame_PlayButton", playOneShot: true);
    }

    public void UnPlay()
    {
        _nodeCanceller.CancelButton();
        _animator.speed = 1;
    }

    public void Play(int timeScale)
    {
        _animator.speed = 1.0f / timeScale;
    }
    async UniTask OnPlay()
    {
        await UniTask.Yield();
        while (true)
        {
            BGMManager.Instance.SetCrossfadeValue(_progressGetter.GetProgress());
            await UniTask.WaitForSeconds(0.5f, cancellationToken: _cancellationToken.Token);
        }
    }
    void OnDisable()
    {
        _cancellationToken?.Cancel();
    }
}
