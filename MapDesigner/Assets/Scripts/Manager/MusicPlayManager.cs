using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MusicPlayManager : MonoBehaviour
{
    public static MusicPlayManager Instance;

    #region Properties

    /// <summary>���ֲ�����</summary>
    [SerializeField] private AudioSource musicAudioSource;
    /// <summary>��Ч������</summary>
    private AudioSource[] effectAudioSources;
    private int effectAudioIndex;

    private bool isMusicPlaying;
    public bool IsMusicPlaying
    {
        get => isMusicPlaying;
        set
        {            
            if (value ^ isMusicPlaying)
            {
                isMusicPlaying = value;
                onMusicPlayStatusChanged?.Invoke(value);
            }
        }
    }

    public float MusicLength => musicAudioSource.clip.length;
    private float musicTime;

    /// <summary>��ǰ����ʱ��</summary>
    public float MusicTime
    {
        get => musicTime;
        set
        {
            // ʱ��һ����ʱ�������䶯
            if (Math.Abs(musicTime - value) < 1e-7)
            {
                return;
            }
            musicTime = value;
            onMusicPlaying?.Invoke(value);
        }
    }

    public float MusicVolume
    {
        get => musicAudioSource.volume;
        set
        {
            musicAudioSource.volume = value;
        }
    }

    public float EffectVolume
    {
        get => effectAudioSources[0].volume;
        set
        {
            for (int i = 0; i < effectAudioSources.Length; i++)
            {
                AudioSource effectAudioSource = effectAudioSources[i];
                effectAudioSource.volume = value;
            }
        }
    }

    /// <summary>�������ʱ�䲽��(s)</summary>
    private float deltaMoveTime = 2;

    private Action<float> onMusicPlaying;
    private Action<bool> onMusicPlayStatusChanged;

    /// <summary>��ʼ����ʱ��</summary>
    [SerializeField] private float StartPlayTime;

    #region Effect Audio

    [SerializeField] private AudioClip tapEffect;
    [SerializeField] private AudioClip flickEffect;
    [SerializeField] private AudioClip holdEffect;

    #endregion

    #endregion

    #region Unity Engine

    private void Awake()
    {
        Instance = this;
        Initialize();
    }

    private void Update()
    {
        if (musicAudioSource.isPlaying)
        {
            MusicTime = musicAudioSource.time;
        }
    }

    #endregion

    #region Public Method

    public void Initialize()
    {
        effectAudioIndex = 0;
        effectAudioSources = new AudioSource[4];

        for (int i = 0; i < 4; i++)
        {
            var effectAudioSource = gameObject.AddComponent<AudioSource>();
            effectAudioSource.loop = false;
            effectAudioSource.playOnAwake = false;
            effectAudioSources[i] = effectAudioSource;
        }

        PlayerInputManager.Instance.OnLeftArrow += isToTheEnd =>
        {
            MusicTime = isToTheEnd ? 0 : Mathf.Max(0, MusicTime - deltaMoveTime);
            Resux.GamePlay.GamePlayer.Instance.RefreshNotes();
        };
        PlayerInputManager.Instance.OnRightArrow += isToTheEnd =>
        {
            MusicTime = isToTheEnd ? MusicLength : Mathf.Min(MusicLength, MusicTime + deltaMoveTime);
            Resux.GamePlay.GamePlayer.Instance.RefreshNotes();
        };
    }

    /// <summary>
    /// ���������Է��ɹ�
    /// </summary>
    /// <returns></returns>
    public bool LoadMusic(string ext)
    {
        AudioType audioType = AudioType.UNKNOWN;
        switch (ext)
        {
            case "wav":
                audioType = AudioType.WAV;
                break;
            case "mp3":
                audioType = AudioType.MPEG;
                break;
            case "ogg":
            default:
                audioType = AudioType.OGGVORBIS;
                break;
        }
        UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip($"file://{MapDesignerSettings.ProjectPath}/audio.{ext}", audioType);
        request.SendWebRequest();
        while (!request.isDone) { }
        AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
        InitMusic(clip);
        return clip != null;
    }

    public void AddMusicPlayingListener(Action<float> onPlaying)
    {
        onMusicPlaying += onPlaying;
    }

    /// <summary>
    /// �������ٸ����¼�
    /// </summary>
    /// <param name="value">����ֵ</param>
    public void OnMusicSpeedChanged(float value)
    {
        if (EditingData.IsOpenMusic)
        {
            musicAudioSource.pitch = value;
        }
    }

    /// <summary>
    /// ����״̬����¼�
    /// </summary>
    /// <param name="onPlayChanged"></param>
    public void AddMusicPlayStatusChangedListener(Action<bool> onPlayChanged)
    {
        onMusicPlayStatusChanged += onPlayChanged;
    }

    /// <summary>
    /// ���´�ָ��λ�ò���
    /// </summary>
    public void RePlayFromStartTime()
    {
        if (EditingData.IsOpenMusic)
        {
            musicAudioSource.Pause();
            musicAudioSource.time = StartPlayTime;
            MusicTime = StartPlayTime;
        }
    }

    /// <summary>
    /// ��������Ӧ�¼�
    /// </summary>
    public void OnMusicProgressChanged(float value)
    {
        if (EditingData.IsOpenMusic)
        {
            if (Mathf.Abs(MusicTime - MusicLength * value) > 1e-5)
            {
                MusicTime = musicAudioSource.time = MusicLength * value;
            }
        }
    }

    public void PauseMusic()
    {
        if (EditingData.IsOpenMusic)
        {
            musicAudioSource.Pause();
            IsMusicPlaying = false;
        }
    }

    public void StopMusic()
    {
        musicAudioSource.Stop();
        IsMusicPlaying = false;
    }

    public void PlayMusic()
    {
        if (EditingData.IsOpenMusic)
        {
            StartPlayTime = musicAudioSource.time = MusicTime;
            GamePreviewManager.Instance.InitJudgeNoteQueue();
            musicAudioSource.Play();
            IsMusicPlaying = true;
        }
    }

    public void PlayEffect(JudgeType judgeType)
    {
        AudioClip effectClip = null;
        switch (judgeType)
        {
            case JudgeType.Tap:
                effectClip = tapEffect;
                break;
            case JudgeType.Hold:
                effectClip = holdEffect;
                break;
            case JudgeType.Flick:
                effectClip = flickEffect;
                break;
        }

        var effectAudioSource = effectAudioSources[effectAudioIndex];

        if (effectAudioSource.clip != effectClip)
        {
            effectAudioSource.clip = effectClip;
        }
        effectAudioSource.Stop();
        effectAudioSource.Play();

        effectAudioIndex++;
        effectAudioIndex %= effectAudioSources.Length;
    }

    #endregion

    #region Private Method

    private void InitMusic(AudioClip musicClip)
    {
        musicAudioSource.clip = musicClip;
        MusicVolume = GlobalSettings.MusicVolume;
        EffectVolume = GlobalSettings.TapAudioVolume;

        // ��ӿո��¼�����
        PlayerInputManager.Instance.OnSpaceKeyDownAction += () =>
        {
            if (IsMusicPlaying)
            {
                PauseMusic();
            }
            else
            {
                PlayMusic();
            }
        };
    }

    #endregion
}
