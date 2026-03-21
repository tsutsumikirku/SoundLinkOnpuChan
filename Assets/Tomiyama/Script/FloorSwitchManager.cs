using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FloorSwitchManager : CancellableComponentBase
{
    [SerializeField] private List<GameObject> _switchableFloors;
    private List<bool> _defaultActiveList = new();

    [SerializeField] private List<SwitchBlock> _switchBlocksList;

    private void Start()
    {
        _defaultActiveList = _switchableFloors.Select(go => go.activeSelf).ToList();
    }
    /// <summary>
    /// 床の表示、非表示を切り替える。
    /// </summary>
    public void SwitchFloor()
    {
        SoundManager.Instance.PlaySE(SESoundData.SE.GameJamTheme);
        foreach (var go in _switchableFloors)
        {
            go.SetActive(!go.activeSelf);
        }

        //ここから追加
        foreach (var go in _switchBlocksList)
        {
            go.ChangeState();
        }

    }
    public void AddToList(GameObject go) => _switchableFloors.Add(go);

    protected override void OnExecuteCancelled()
    {
        for (int i = 0; i < _switchableFloors.Count; i++)
        {
            _switchableFloors[i].SetActive(_defaultActiveList[i]);
        }

        //ここから追加
        foreach (var go in _switchBlocksList)
        {
            go.Reset();
        }
    }


    public void AddToBlockList(GameObject obj) => _switchBlocksList.Add(obj.GetComponent<SwitchBlock>());
}
