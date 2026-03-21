using System.Runtime.InteropServices;

#if UNITY_IOS && !UNITY_EDITOR
public static class Vibrate
{
    [DllImport("__Internal")]
    static extern void _playSystemSound(int n);

    public static void VibrateDevice(int vibrationType)
    {
        _playSystemSound(vibrationType);
    }

    public enum VibrationType
    {
        SingleShortVibrate = 1003,
        DoubleLongVibrate = 1011,
        DoubleShortVibrate = 1102,
        SingleVeryShortVibrate = 1161,
        DoubleVeryShortVibrate,
        MultiVeryShortVibrate,
        DoubleVeryLongVibrate,
        SingleLongVibrate,
        SingleVeryShortVibrate2,
        SingleVeryShortVibrate3
    }
}
#endif