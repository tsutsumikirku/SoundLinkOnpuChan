using System;
using UnityEngine;
using CriWare;
using UnityEngine.UI;

public class BGMManager : MonoBehaviour
{
    [SerializeField] CriAtomSource _editSource;
    [SerializeField] CriAtomSource[] _inGameSources = new CriAtomSource[3];
    [SerializeField] CriAtomSource _resultSource;
    [SerializeField] CriAtomSource _titleSource;
    [SerializeField] CriAtomSource _stageSelectSource;
    private float _bgmVolume;
    private float _seVolume;
    public static BGMManager Instance;
    public float BGMVolume
    {
        get => _bgmVolume;
        set => BGMSetVolume(value);
    }
    public float SEVolume
    {
        get => _seVolume;
        set
        {
            _seVolume = value;
            var data = new SEVolumeData() { Volume = _seVolume };
            JsonSave.Save("SEVolume", data);
            CriSEManager.Instance.SEVolume = _seVolume;
        }
    }
    void Awake()
    {
        if (Instance) return;
        Instance = this;
    }

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        if (!JsonSave.TryLoad("SEVolume", out SEVolumeData seData))
        {
            seData = new SEVolumeData() { Volume = 0.5f };
            JsonSave.Save("SEVolume", seData);
        }
        _seVolume = seData.Volume;
        CriSEManager.Instance.SEVolume = _seVolume;
        if (!JsonSave.TryLoad("BGMVolume", out BGMVolumeData data))
        {
            data = new BGMVolumeData() { Volume = 0.5f };
            JsonSave.Save("BGMVolume", data);
        }
        _bgmVolume = data.Volume;
        _titleSource.volume = _bgmVolume;
        _stageSelectSource.volume = _bgmVolume;
        _resultSource.volume = _bgmVolume;
        _editSource.volume = _bgmVolume;
        foreach (var source in _inGameSources)
        {
            source.volume = 0f;
        }
    }
    public void NotInit()
    {
        var seData = new SEVolumeData() { Volume = _seVolume };
        JsonSave.Save("SEVolume", seData);
        var bgmData = new BGMVolumeData() { Volume = _bgmVolume };
        JsonSave.Save("BGMVolume", bgmData);
    }
    public void TitleBGM(bool play)
    {
        if (play) _titleSource.Play();
        else _titleSource.Stop();
    }
    public void StageSelectBGM(bool play)
    {
        if (play) _stageSelectSource.Play();
        else _stageSelectSource.Stop();
    }

    public void ResultBGM(bool play)
    {
        if (play) _resultSource.Play();
        else _resultSource.Stop();
    }
    public void InGameBGM(bool play)
    {
        if (play)
        {
            _editSource.volume = _bgmVolume;
            _editSource.Play();
            foreach (var source in _inGameSources)
            {
                source.volume = 0f;
                source.Play();
            }
        }
        else
        {
            _editSource.Stop();
            foreach (var source in _inGameSources)
            {
                source.Stop();
            }
        }
    }
    public void BGMSetVolume(float newVolume)
    {
        _bgmVolume = newVolume;
        var data = new BGMVolumeData() { Volume = _bgmVolume };
        JsonSave.Save("BGMVolume", data);
        _editSource.volume = _bgmVolume;
        _titleSource.volume = _bgmVolume;
        _stageSelectSource.volume = _bgmVolume;
        _resultSource.volume = _bgmVolume;
        // in-game BGMの音量はクロスフェード時に調整されるためここでは0
        foreach (var source in _inGameSources)
        {
            source.volume = 0f;
        }
    }

    /// <summary>
    /// 0〜1の値でBGMをクロスフェードさせる
    /// 0.0   → BGM1のみ
    /// 0.25  → BGM1とBGM2の中間
    /// 0.5   → BGM2のみ
    /// 0.75  → BGM2とBGM3の中間
    /// 1.0   → BGM3のみ
    /// </summary>
    public void SetCrossfadeValue(float value)
    {
        _editSource.volume = 0f;

        if (value <= 0.5f)
        {
            // 0〜0.5: 1から2へクロスフェード
            float t = value / 0.5f;
            _inGameSources[0].volume = Mathf.Lerp(_bgmVolume, 0f, t);
            _inGameSources[1].volume = Mathf.Lerp(0f, _bgmVolume, t);
            _inGameSources[2].volume = 0f;
        }
        else
        {
            // 0.5〜1: 2から3へクロスフェード
            float t = (value - 0.5f) / 0.5f;
            _inGameSources[0].volume = 0f;
            _inGameSources[1].volume = Mathf.Lerp(_bgmVolume, 0f, t);
            _inGameSources[2].volume = Mathf.Lerp(0f, _bgmVolume, t);
        }
    }

    public void Edit()
    {
        _editSource.volume = _bgmVolume;
        foreach (var source in _inGameSources)
        {
            source.volume = 0f;
        }
    }
}
public class SEVolumeData
{
    public float Volume = 0.5f;
}
public class BGMVolumeData
{
    public float Volume = 0.5f;
}

