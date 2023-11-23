using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 枚举扩展
/// </summary>
public static class EnumExtension
{
    public static string GetName(this JudgeType judgeType)
    {
        switch (judgeType)
        {
            case JudgeType.Tap:
                return "点击";
            case JudgeType.Hold:
                return "长按";
            case JudgeType.Flick:
                return "划动";
            default:
                return "";
        }
    }

    public static string GetName(this Difficulty difficulty)
    {
        switch (difficulty)
        {
            case Difficulty.Tale:
                return "笑谈";
            case Difficulty.Romance:
                return "演义";
            case Difficulty.History:
                return "史实";
            case Difficulty.Story:
                return "剧情";
            case Difficulty.Revival:
                return "重现";
            default:
                return "";
        }
    }

    public static string GetName(this HoldPathType pathType)
    {
        switch (pathType)
        {
            case HoldPathType.DirectLine:
                return "直线";
            case HoldPathType.BezierLine:
                return "贝塞尔曲线";
            case HoldPathType.Curve:
                return "自定义曲线";
            default:
                return "unknown";
        }
    }
}
