using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ����
/// </summary>
public static class ConstData
{
    #region ����ָ��һЩ����

    public static class PlayerRefKey
    {
        public const string MapDataPathKey = "MapDataPath";
        public const string MusicVolumeKey = "MusicVolume";
        public const string TapAudioVolumeKey = "EffectVolume";
        public const string AutoSaveIntervalKey = "AutoSaveInterval";
        public const string AutoSaveCountKey = "AutoSaveCount";
        public const string MaxPreviewMeasureCountKey = "MaxPreviewMeasureCount";
        public const string EffectSizeKey = "EffectSize";
        public const string IsPosLimitKey = "IsPosLimit";
        public const string PreviewAutoPlayKey = "PreviewAutoPlayOn";
        public const string MapDesignerIdKey = "MapDesignerId";
    }

    #endregion

    #region other

    public const string Application = "resux.mapdesigner";

    #endregion

    #region �������ʾ

    public const float PI = 3.141592654f;
    public const float PI2 = 2 * PI;
    /// <summary>���ı�</summary>
    public static List<int> BeatList = new List<int> { 2, 3, 4, 6, 8, 12, 16, 24, 32 };
    /// <summary>�����ϸ��</summary>
    // public static List<int> CoordinateThicknessList = new List<int> { 0, 1, 2, 5, 10, 20 };
    /// <summary>������ɫ</summary>
    public static List<List<Color>> BeatColors = new List<List<Color>>
    {
        new List<Color>{Color.white, Color.blue},
        new List<Color>{Color.white, Color.green, Color.green},
        new List<Color>{Color.white, new Color(0.5f, 0, 0.5f), Color.blue, new Color(0.5f, 0, 0.5f)},
        new List<Color>{Color.white, Color.green, Color.green, Color.blue, Color.green, Color.green},
        new List<Color>{Color.white, Color.grey, new Color(0.5f, 0, 0.5f), Color.grey, Color.blue, Color.grey, new Color(0.5f, 0, 0.5f), Color.grey},
        new List<Color>{Color.white, Color.grey, Color.green, new Color(0.5f, 0, 0.5f), Color.green, Color.grey, Color.blue, Color.grey, Color.green, new Color(0.5f, 0, 0.5f), Color.green, Color.grey},
        new List<Color>{Color.white, Color.grey, Color.grey, Color.grey, new Color(0.5f, 0, 0.5f), Color.grey, Color.grey, Color.grey, Color.blue, Color.grey, Color.grey, Color.grey, new Color(0.5f, 0, 0.5f), Color.grey, Color.grey, Color.grey},
        new List<Color>{Color.white, Color.grey, Color.grey, Color.grey, Color.green, Color.grey, new Color(0.5f, 0, 0.5f), Color.grey, Color.green, Color.grey, Color.grey, Color.grey, Color.blue, Color.grey, Color.grey, Color.grey, Color.green, Color.grey, new Color(0.5f, 0, 0.5f), Color.grey, Color.green, Color.grey, Color.grey, Color.grey},
        new List<Color>{Color.white, Color.grey, Color.grey, Color.grey, Color.grey, Color.grey, Color.grey, Color.grey, new Color(0.5f, 0, 0.5f), Color.grey, Color.grey, Color.grey, Color.grey, Color.grey, Color.grey, Color.grey, Color.blue, Color.grey, Color.grey, Color.grey, Color.grey, Color.grey, Color.grey, Color.grey, new Color(0.5f, 0, 0.5f), Color.grey, Color.grey, Color.grey, Color.grey, Color.grey, Color.grey, Color.grey},
    };

    public static Dictionary<JudgeType, Color> JudgeNoteColor = new Dictionary<JudgeType, Color>()
    {
        {JudgeType.Tap, Color.blue},
        {JudgeType.Flick, Color.yellow},
        {JudgeType.Hold, Color.green}
    };
    /// <summary>ͼƬ����</summary>
    public static List<string> ImageNames = new List<string> { };

    #endregion

    #region ��Ϸ�������

    public const int WaterFaceHeight = 720;
    public const int HalfNoteClickRadius = 100;
    public const int JudgePointClickRadius = 20;

    // 0.1% - 99%����ֵ����1000
    public const int MaxLevel = 990;
    public const int MinLevel = 1;

    #endregion

    #region �༭�������

    public static (float min, float max) HorizontalSpeedRange { get; }
    public static (float min, float max) VerticalSpeedRange { get; }
    public static (float min, float max) PositionXRange { get; }
    public static (float min, float max) PositionYRange { get; }
    /// <summary>�Ƕ�Ϊ��x�������������ʱ�룬ͬ������ϵ�ļ��㷽ʽ�������ƣ�</summary>
    public static (float min, float max) AngleRange { get; }
    /// <summary>hold·�����������ֵ�ķ�Χ</summary>
    public static (int min, int max) ValueRange { get; }

    /// <summary>��Ϸ������������꣨����Ϸ����ϵ�ڣ�</summary>
    public static Vector2 GameCenter { get; }

    public static Color EditingHalfNoteColor { get; }
    public static Color NotEditingHalfNoteColor { get; }

    /// <summary>�϶��Ļ���ʱ��</summary>
    public const float dragDelayTime = 0.2f;

    /// <summary>�Զ��������ķ�Χ�����ӣ�</summary>
    public static (int min, int max) AutoSaveIntervalRange { get; }

    /// <summary>�Զ������������������ݣ�</summary>
    public static (int min, int max) AutoSaveCountRange { get; }

    /// <summary>���Ԥ��С������Χ</summary>
    public static (int min, int max) MaxPreviewMeasureCount { get; }

    /// <summary>�����Ч��С��Χ</summary>
    public static (double min, double max) EffectSizeRange { get; }

    /// <summary>���������ļ��/����</summary>
    public static (int x, int y) LimitPos { get; }

    #endregion

    static ConstData()
    {
        HorizontalSpeedRange = (-1600, 1600);
        VerticalSpeedRange = (-1600, 1600);
        PositionXRange = (-960, 2880);
        PositionYRange = (-540, 1620);
        AngleRange = (-PI, PI);
        ValueRange = (50, 1000);

        GameCenter = new Vector2(960, 540);

        EditingHalfNoteColor = Color.white;
        NotEditingHalfNoteColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);

        AutoSaveIntervalRange = (1, 10);
        AutoSaveCountRange = (1, 10);
        EffectSizeRange = (0.5, 1.5);
        MaxPreviewMeasureCount = (50, 150);

        LimitPos = (20, 10);
    }
}