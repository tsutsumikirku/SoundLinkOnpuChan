using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingPanel : MonoBehaviour
{
    [SerializeField] private UIButton _oneButton;
    [SerializeField] private UIButton _minusOneButton;
    Action<float> _updateValueAction;
    
}
