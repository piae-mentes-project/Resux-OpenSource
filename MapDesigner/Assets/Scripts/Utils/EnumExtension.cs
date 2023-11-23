using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ö����չ
/// </summary>
public static class EnumExtension
{
    public static string GetName(this JudgeType judgeType)
    {
        switch (judgeType)
        {
            case JudgeType.Tap:
                return "���";
            case JudgeType.Hold:
                return "����";
            case JudgeType.Flick:
                return "����";
            default:
                return "";
        }
    }

    public static string GetName(this Difficulty difficulty)
    {
        switch (difficulty)
        {
            case Difficulty.Tale:
                return "Ц̸";
            case Difficulty.Romance:
                return "����";
            case Difficulty.History:
                return "ʷʵ";
            case Difficulty.Story:
                return "����";
            case Difficulty.Revival:
                return "����";
            default:
                return "";
        }
    }

    public static string GetName(this HoldPathType pathType)
    {
        switch (pathType)
        {
            case HoldPathType.DirectLine:
                return "ֱ��";
            case HoldPathType.BezierLine:
                return "����������";
            case HoldPathType.Curve:
                return "�Զ�������";
            default:
                return "unknown";
        }
    }
}
