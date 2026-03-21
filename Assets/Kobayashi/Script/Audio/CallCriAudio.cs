using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CallCriAudio : MonoBehaviour
{
    [SerializeField] private int _sheetID;
    [SerializeField] private bool _playOneShot = true;
    public void SetCueSheetID(int id) => _sheetID = id;
    public void CallSE(int cueID)
    {
        CriSEManager.Instance.PlaySE(_sheetID, cueID,  playOneShot:_playOneShot);
    }

    public void CallSE(string cueName)
    {
        CriSEManager.Instance.PlaySE(cueName, playOneShot: _playOneShot);
    }
}
