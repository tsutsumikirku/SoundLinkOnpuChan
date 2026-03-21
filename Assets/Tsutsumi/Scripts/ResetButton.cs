using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetButton : MonoBehaviour
{
    [ContextMenu("SaveReset")]
    public void SaveReset()
    {
        JsonSave.Reset();
    }
}
