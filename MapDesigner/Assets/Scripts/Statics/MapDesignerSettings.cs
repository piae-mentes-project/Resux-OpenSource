using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// 制谱器的工程设置
/// </summary>
public static class MapDesignerSettings
{
    #region inner class

    /// <summary>
    /// 保存用
    /// </summary>
    public class MapDesignerProjectSetting
    {
        public List<(Beat beat, float bpm)> BpmList;
        public int Delay;
        public string MusicName;
        /// <summary>音乐扩展名</summary>
        public string Ext;

        public MapDesignerProjectSetting()
        {
            BpmList = new List<(Beat beat, float bpm)>();
        }
    }

    #endregion

    #region Properties

    public const string FileName = "ProjectSetting.json";

    public static List<(Beat beat, float bpm)> BpmList;
    public static int Delay;
    public static string MusicName;
    public static string Ext;

    #region 无需保存的内容

    public static string ProjectPath;
    public static string ProjectOutPath;

    #endregion

    #endregion

    static MapDesignerSettings()
    {
        BpmList = new List<(Beat beat, float bpm)>();
        MusicName = "";
        Ext = "ogg";
    }

    #region Public Method

    public static void SortBPM()
    {
        BpmList.Sort((left, right) =>
        {
            return left.beat.CompareTo(right.beat);
        });
    }

    public static void SaveSettings()
    {
        var path = $"{ProjectPath}/{FileName}";
        var setting = new MapDesignerProjectSetting()
        {
            BpmList = BpmList,
            Delay = Delay,
            MusicName = MusicName,
            Ext = Ext
        };
        File.WriteAllText(path, JsonConvert.SerializeObject(setting));
    }

    public static void LoadSettings(string path)
    {
        MapDesignerProjectSetting setting = LoadProjectSettings(path);
        LoadSettings(setting);
    }

    public static void LoadSettings(MapDesignerProjectSetting setting)
    {
        BpmList = setting.BpmList;
        Delay = setting.Delay;
        MusicName = setting.MusicName;
        Ext = setting.Ext;
    }

    public static MapDesignerProjectSetting LoadProjectSettings(string path)
    {
        var content = File.ReadAllText(path);
        var setting = JsonConvert.DeserializeObject<MapDesignerProjectSetting>(content);
        if (string.IsNullOrEmpty(setting.Ext))
        {
            setting.Ext = "ogg";
        }
        return setting;
    }

    #endregion
}
