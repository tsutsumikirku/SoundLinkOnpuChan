using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchBlock : MonoBehaviour
{
    [SerializeField, Header("消えてるオブジェクト")] private GameObject _invisible;
    [SerializeField, Header("出現しているオブジェクト")] private GameObject _normal;

    [SerializeField, Header("初期の出現の有無")] private bool _firstVisible; //初期の状態

    bool _currentVisible;

    // Start is called before the first frame update
    void Start()
    {
        var switchManager = FindAnyObjectByType<FloorSwitchManager>();
        if (switchManager != null)
        {
            switchManager.AddToBlockList(this.gameObject);
        }

        _currentVisible = _firstVisible;
        SwitchingBlock(_currentVisible);
    }

    private void SwitchingBlock(bool visible)
    {
        if (visible)
        {
            _invisible.SetActive(false);
            _normal.SetActive(true);
        }
        else
        {
            _invisible.SetActive(true);
            _normal.SetActive(false);
        }
    }

    public void ChangeState()
    {
        _currentVisible = !_currentVisible;
        SwitchingBlock(_currentVisible);
    }

    public void Reset()
    {
        _currentVisible = _firstVisible;
        SwitchingBlock(_currentVisible);
    }
}
