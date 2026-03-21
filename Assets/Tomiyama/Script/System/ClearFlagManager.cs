using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 全ステージのクリアフラグを監理するクラス。
/// </summary>
public class ClearFlagManager : MonoBehaviour
{
    private bool[] _clearFlagsArr;
    [SerializeField] private int _stageCount = 1;
    [SerializeField] private int _nonStageCount = 1;
    private static ClearFlagManager _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        _clearFlagsArr = new bool[_stageCount];
        SceneManager.sceneLoaded += (scene, mode) =>
        {
            var goal = FindAnyObjectByType<GoalBehaviour>();

            // ゴールのGameObjectがシーン上にあったら
            if (goal != null)
            {
                // シーンのインデックスを取得
                int index = scene.buildIndex - _nonStageCount;

                // クリア時のActionに処理を追加
                goal.OnClear += () => _clearFlagsArr[index] = true;
                return;
            }

            var iconManager = FindAnyObjectByType<ClearIconUpdateManager>();

            // IconManagerがある = ステージセレクトのシーンである
            if (iconManager != null)
            {
                // 記録されたクリアフラグを渡す。
                iconManager.UpdateImages(_clearFlagsArr);
            }
        };
    }
}
