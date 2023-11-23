using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;

#region Structer For Edit

public class InGameMusicMap
{
    public int musicId;
    public Difficulty difficulty;
    public int diffLevel;
    public List<InGameJudgeNoteInfo> judgeNotes;
    public List<EditNoteInfo> decorativeNotes;

    public InGameMusicMap()
    {

    }

    public InGameMusicMap(Difficulty difficulty)
    {
        musicId = default;
        diffLevel = default;
        this.difficulty = difficulty;
        judgeNotes = new List<InGameJudgeNoteInfo>();
        decorativeNotes = new List<EditNoteInfo>();
    }
}

public class InGameJudgeNoteInfo
{
    public JudgeType judgeType;
    [JsonIgnore]
    public JudgePair judge => judges[0];
    public (EditNoteInfo up, EditNoteInfo down) movingNotes;
    public List<JudgePair> judges;
    public HoldPathType pathType;
    public List<(float angle, float value)> pathParameters;
    /// <summary>hold的自定义曲线，每相邻两个判定点间有一个</summary>
    public List<(AnimationCurve xCurve, AnimationCurve yCurve)> HoldCurves;

    public InGameJudgeNoteInfo()
    {
        judgeType = default;
        pathType = default;
        judges = new List<JudgePair>();
        pathParameters = new List<(float angle, float value)>();
        movingNotes = (null, null);
        HoldCurves = new List<(AnimationCurve xCurve, AnimationCurve yCurve)>();
    }

    public InGameJudgeNoteInfo(InGameJudgeNoteInfo info)
    {
        judgeType = info.judgeType;
        pathType = info.pathType;
        judges = new List<JudgePair>(info.judges.Count);
        info.judges.ForEach(judge => judges.Add(new JudgePair(judge)));
        pathParameters = new List<(float angle, float value)>(info.pathParameters.Count);
        for (int i = 0; i < info.pathParameters.Count; i++)
        {
            var param = info.pathParameters[i];
            pathParameters.Add((param.angle, param.value));
        }
        movingNotes = (new EditNoteInfo(info.movingNotes.up), new EditNoteInfo(info.movingNotes.down));
        var holdCurveCount = info.HoldCurves.Count;
        HoldCurves = new List<(AnimationCurve xCurve, AnimationCurve yCurve)>(holdCurveCount);
        for (int i = 0; i < holdCurveCount; i++)
        {
            var keys = info.HoldCurves[i].xCurve.keys;
            var newXCurve = new AnimationCurve(keys);
            keys = info.HoldCurves[i].yCurve.keys;
            var newYCurve = new AnimationCurve(keys);
            HoldCurves.Add((newXCurve, newYCurve));
        }
    }

    public InGameJudgeNoteInfo(JudgeType judgeType, Vector2 judgePosition, int judgeTime, HoldPathType pathType = HoldPathType.DirectLine)
    {
        this.judgeType = judgeType;
        this.pathType = pathType;
        this.judges = new List<JudgePair>()
        {
            new JudgePair(judgePosition, judgeTime)
        };
        pathParameters = new List<(float angle, float value)>()
        {
            (0, 0)
        };
        this.movingNotes = (null, null);
        HoldCurves = new List<(AnimationCurve xCurve, AnimationCurve yCurve)>();
    }

    /// <summary>
    /// 添加判定的同时添加路径点参数
    /// </summary>
    /// <param name="judgePair">判定点</param>
    public void AddJudge(JudgePair judgePair)
    {
        AddJudge(judgePair, (0, 0));
    }

