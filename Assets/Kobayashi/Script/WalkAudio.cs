using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkAudio : MonoBehaviour
{
    [SerializeField] private Player _player;
    [SerializeField] private string[] _walkAudio;
    [SerializeField] private string[] _backWalkAudio;
    
    void CallWalkAudio(int id)
    {
        id = Mathf.Clamp(id,0, _walkAudio.Length);
        var walkAudioData = _player.MoveDirection == 1 ? _walkAudio : _backWalkAudio;
        var walkAudio = walkAudioData[id];
        CriSEManager.Instance.PlaySE(walkAudio, playOneShot: false);
    }
}
