using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetScreen : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // 自動回転を有効化
        Screen.orientation = ScreenOrientation.AutoRotation;

        // 許可する向きだけオンにする
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
    }
}
