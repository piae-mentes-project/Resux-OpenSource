using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

/// <summary>
/// 总体设置
/// </summary>
public static class GlobalSettings
{
    /// <summary>谱面数据的保存位置</summary>
    public static string MapDataPath { get; set; }
    /// <summary>音乐音量</summary>
    public static float MusicVolume { get; set; }
    /// <summary>打击音音量</summary>
    public static float TapAudioVolume { get; set; }
    /// <summary>自动保存间隔（分钟）</summary>
    public static float AutoSaveInterval { get; set; }
    /// <summary>自动保存数量</summary>
    public static int AutoSaveCount { get; set; }
    /// <summary>最大预览小节数量</summary>
    public static int MaxPreviewMeasureCount { get; set; }
    /// <summary>打击特效大小</summary>
    public static float EffectSize { get; set; }
    /// <summary>是否启用网格吸附</summary>
    public static bool IsPosLimitActive { get; set; }
    /// <summary>自动游玩是否开启</summary>
    public static bool IsPreviewAutoPlayOn { get; set; }

    static GlobalSettings()
    {
        
    }

    /// <summary>
    /// 加载设置
    /// </summary>
    public static void LoadSettings()
    {
        var defaultPath = $"{System.Environment.CurrentDirectory}/Datas";
        var currentPath = PlayerPrefs.GetString(ConstData.PlayerRefKey.MapDataPathKey, defaultPath);
        if (!Directory.Exists(currentPath))
        {
            currentPath = defaultPath;
        }
        MapDataPath = currentPath;
        MusicVolume = PlayerPrefs.GetFloat(ConstData.PlayerRefKey.MusicVolumeKey, 1.0f);
        TapAudioVolume = PlayerPrefs.GetFloat(ConstData.PlayerRefKey.TapAudioVolumeKey, 1.0f);
        AutoSaveInterval = PlayerPrefs.GetFloat(ConstData.PlayerRefKey.AutoSaveIntervalKey, 2);
        AutoSaveCount = PlayerPrefs.GetInt(ConstData.PlayerRefKey.AutoSaveCountKey, 3);
        MaxPreviewMeasureCount = PlayerPrefs.GetInt(ConstData.PlayerRefKey.MaxPreviewMeasureCountKey, 100);
        EffectSize = PlayerPrefs.GetFloat(ConstData.PlayerRefKey.EffectSizeKey, 1.5f);
        IsPosLimitActive = PlayerPrefs.GetInt(ConstData.PlayerRefKey.IsPosLimitKey, 0) > 0;
        IsPreviewAutoPlayOn = PlayerPrefs.GetInt(ConstData.PlayerRefKey.PreviewAutoPlayKey, 0) > 0;
    }

    /// <summary>
    /// 保存设置
    /// </summary>
    public static void SaveSettings()
    {
        PlayerPrefs.SetString(ConstData.PlayerRefKey.MapDataPathKey, MapDataPath);
        PlayerPrefs.SetFloat(ConstData.PlayerRefKey.MusicVolumeKey, MusicVolume);
        PlayerPrefs.SetFloat(ConstData.PlayerRefKey.TapAudioVolumeKey, TapAudioVolume);
        PlayerPrefs.SetFloat(ConstData.PlayerRefKey.AutoSaveIntervalKey, AutoSaveInterval);
        PlayerPrefs.SetInt(ConstData.PlayerRefKey.AutoSaveCountKey, AutoSaveCount);
        PlayerPrefs.SetInt(ConstData.PlayerRefKey.MaxPreviewMeasureCountKey, MaxPreviewMeasureCount);
        PlayerPrefs.SetFloat(ConstData.PlayerRefKey.EffectSizeKey, EffectSize);
        PlayerPrefs.SetInt(ConstData.PlayerRefKey.IsPosLimitKey, IsPosLimitActive ? 1 : 0);
        PlayerPrefs.SetInt(ConstData.PlayerRefKey.PreviewAutoPlayKey, IsPreviewAutoPlayOn ? 1 : 0);
    }
}