    /// <summary>
    /// 添加判定的同时添加路径点参数
    /// </summary>
    /// <param name="judgePair">判定点</param>
    /// <param name="param">曲线参数</param>
    public void AddJudge(JudgePair judgePair, (float angle, float value) param, int index = -1, (AnimationCurve xCurve, AnimationCurve yCurve)? curves = null)
    {
        judges.Add(judgePair);
        pathParameters.Add(param);

        var judgeCount = judges.Count;
        if (judgeCount > 1)
        {
            Debug.Log("添加自定义曲线");
            if (index < 0)
            {
                var lastJudgePair = judges[judges.Count - 2];
                AnimationCurve xCurve, yCurve;
                // 针对完全竖直的策略
                if (Mathf.Approximately(lastJudgePair.judgePosition.x, judgePair.judgePosition.x))
                {
                    xCurve = AnimationCurve.Linear(0, 0.5f, 1, 0.5f);
                }
                else
                {
                    xCurve = AnimationCurve.Linear(0, 0, 1, 1);
                }

                // 针对完全水平的策略
                if (Mathf.Approximately(lastJudgePair.judgePosition.y, judgePair.judgePosition.y))
                {
                    yCurve = AnimationCurve.Linear(0, 0.5f, 1, 0.5f);
                }
                else
                {
                    yCurve = AnimationCurve.Linear(0, 0, 1, 1);
                }

                HoldCurves.Add((xCurve, yCurve));
            }
            else
            {
                HoldCurves.Insert(index, curves.Value);
            }
        }

        if (judgeCount > 2)
        {
            SortJudges();
        }
    }

    public void DeleteJudge(JudgePair judgePair, out int curveIndex, out (AnimationCurve xCurve, AnimationCurve yCurve) curves)
    {
        var index = curveIndex = judges.FindIndex(judge => ReferenceEquals(judge, judgePair));
        curves = default;

        // 个数大于2，可能要删曲线
        if (judges.Count > 2)
        {
            if (index < HoldCurves.Count && index >= 0)
            {
                curves = HoldCurves[curveIndex];
                HoldCurves.RemoveAt(index);
            }
            else if (index == HoldCurves.Count - 1)
            {
                curveIndex = index - 1;
                curves = HoldCurves[curveIndex];
                HoldCurves.RemoveAt(index - 1);
            }
        }

        judges.RemoveAt(index);
        pathParameters.RemoveAt(index);
    }

    public void MoveTimeTo(int targetTime)
    {
        var offset = targetTime - judge.judgeTime;
        MoveTimeOffset(offset);
    }

    public void MoveTimeOffset(int offset)
    {
        // 首先是判定的时间
        foreach (var judge in judges)
        {
            judge.judgeTime += offset;
        }

        // 然后是半note
        movingNotes.up.MoveTimeOffset(offset);
        movingNotes.down.MoveTimeOffset(offset);
    }

    public void MovePosition(Vector2 offset)
    {
        // 首先是判定的位置
        foreach (var judge in judges)
        {
            var pos = judge.judgePosition;
            pos += offset;
            judge.judgePosition = pos;
        }

        // 然后是半note
        movingNotes.up.MovePosition(offset);
        movingNotes.down.MovePosition(offset);

        if (judgeType == JudgeType.Hold)
        {
            HoldPathEditManager.Instance.RefreshNoteHoldPath(this);
        }
    }

    /// <summary>
    /// 中心对齐镜像
    /// </summary>
    public void MirrorByCenter()
    {
        // 首先是判定
        judges.ForEach(judgePair =>
        {
            judgePair.judgePosition = Tools.GetHorizontalSymmetryPos(judgePair.judgePosition, ConstData.GameCenter.x);
        });

        // 然后是半note
        // 上半
        var pos = movingNotes.up.noteMoveInfo.p0;
        var speed = movingNotes.up.noteMoveInfo.v0;
        speed.x = -speed.x;
        movingNotes.up.noteMoveInfo.p0 = Tools.GetHorizontalSymmetryPos(pos, ConstData.GameCenter.x);
        movingNotes.up.noteMoveInfo.v0 = speed;

        // 下半
        pos = movingNotes.down.noteMoveInfo.p0;
        speed = movingNotes.down.noteMoveInfo.v0;
        speed.x = -speed.x;
        movingNotes.down.noteMoveInfo.p0 = Tools.GetHorizontalSymmetryPos(pos, ConstData.GameCenter.x);
        movingNotes.down.noteMoveInfo.v0 = speed;

        // hold还要计算hold的路径
        if (JudgeType.Hold == judgeType)
        {
            var length = pathParameters.Count;
            for (int i = 0; i < length; i++)
            {
                float newAngle;
                if (pathParameters[i].angle > 0)
                {
                    newAngle = ConstData.PI - pathParameters[i].angle;
                }
                else
                {
                    newAngle = -ConstData.PI - pathParameters[i].angle;
                }

                pathParameters[i] = (newAngle, pathParameters[i].value);
            }
        }
    }

