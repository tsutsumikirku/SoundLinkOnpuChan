using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TimeScaleManager
{
    public static float InGameTimeScale = 1;
    static Action<float> OnChangeTimeScale;

    static Dictionary<IChangeTimeScale, Action<float>> _registerActionList = new();
    
    /// <summary>
    /// TimeScale変更時の処理
    /// </summary>
    /// <param name="time">変更後のTimeScale</param>
    public static void ChangeTimeScale(float time)
    {
        if (time <= 0) Debug.Log("Time Scaleに0以下の値が入力されました。");
        InGameTimeScale = time;
        Time.timeScale = InGameTimeScale;
        OnChangeTimeScale?.Invoke(InGameTimeScale);
    }

    public interface IChangeTimeScale
    {
        void ChangeTimeScale(float time);
        static void RegisterObject(IChangeTimeScale change)
        {
            if (_registerActionList.TryGetValue(change, out var action)) return;

            OnChangeTimeScale += change.ChangeTimeScale;
            _registerActionList[change] = action;
        }
        static void UnregisterObject(IChangeTimeScale change)
        {
            if (_registerActionList.TryGetValue(change, out var action)) return;
            OnChangeTimeScale -= action;
        }
    }

}
