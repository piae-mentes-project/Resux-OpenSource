using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldPathEditManager : MonoBehaviour
{
    #region Static

    public static HoldPathEditManager Instance;

    #endregion

    #region properties

    

    #endregion

    #region Unity

    private void Awake()
    {
        Instance = this;
    }

    #endregion

    #region Public Method

    public void RefreshNoteHoldPath(InGameJudgeNoteInfo holdJudgeNote = null)
    {
        var judgeNote = holdJudgeNote ?? EditingData.CurrentEditingJudgeNote;
        var judges = judgeNote.judges;
        var parameters = judgeNote.pathParameters;
        var curves = judgeNote.HoldCurves;
        var upNote = judgeNote.movingNotes.up;
        var downNote = judgeNote.movingNotes.down;

        List<Vector2> path;
        switch (judgeNote.pathType)
        {
            case HoldPathType.DirectLine:
                path = CalculateDirectLinePath(judges);
                break;
            case HoldPathType.BezierLine:
                path = CalculateCurvePath(parameters, judges);
                break;
            case HoldPathType.Curve:
                path = CalculateAnimationCurvePath(curves, judges);
                break;
            default:
                path = new List<Vector2>();
                break;
        }

        upNote.SetHoldPath(path);
        downNote.SetHoldPath(path);

        SceneImageRenderManager.Instance.RefreshHoldJudgePathPreview();
    }

    #endregion

    #region Private Method

    private List<Vector2> CalculateAnimationCurvePath(List<(AnimationCurve xCurve, AnimationCurve yCurve)> curves, List<JudgePair> judges)
    {
        List<Vector2> path = new List<Vector2>();

        var currentTime = judges[0].judgeTime + 10;
        var endTime = judges[judges.Count - 1].judgeTime;
        // 10msһ��
        for (int i = 1; currentTime <= endTime && i < judges.Count; currentTime += 10)
        {
            if (currentTime > judges[i].judgeTime)
            {
                i++;
            }

            var currentCurves = curves[i - 1];

            var judge0 = judges[i - 1];
            var judge1 = judges[i];
            var pos0 = judge0.judgePosition;
            var pos1 = judge1.judgePosition;

            var t = (currentTime - judge0.judgeTime) / (float)(judge1.judgeTime - judge0.judgeTime);
            float x, y;
            // ����ȫ��ֱ��ȡ��Ӧ�Դ�ʩ
            if (Mathf.Approximately(pos0.x, pos1.x))
            {
                x = (currentCurves.xCurve.Evaluate(t) - 0.5f) * 720 + pos0.x;
            }
            else
            {
                x = currentCurves.xCurve.Evaluate(t) * (pos1.x - pos0.x) + pos0.x;
            }

            // ����ȫˮƽ��ȡ��Ӧ�Դ�ʩ
            if (Mathf.Approximately(pos0.y, pos1.y))
            {
                y = (currentCurves.yCurve.Evaluate(t) - 0.5f) * 720 + pos0.y;
            }
            else
            {
                y = currentCurves.yCurve.Evaluate(t) * (pos1.y - pos0.y) + pos0.y;
            }

            path.Add(new Vector2(x, y));
        }

        return path;
    }

    private List<Vector2> CalculateCurvePath(List<(float angle, float value)> parameters, List<JudgePair> judges)
    {
        List<Vector2> path = new List<Vector2>();
        var currentTime = judges[0].judgeTime + 10;
        var endTime = judges[judges.Count - 1].judgeTime;
        // 10msһ��
        for (int i = 1; currentTime <= endTime && i < judges.Count; currentTime += 10)
        {
            if (currentTime > judges[i].judgeTime)
            {
                i++;
            }

            var judge0 = judges[i - 1];
            var judge1 = judges[i];

            var t = (currentTime - judge0.judgeTime) / (float)(judge1.judgeTime - judge0.judgeTime);
            path.Add(GetPositionFromBezierCurve(judge0, judge1, parameters[i - 1], parameters[i], t));
        }

        return path;
    }

    private List<Vector2> CalculateDirectLinePath(List<JudgePair> judges)
    {
        List<Vector2> path = new List<Vector2>();
        var currentTime = judges[0].judgeTime + 10;
        var endTime = judges[judges.Count - 1].judgeTime;
        // 10msһ��
        for (int i = 1; currentTime <= endTime && i < judges.Count; currentTime += 10)
        {
            if (currentTime > judges[i].judgeTime)
            {
                i++;
            }

            var judge0 = judges[i - 1];
            var judge1 = judges[i];

            var t = (currentTime - judge0.judgeTime) / (float)(judge1.judgeTime - judge0.judgeTime);
            path.Add(Vector2.Lerp(judge0.judgePosition, judge1.judgePosition, t));
        }

        return path;
    }

    /// <summary>
    /// ���ױ���������
    /// </summary>
    /// <param name="start">��ʼ���ж��㣨p0��</param>
    /// <param name="end">�������ж��㣨p3��</param>
    /// <param name="startParam">��ʼ�˲��������ڼ�����Ƶ�p1��</param>
    /// <param name="endParam">�����˲��������ڼ�����Ƶ�p2��</param>
    /// <param name="t">�����е�λ�� t��[0,1]</param>
    /// <returns>�����ж�Ӧ�ĵ�λ��</returns>
    private Vector2 GetPositionFromBezierCurve(JudgePair start, JudgePair end, (float angle, float value) startParam, (float angle, float value) endParam, float t)
    {
        Vector2 p0 = start.judgePosition;
        Vector2 p3 = end.judgePosition;

        // ������Ƶ�
        // ��ʼ�����������Ҫ���յ�ķ�������
        var startAngle = startParam.angle;
        var endAngle = -endParam.angle;
        Vector2 p1 = p0 + new Vector2(Mathf.Cos(startAngle), Mathf.Sin(startAngle)) * startParam.value;
        Vector2 p2 = p3 + new Vector2(Mathf.Cos(endAngle), Mathf.Sin(endAngle)) * endParam.value;

        // ���������߽��⣺https://www.jianshu.com/p/55099e3a2899
        return Mathf.Pow(1 - t, 3) * p0 + 3 * t * Mathf.Pow(1 - t, 2) * p1 + 3 * t * t * (1 - t) * p2 + t * t * t * p3;
    }

    #endregion
}