    public void ScaleTime(float scale)
    {
        // 首先是判定的时间
        foreach (var judge in judges)
        {
            judge.judgeTime = (int)(judge.judgeTime * scale + 0.5f);
        }

        // 然后是半note
        movingNotes.up.ScaleTime(scale);
        movingNotes.down.ScaleTime(scale);
    }

    public void InitHoldCurves()
    {
        if (judgeType == JudgeType.Hold)
        {
            var curveCount = judges.Count - 1;
            HoldCurves = new List<(AnimationCurve xCurve, AnimationCurve yCurve)>(curveCount);
            for (int i = 0; i < curveCount; i++)
            {
                HoldCurves.Add((AnimationCurve.Linear(0, 0, 1, 1), AnimationCurve.Linear(0, 0, 1, 1)));
            }
        }
    }

    public void RefreshPath()
    {
        movingNotes.up.holdPath.Clear();
        movingNotes.down.holdPath.Clear();

        // 重新计算路径
        // 因为计算运动用的是本体的，所以会连带上hold路径，因此先清空，然后先算路径，再添加hold路径
        movingNotes.up.CalculatePath();
        movingNotes.down.CalculatePath();

        if (JudgeType.Hold == judgeType)
        {
            HoldPathEditManager.Instance.RefreshNoteHoldPath(this);
        }
    }

    private void SortJudges()
    {
        for (int i = 0; i < judges.Count; i++)
        {
            for (int j = 1; j < judges.Count - i; j++)
            {
                if (judges[j].judgeTime < judges[j - 1].judgeTime)
                {
                    (judges[j], judges[j - 1]) = (judges[j - 1], judges[j]);

                    (pathParameters[j], pathParameters[j - 1]) = (pathParameters[j - 1], pathParameters[j]);

                    // curve算在两个判定点的前者身上，最后一个没有
                    if (j < HoldCurves.Count)
                    {
                        (HoldCurves[j], HoldCurves[j - 1]) = (HoldCurves[j - 1], HoldCurves[j]);
                    }
                }
            }
        }
    }
}

public class EditNoteInfo : NoteInfo
{
    public Vector2[] path;
    public List<Vector2> totalPath;

    public EditNoteInfo()
    {
        path = null;
        totalPath = new List<Vector2>();
    }

    public EditNoteInfo(NoteInfo noteInfo) : base(noteInfo)
    {
        path = null;
        totalPath = new List<Vector2>();
    }

    public EditNoteInfo(EditNoteInfo editNoteInfo) : base(editNoteInfo)
    {
        path = new Vector2[editNoteInfo.path.Length];
        for (int i = 0; i < path.Length; i++)
        {
            path[i] = editNoteInfo.path[i];
        }

        totalPath = new List<Vector2>(editNoteInfo.totalPath);
        // totalPath = new List<Vector2>(editNoteInfo.totalPath.Count);
        // editNoteInfo.totalPath.ForEach(pos => totalPath.Add(pos));
    }

    public EditNoteInfo(WeightType weightType, HalfType halfType, NoteMoveInfo noteMoveInfo, List<Vector2> holdPath)
        : base(weightType, halfType, noteMoveInfo, holdPath)
    {
        path = null;
        totalPath = new List<Vector2>();
    }

    public void CalculatePath()
    {
        path = NoteMoveForEdit.CalculatePositionList(this, noteMoveInfo.startTime);
        totalPath.Clear();
        if (path != null && path.Length > 0)
        {
            totalPath.AddRange(path);
        }
        if (holdPath != null && holdPath.Count > 0)
        {
            totalPath.AddRange(holdPath);
        }
        GamePreviewManager.Instance.UpdatePreview();
    }

