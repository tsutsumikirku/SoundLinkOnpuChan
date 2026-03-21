using UnityEngine;
using UnityEngine.UI;

public class VibrationTest : MonoBehaviour
{
    [SerializeField] private InputField vibrationTypeInput;
    [SerializeField] private Button vibrateButton;
    void Start()
    {
        vibrateButton.onClick.AddListener(() => TriggerVibrate(int.Parse(vibrationTypeInput.text)));
    }
    public void TriggerVibrate(int vibrationType)
    {
        #if UNITY_IOS && !UNITY_EDITOR
        Vibrate.VibrateDevice(vibrationType);
        #endif
    }
}
