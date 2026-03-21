using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    public void Play()
    {
        SoundManager.Instance.PlaySE(SESoundData.SE.GameJamTheme);
    }
}