    public void SetHoldPath(List<Vector2> holdPath)
    {
        this.holdPath = new List<Vector2>(holdPath);
        CalculatePath();
    }

    public void SetInitPos(Vector2 p0)
    {
        noteMoveInfo.p0 = p0;
        CalculatePath();
    }

    public void SetInitV0(Vector2 v0)
    {
        noteMoveInfo.v0 = v0;
        CalculatePath();
    }

    public void MoveTimeOffset(int offset)
    {
        // 起止时间
        noteMoveInfo.startTime += offset;
        noteMoveInfo.endTime += offset;
    }

    public void MovePosition(Vector2 offset)
    {
        // 初始位置
        var pos = noteMoveInfo.p0;
        pos += offset;
        noteMoveInfo.p0 = pos;

        CalculatePath();
    }

    public void ScaleTime(float scale)
    {
        // 起止时间
        noteMoveInfo.startTime = (int)(noteMoveInfo.startTime * scale + 0.5f);
        noteMoveInfo.endTime = (int)(noteMoveInfo.endTime * scale + 0.5f);
    }
}

[Serializable]
public struct Beat : IComparable<Beat>
{
    public int IntPart;
    public int UpPart;
    public int DownPart;
    public Beat(int i, int up = 0, int down = 2)
    {
        IntPart = i;
        UpPart = up;
        DownPart = down == 0 ? 1 : down;
        IntPart += UpPart / DownPart;
        UpPart %= DownPart;
        if (UpPart != 0)
        {
            int t = Gcd(UpPart, DownPart);
            UpPart /= t;
            DownPart /= t;
        }
    }
    public float Prase()
    {
        return (float)IntPart + (float)UpPart / (float)DownPart;
    }
    public override string ToString()
    {
        return $"{IntPart}: {UpPart}/{DownPart}";
    }
    public static int Gcd(int n1, int n2)
    {
        int max = n1 > n2 ? n1 : n2;
        int min = n1 < n2 ? n1 : n2;
        int remainder;
        while (min != 0)
        {
            remainder = max % min;
            max = min;
            min = remainder;
        }
        return max;
    }
    public static int Lcm(int n1, int n2)
    {
        return n1 * n2 / Gcd(n1, n2);
    }
    public static bool operator <(Beat a, Beat b)
    {
        return a.Prase() < b.Prase();
    }
    public static bool operator >(Beat a, Beat b)
    {
        return a.Prase() > b.Prase();
    }
    public static bool operator <=(Beat a, Beat b)
    {
        return a.Prase() <= b.Prase();
    }
    public static bool operator >=(Beat a, Beat b)
    {
        return a.Prase() >= b.Prase();
    }
    public static bool operator ==(Beat a, Beat b)
    {
        // return Math.Abs(a.Prase() - b.Prase()) < 1e-5;
        return a.IntPart == b.IntPart && a.UpPart == b.UpPart && a.DownPart == b.DownPart;
    }
    public static bool operator !=(Beat a, Beat b)
    {
        return Math.Abs(a.Prase() - b.Prase()) > 1e-5;
    }
    public override bool Equals(object a)
    {
        if (a == null || GetType() != a.GetType())
        {
            return false;
        }
        return this == (Beat)a;
    }
    public override int GetHashCode()
    {
        return this.Prase().GetHashCode();
    }
    public int CompareTo(Beat b)
    {
        return this == b ? 0 : this < b ? -1 : 1;
    }
    public static Beat operator +(Beat a, Beat b)
    {
        int i = Lcm(a.DownPart, b.DownPart);
        return new Beat(a.IntPart + b.IntPart, a.UpPart * (i / a.DownPart) + b.UpPart * (i / b.DownPart), i);
    }
    public static Beat operator -(Beat a, Beat b)
    {
        int i = Lcm(a.DownPart, b.DownPart);
        int ai = a.UpPart * (i / a.DownPart);
        int bi = b.UpPart * (i / b.DownPart);
        if (ai >= bi)
        {
            return new Beat(a.IntPart - b.IntPart, ai - bi, i);
        }
        else
        {
            return new Beat(a.IntPart - b.IntPart - 1, ai + i - bi, i);
        }
    }
}

