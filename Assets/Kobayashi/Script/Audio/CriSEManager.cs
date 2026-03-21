#region

using System;
using System.Collections.Generic;
using System.Threading;
using CriWare;
using Cysharp.Threading.Tasks;
using UnityEngine;

#endregion

public class CriSEManager : MonoBehaviour, TimeScaleManager.IChangeTimeScale
{
    public static CriSEManager Instance;
    [SerializeField] private CriAtomSource _seSource;

    //マスターボリューム管理用
    [SerializeField] [Range(0, 1)] private float _seVolume = 1;
    [SerializeField] [Range(0, 1)] private float _sePitch = 1;
    [SerializeField] [Range(-255, 255)] private int _seCuePriority = -1;
    [SerializeField] [Range(-1200, 1200)] private float _seTimeScalePitch = 1200;

    [Header("CueSheetDataを入力してください")] public CueSheetData[] SeCueSheetData;
    public List<AudioData> SeAudioData;
    private readonly List<CriAtomExPlayer> _sePlayers = new();
    private float _defaultSEPitch = 0;
    private CancellationTokenSource _cancellationTokenSource;

    public float SEVolume
    {
        get => _seVolume;
        set => _seVolume = Mathf.Clamp01(value);
    }

    public float SEPitch
    {
        get => _sePitch;
        set => _sePitch = Mathf.Clamp(value, -1200, 1200);
    }

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
            return;
        }


        //データとして保持しているものをCriコンポーネントに追加する。
        foreach (var data in SeCueSheetData) RegisterCueSheet(data.cueSheetName, data.acbFileName, data.awbFileName);
        
        _defaultSEPitch = SEPitch;
        //トークン作成
        _cancellationTokenSource = new CancellationTokenSource();


        void RegisterCueSheet(string cueSheetName, string acbPath, string awbPath)
        {
            var sheet = CriAtom.GetCueSheet(cueSheetName);
            if (sheet != null)
            {
                Debug.Log($"{cueSheetName} は既に登録済みです");
                return;
            }

            if (string.IsNullOrEmpty(cueSheetName)) Debug.LogWarning("cueSheetNameに不正なデータが入力されました。");

            CriAtom.AddCueSheet(cueSheetName, acbPath, awbPath);
            Debug.Log($"{cueSheetName}を登録しました");
        }
    }

    private void OnEnable()
    {
        TimeScaleManager.IChangeTimeScale.RegisterObject(this);
    }

    private void OnDisable()
    {
        TimeScaleManager.IChangeTimeScale.UnregisterObject(this);
        foreach (var player in _sePlayers)
        {
            if (player != null)
            {
                player.Stop();
                player.Dispose();
            }
        }
        _sePlayers.Clear();
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
    }

    private void OnDestroy()
    {
    }

    public void ChangeTimeScale(float time)
    {
        //初期のTimeScaleを1とする
        SEPitch = time > 1 ? _seTimeScalePitch * time : _defaultSEPitch;
    }


    //IDでの保守が難しいため、sheetのindexとcueのindexで管理する
    /// <summary>
    ///     SE再生メソッド
    /// </summary>
    /// <param name="sheetName">シートの名前</param>
    /// <param name="cueID">シート内でのID</param>
    /// <param name="volume">ボリューム</param>
    /// <param name="pitch">pitch</param>
    public void PlaySE(string sheetName, int cueID, float volume = 0, float pitch = 0, bool playOneShot = true)
    {
        var sheet = CriAtom.GetCueSheet(sheetName);
        sheet.acb.GetCueInfoByIndex(cueID, out var info);
        var cueName = info.name;
        PlaySE(cueName, volume, pitch, playOneShot);
    }

    public void PlaySE(int sheetID, int cueID, float volume = 0, float pitch = 0, bool playOneShot = true)
    {
        PlaySE(SeCueSheetData[sheetID].cueSheetName, cueID, volume, pitch, playOneShot);
    }

    public void PlaySE(string cueName, float volume = 0, float pitch = 0, bool playOneShot = true)
    {
        PlayAudio(SeAudioData, cueName, volume, pitch, playOneShot);
    }

    private void PlayAudio(List<AudioData> data, string cueName, float volume = 0, float pitch = 0,
        bool isPlayOneShot = true)
    {
        volume = Mathf.Clamp01(volume);
        pitch = Mathf.Clamp(pitch, -1200, 1200);
        var audioData = data.Find(data => data.AudioName == cueName);
        if (audioData == null)
        {
            Debug.Log($"{cueName} is not found");
            return;
        }

        var acb = CriAtom.GetAcb(audioData.CueSheetName);
        if (acb == null)
        {
            Debug.Log($"{cueName} is not found");
            return;
        }

        //ボリュームが設定されている場合はそちらを優先する
        volume = (volume == 0 ? audioData.Volume : volume) * _seVolume;
        //pitchが設定された場合は、全体のpitchを無効化する。
        pitch = pitch == 0 ? Mathf.Clamp(audioData.Pitch + _sePitch, -1200, 1200) : pitch;
        if (isPlayOneShot)
        {
            //audioの設定変更
            var player = new CriAtomExPlayer();
            player.SetVolume(volume);
            player.SetPitch(pitch);
            player.SetCue(acb, cueName);
            player.SetVoicePriority(_seCuePriority);
            var playback = player.Start();
            _sePlayers.Add(player);

            AudioDispose(player, playback, _cancellationTokenSource.Token).Forget();
        }
        else
        {
            _seSource.Stop();
            _seSource.volume = volume;
            _seSource.pitch = pitch;
            _seSource.cueName = cueName;
            _seSource.cueSheet = audioData.CueSheetName;
            _seSource.Play();
        }
    }

    public void DebugPlaySE()
    {
        for (var i = 0; i < 50; i++) PlayAudio(SeAudioData, "SE_InGame_Onpu_Attack");
    }

    private async UniTask AudioDispose(CriAtomExPlayer player, CriAtomExPlayback playback,
        CancellationToken token = default)
    {
        // 音が終わるまで待つ
        while (playback.GetStatus() != CriAtomExPlayback.Status.Removed)
        {
            await UniTask.Yield();
            token.ThrowIfCancellationRequested();
        }

        // 再生終了後にプレイヤーを破棄
        _sePlayers.Remove(player);
        playback.Stop();
        player.Dispose();
    }

    public void StopSE()
    {
        _seSource.Stop();
    }

    //メモリ管理用メソッド
    public void LoadCueSheet(int id)
    {
        var data = SeCueSheetData[id];
        CriAtom.AddCueSheet(data.cueSheetName, data.acbFileName, data.awbFileName);
    }

    public void RemoveCueSheet(int id)
    {
        CriAtom.RemoveCueSheet(SeCueSheetData[id].cueSheetName);
    }

    [Serializable]
    public class CueSheetData
    {
        public string cueSheetName;
        public string acbFileName;
        public string awbFileName;
    }

    [Serializable]
    public class AudioData
    {
        public string AudioName;
        public string CueSheetName;
        public int CueSheetID;
        public int AudioID;
        [Range(0, 1)] public float Volume = .5f;
        [Range(-1200, 1200)] public float Pitch;
    }
}