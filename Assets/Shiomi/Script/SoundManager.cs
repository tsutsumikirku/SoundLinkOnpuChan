using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour, TimeScaleManager.IChangeTimeScale
{
    [SerializeField] AudioSource _bgmAudioSource;
    [SerializeField] AudioSource _seAudioSource;
    [SerializeField] List<BGMSoundData> _bgmSoundDatas;
    [SerializeField] List<SESoundData> _seSoundDatas;

    public float _masterVolume = 1;
    public float _bgmMasterVolume = 1;
    public float _seMasterVolume = 1;

    public static SoundManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        TimeScaleManager.IChangeTimeScale.RegisterObject(this);
    }
    private void OnDisable()
    {
        TimeScaleManager.IChangeTimeScale.UnregisterObject(this);
    }

    public void PlayBGM(BGMSoundData.BGM bgm)
    {
        BGMSoundData data = _bgmSoundDatas.Find(data => data._bgm == bgm);
        _bgmAudioSource.clip = data._audioClip;
        _bgmAudioSource.volume = data._volume * _bgmMasterVolume * _masterVolume;
        _bgmAudioSource.Play();
    }


    public void PlaySE(SESoundData.SE se)
    {
        SESoundData data = _seSoundDatas.Find(data => data._se == se);
        _seAudioSource.volume = data._volume * _seMasterVolume * _masterVolume;
        _seAudioSource.PlayOneShot(data._audioClip);
    }

    public void ChangeSEPitch(float pitch)
    {
        _seAudioSource.pitch = pitch;
    }

    /// <summary>
    /// 徐々に音を消す
    /// </summary>
    public void FadeBGMSound(float duration)
    {
        float vol = 0.0f;
        _bgmAudioSource.DOFade(vol, duration);
    }

    public void TestSE()
    {
        PlayBGM(BGMSoundData.BGM.Ingame);
    }

    public void ChangeTimeScale(float time)
    {
        ChangeSEPitch(time);
    }
}

[System.Serializable]
public class BGMSoundData
{
    public enum BGM
    {
        //ここの部分がラベルになる
        Title,
        StageSelect,
        Ingame,
        ResultPanel
    }

    public BGM _bgm;
    public AudioClip _audioClip;
    [Range(0, 1)] public float _volume = 1;
}

[System.Serializable]
public class SESoundData
{
    public enum SE
    {
        //ここの部分がラベルになる
        Walk,
        Jump,
        Landing,
        Attack,
        GameJamTheme,
        Select,
        EnemyDefeat,
        Goal,
        GameClear,
        GameOver,
        Stop,
        Result,
        Play,
        Set,
        Typing
    }

    public SE _se;
    public AudioClip _audioClip;
    [Range(0, 1)] public float _volume = 1;
}