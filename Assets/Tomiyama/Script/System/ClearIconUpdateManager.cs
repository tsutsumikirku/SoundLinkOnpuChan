using UnityEngine;
using UnityEngine.UI;

public class ClearIconUpdateManager : MonoBehaviour
{
    [SerializeField] private Image[] _clearIconsList;
    [SerializeField] private Sprite _unclearedIcon;
    [SerializeField] private Sprite _clearedIcon;

    /// <summary>
    /// ステージ選択画面のクリアアイコンを更新する。
    /// </summary>
    /// <param name="flags"></param>
    public void UpdateImages(bool[] flags)
    {
        for (int i = 0; i < flags.Length; i++)
        {
            _clearIconsList[i].sprite = (flags[i]) ? _clearedIcon : _unclearedIcon;
        }
    }
}