/// <summary>
/// 半note的预设信息
/// </summary>
public class HalfNotePresetInfo
{
    public string presetName;
    public Vector2 v0;
    /// <summary>轨迹的水平偏移</summary>
    public float xOffset;
    /// <summary>原始P0</summary>
    public Vector2 rawP0;
    public bool isReverse;
    /// <summary>运动的持续时间</summary>
    public int moveTime;

    public WeightType weightType;

    public HalfNotePresetInfo()
    {

    }

    public HalfNotePresetInfo(EditNoteInfo noteInfo)
    {
        weightType = noteInfo.weightType;

        v0 = noteInfo.noteMoveInfo.v0;
        rawP0 = noteInfo.noteMoveInfo.p0;

        var pathLength = noteInfo.path.Length;
        xOffset = noteInfo.path[pathLength - 1].x - noteInfo.noteMoveInfo.p0.x;
        isReverse = noteInfo.noteMoveInfo.isReverse;
        moveTime = noteInfo.noteMoveInfo.endTime - noteInfo.noteMoveInfo.startTime;
    }

    public Vector2 GetP0(Vector2 judgePos)
    {
        var pos = rawP0;
        pos.x = judgePos.x - xOffset;
        return pos;
    }

}

public class HalfNotePresetGroupInfo
{
    public int upHalfIndex;
    public int downHalfIndex;

    public HalfNotePresetGroupInfo(int upHalfIndex, int downHalfIndex)
    {
        this.upHalfIndex = upHalfIndex;
        this.downHalfIndex = downHalfIndex;
    }
}

public class PresetStructer
{
    public HalfNotePresetInfo[] halfNotePresets;
    public HalfNotePresetGroupInfo[] halfNoteGroupPresets;
}

#endregion

#region Structer In Game

#region Enum

public enum Difficulty
{
    Tale,
    Romance,
    History,
    Story,
    Revival
}

public enum JudgeType
{
    Tap,
    Hold,
    Flick
}

public enum WeightType
{
    Light,
    Middle,
    Weight
}

public enum HalfType
{
    UpHalf,
    DownHalf
}

#endregion

public class JudgePair
{
    public Vector2 judgePosition;
    public int judgeTime;
    public JudgePair()
    {
        judgePosition = default;
        judgeTime = default;
    }

    public JudgePair(JudgePair judgePair)
    {
        judgePosition = judgePair.judgePosition;
        judgeTime = judgePair.judgeTime;
    }

    public JudgePair(Vector2 judgePosition, int judgeTime)
    {
        this.judgePosition = judgePosition;
        this.judgeTime = judgeTime;
    }

    public override bool Equals(object obj)
    {
        var judgePair = obj as JudgePair;
        if (judgePair == null)
        {
            return false;
        }

        return this == judgePair || (judgeTime == judgePair.judgeTime && judgePosition.Equals(judgePair.judgePosition));
    }

    public bool Equals(JudgePair other)
    {
        return this == other || (judgePosition.Equals(other.judgePosition) && judgeTime == other.judgeTime);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (judgePosition.GetHashCode() * 397) ^ judgeTime;
        }
    }
}

public class NoteInfo
{
    public WeightType weightType;
    public HalfType halfType;
    public NoteMoveInfo noteMoveInfo;
    public List<Vector2> holdPath;
    public NoteInfo()
    {
        weightType = default;
        halfType = default;
        noteMoveInfo = default;
        holdPath = default;
    }
    public NoteInfo(NoteInfo info)
    {
        try
        {
            weightType = info.weightType;
            halfType = info.halfType;
            noteMoveInfo = new NoteMoveInfo(info.noteMoveInfo);
            holdPath = new List<Vector2>(info.holdPath);
        }
        catch (Exception e)
        {
            Debug.LogError($"info is null? : {info is null}");
            throw;
        }
    }
    public NoteInfo(WeightType weightType, HalfType halfType, NoteMoveInfo noteMoveInfo, List<Vector2> holdPath)
    {
        this.weightType = weightType;
        this.halfType = halfType;
        this.noteMoveInfo = noteMoveInfo ?? throw new ArgumentNullException(nameof(noteMoveInfo));
        this.holdPath = holdPath;
    }
    public NoteInfo(NoteInfo info, List<JudgePair> path)
    {
        this.weightType = info.weightType;
        this.halfType = info.halfType;
        this.noteMoveInfo = info.noteMoveInfo;
        this.holdPath = new List<Vector2>();
        path.ForEach(i => this.holdPath.Add(i.judgePosition));
    }

