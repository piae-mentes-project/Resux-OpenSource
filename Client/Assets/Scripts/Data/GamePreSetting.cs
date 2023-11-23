using UnityEngine;
using System.Collections;
using Resux;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

/// <summary>
/// 游戏的基础配置
/// </summary>
public class GamePreSetting
{
    /// <summary>
    /// 游戏使用的语言
    /// </summary>
    public Language Language = Language.ChineseSimplified;


    /// <summary>
    /// 游戏整体偏移（未插入蓝牙耳机时）
    /// </summary>
    [JsonProperty("Offset")]
    public int NormalOffset = 0;

    /// <summary>
    /// 游戏整体偏移（使用蓝牙耳机时）
    /// </summary>
    [JsonProperty("BluetoothOffset")]
    public int BluetoothOffset = 500;

    /// <summary>
    /// 根据 <see cref="IsUsingBluetoothHeadset"/> 自动判断应该使用的延迟设定 <br/>
    /// [本字段不会被序列化]
    /// </summary>
    [JsonIgnore]
    public int Offset
    {
        get => IsUsingBluetoothHeadset ? BluetoothOffset : NormalOffset;
        set
        {
            if (IsUsingBluetoothHeadset)
            {
                BluetoothOffset = value;
            }
            else
            {
                NormalOffset = value;
            }
        }
    }

    /// <summary>
    /// 指示 <see cref="Offset"/> 应选用哪个延迟值 <br/>
    /// [本字段不会被序列化]
    /// </summary>
    [JsonIgnore]
    public bool IsUsingBluetoothHeadset = false;

    /// <summary>
    /// Dsp缓冲区大小
    /// </summary>
    public int DspBufferSize = AudioSettings.GetConfiguration().dspBufferSize;

    /// <summary>
    /// 通用音效音量
    /// </summary>
    public int EffectVolume = 100;

    /// <summary>
    /// 打击音效大小
    /// </summary>
    public int EffectLoudness = 100;

    /// <summary>
    /// 打击特效大小
    /// </summary>
    public float EffectSize = 1.3f;

    /// <summary>
    /// Note大小
    /// </summary>
    public float NoteSize = 1.3f;

    /// <summary>
    /// 曲绘背景亮度
    /// </summary>
    public float CoverBrightness = 0.9f;

    /// <summary>
    /// 曲绘模糊倍率
    /// </summary>
    public float CoverBlurRate = 0.3f;

    /// <summary>
    /// 总体音量大小
    /// </summary>
    public float MainAudioVolume = 1;

    /// <summary>
    /// 音乐音量大小
    /// </summary>
    public float MusicVolume = 1;

    public GamePreSetting()
    {
    }

    public GamePreSetting(GamePreSetting gamePreSetting)
    {
        Language = gamePreSetting.Language;
        Offset = gamePreSetting.Offset;
        DspBufferSize = gamePreSetting.DspBufferSize;
        EffectLoudness = gamePreSetting.EffectLoudness;
        EffectVolume = gamePreSetting.EffectVolume;
        EffectSize = gamePreSetting.EffectSize;
        NoteSize = gamePreSetting.NoteSize;
        CoverBlurRate = gamePreSetting.CoverBlurRate;
        CoverBrightness = gamePreSetting.CoverBrightness;
        MainAudioVolume = gamePreSetting.MainAudioVolume;
        MusicVolume = gamePreSetting.MusicVolume;
    }
}