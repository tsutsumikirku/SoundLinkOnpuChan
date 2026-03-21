using UnityEngine;

/// <summary>
/// 自動的にFloorSwitchManagerに追加するためのクラス。
/// </summary>
public class SwitchAssigner : MonoBehaviour
{
    [SerializeField, Header("登録対象のゲームオブジェクト")] private GameObject _assignTargetObject;
    void Start()
    {
        var switchManager = FindAnyObjectByType<FloorSwitchManager>();
        if (switchManager != null && _assignTargetObject)
        {
            switchManager.AddToList(_assignTargetObject);
        }
    }
}
