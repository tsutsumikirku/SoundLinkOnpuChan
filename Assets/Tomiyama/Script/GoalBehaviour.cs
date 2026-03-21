using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoalBehaviour : MonoBehaviour
{
    public Action OnClear;
    [SerializeField] private LayerMask _playerLayer;
    [SerializeField] private Ease _fadeEasing;
    [SerializeField] private float _fadeDuration;

    private ClearPanelManager _clearPanel;

    private void Start()
    {
        _clearPanel = FindObjectOfType<ClearPanelManager>();
        var nodeCanceller = FindAnyObjectByType<Executor>();
        if (nodeCanceller != null) OnClear += () => nodeCanceller.CancelNodes();

        OnClear += () => OnClear = null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (1 << collision.gameObject.layer == _playerLayer)
        {
            TimeScaleManager.ChangeTimeScale(1);
            OnClear?.Invoke();
            GoalDetector.OnClearDetected?.Invoke();
            _clearPanel.ClearSetUp();
        }
    }
}

public static class GoalDetector
{
    /// <summary>
    /// クリアが検出された際に呼び出されるAction
    /// </summary>
    public static Action OnClearDetected;

    /// <summary>
    /// 最初のシーンが読み込まれた際にsceneLoadedへActionの初期化処理を登録する
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize() => SceneManager.sceneLoaded += (_, _) => OnClearDetected = null;

    /// <summary>
    /// インターフェイスのOnGoalをOnClearDetectedに登録する
    /// </summary>
    /// <param name="goalDetectable"></param>
    public static void Register(IGoalDetectable goalDetectable) => OnClearDetected += goalDetectable.OnGoal;

    /// <summary>
    /// インターフェイスのOnGoalをOnClearDetectedから解除する
    /// </summary>
    /// <param name="goalDetectable"></param>
    public static void Unregister(IGoalDetectable goalDetectable) => OnClearDetected -= goalDetectable.OnGoal;

    /// <summary>
    /// ゴールした際に呼び出されるメソッドを持つインターフェイス
    /// </summary>
    public interface IGoalDetectable
    {
        /// <summary>
        /// ゴール時に呼び出したい処理を実装する関数
        /// </summary>
        public void OnGoal();
    }
}