    public void Copy(NoteInfo info)
    {
        weightType = info.weightType;
        halfType = info.halfType;
        noteMoveInfo = info.noteMoveInfo;
        holdPath = new List<Vector2>(info.holdPath);
    }
}

public class NoteMoveInfo
{
    public Vector2 v0;
    public Vector2 p0;
    public int startTime;
    public int endTime;
    public bool isReverse;

    public NoteMoveInfo()
    {
        v0 = default;
        p0 = default;
        startTime = default;
        endTime = default;
        isReverse = default;
    }

    public NoteMoveInfo(NoteMoveInfo info)
    {
        v0 = info.v0;
        p0 = info.p0;
        startTime = info.startTime;
        endTime = info.endTime;
        isReverse = info.isReverse;
    }
    public NoteMoveInfo(Vector2 v0, Vector2 p0, int startTime, int endTime, bool isReverse)
    {
        this.v0 = v0;
        this.p0 = p0;
        this.startTime = startTime;
        this.endTime = endTime;
        this.isReverse = isReverse;
    }
}

public class MusicMap
{
    public int musicId;
    public Difficulty difficulty;
    public int diffLevel;
    public List<JudgeNoteInfo> judgeNotes;
    public List<NoteInfo> decorativeNotes;
    public MusicMap()
    {
        musicId = default;
        diffLevel = default;
        difficulty = default;
        judgeNotes = new List<JudgeNoteInfo>();
        decorativeNotes = new List<NoteInfo>();
    }
    public MusicMap(InGameMusicMap tMap)
    {
        musicId = tMap.musicId;
        difficulty = tMap.difficulty;
        diffLevel = tMap.diffLevel;
        judgeNotes = new List<JudgeNoteInfo>();
        decorativeNotes = new List<NoteInfo>();
        tMap.judgeNotes.ForEach(judgeNote =>
        {
            judgeNotes.Add(new JudgeNoteInfo(judgeNote.judgeType, judgeNote.movingNotes.up, judgeNote.movingNotes.down, judgeNote.judges));
        });
    }
}

public class JudgeNoteInfo
{
    public JudgeType judgeType;
    public NoteInfo halfNote1;
    public NoteInfo halfNote2;
    public List<JudgePair> judges;
    public JudgeNoteInfo()
    {
        judgeType = default;
        halfNote1 = default;
        halfNote2 = default;
        judges = default;
    }
    public JudgeNoteInfo(JudgeType type, NoteInfo note1, NoteInfo note2, List<JudgePair> tJs)
    {
        try
        {
            judgeType = type;
            halfNote1 = new NoteInfo(note1);
            halfNote2 = new NoteInfo(note2);
            judges = tJs;
        }
        catch (Exception e)
        {
            Debug.Log($"构建判定：type: {type}, first judge time: {tJs[0].judgeTime}, half note1 is null?: {note1 is null}, half note2 is null?: {note2 is null}");
            Debug.Log($"上半：重量: {note1.weightType}, 类型: {note1.halfType}, 运动空否？: {note1.noteMoveInfo is null}, 路径空否?: {note1.holdPath is null}");
            Debug.Log($"下半：重量: {note2.weightType}, 类型: {note2.halfType}, 运动空否？: {note2.noteMoveInfo is null}, 路径空否?: {note2.holdPath is null}");
            throw;
        }
    }

    public int GetEarlyestHalfNoteTime()
    {
        return Mathf.Min(halfNote1.noteMoveInfo.startTime, halfNote2.noteMoveInfo.startTime);
    }
}

#endregion