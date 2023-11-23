using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 工具类
/// </summary>
public static class Tools
{
    public static float TransStringToFloat(string str, float defaultValue = 0)
    {
        if (float.TryParse(str, out var tValue))
        {
            return tValue;
        }
        return defaultValue;
    }

    public static int TransStringToInt(string str, int defaultValue = 0)
    {
        if (int.TryParse(str, out var tValue))
        {
            return tValue;
        }
        return defaultValue;
    }

    /// <summary>
    /// 节拍转换成时间
    /// </summary>
    /// <param name="beat">节拍</param>
    /// <param name="BPMTable">bpm表</param>
    /// <returns>时间</returns>
    public static float TransBeatToTime(Beat beat, List<(Beat beat, float bpm)> BPMTable)
    {
        if (beat.Prase() <= 0)
        {
            return TransSimpleBeatToTime(beat, BPMTable[0].bpm);
        }
        else
        {
            float tTime = 0;
            for (int i = 0; i < BPMTable.Count - 1; ++i)
            {
                if (beat > BPMTable[i + 1].beat)
                {
                    tTime += TransSimpleBeatToTime(BPMTable[i + 1].beat - BPMTable[i].beat, BPMTable[i].bpm);
                }
                else
                {
                    tTime += TransSimpleBeatToTime(beat - BPMTable[i].beat, BPMTable[i].bpm);
                    return tTime;
                }
            }
            tTime += TransSimpleBeatToTime(beat - BPMTable[BPMTable.Count - 1].beat, BPMTable[BPMTable.Count - 1].bpm);
            return tTime;
        }
    }

    /// <summary>
    /// 时间转换成节拍
    /// 当前小节数，是小数，代表着当前时间所在的小节位置
    /// </summary>
    /// <param name="time"></param>
    /// <param name="BPMTable"></param>
    /// <returns></returns>
    public static float TransTimeToBeat(float time, List<(Beat beat, float bpm)> BPMTable, out float bpm)
    {
        List<(float time, float bpm)> tList = TransBpmTableToTimeBaseTable(BPMTable);
        if (time <= 0)
        {
            bpm = tList[0].bpm;
            return time * tList[0].bpm / 60000.0f;
        }
        for (int i = 0; i < tList.Count - 1; ++i)
        {
            if (time < tList[i + 1].time)
            {
                bpm = tList[i].bpm;
                return BPMTable[i].beat.Prase() + (time - tList[i].time) * tList[i].bpm / 60000.0f;
            }
        }

        var timeGroup = tList[tList.Count - 1];
        bpm = timeGroup.bpm;
        return BPMTable[BPMTable.Count - 1].beat.Prase() + (time - timeGroup.time) * timeGroup.bpm / 60000.0f;
    }

    /// <summary>
    /// BPM表转换成时间为基准
    /// </summary>
    /// <param name="BPMTable">拍子bpm表</param>
    /// <returns>时间bpm表</returns>
    public static List<(float time, float bpm)> TransBpmTableToTimeBaseTable(List<(Beat beat, float bpm)> BPMTable)
    {
        List<(float time, float bpm)> tList = new List<(float time, float bpm)>();
        BPMTable.ForEach(i => tList.Add((TransBeatToTime(i.beat, BPMTable), i.bpm)));
        return tList;
    }

    /// <summary>
    /// 简易节拍转换成时间
    /// </summary>
    /// <param name="beat">节拍</param>
    /// <param name="BPM">bpm</param>
    /// <returns></returns>
    public static float TransSimpleBeatToTime(Beat beat, float BPM)
    {
        return 60000.0f * beat.Prase() / BPM;
    }

    /// <summary>
    /// 格式化时间
    /// </summary>
    /// <param name="time">时间</param>
    /// <returns>格式化后的时间字符串</returns>
    public static string TimeFormat(float time)
    {
        return $"{(int)(time / 60):0}:{(int)time % 60:00}:{((time % 1) * 1000):000}";
    }

    /// <summary>
    /// 格式化时间
    /// </summary>
    /// <param name="time">毫秒的时间</param>
    /// <returns>格式化后的时间字符串</returns>
    public static string TimeFormat(int time)
    {
        var second = time / 1000;
        return $"{(second / 60):0}:{second % 60:00}:{(time % 1000):000}";
    }

    /// <summary>
    /// 从窗口到1080p的位置缩放（位置正向转换）
    /// </summary>
    /// <param name="pos">位置</param>
    /// <param name="startPoint">起始端点</param>
    /// <param name="length">长度</param>
    /// <param name="rawLength">原始长度</param>
    /// <returns>转换后位置</returns>
    public static float ScalePosToRawPos(float pos, float startPoint, float length, float rawLength)
    {
        return (pos - startPoint) / length * rawLength;
    }

    /// <summary>
    /// 从1080p到窗口的位置缩放（位置反向转换）
    /// </summary>
    /// <param name="pos">位置</param>
    /// <param name="startPoint">起始端点</param>
    /// <param name="length">长度</param>
    /// <param name="rawLength">原始长度</param>
    /// <returns>转换后位置</returns>
    public static float ScaleRawPosToPos(float pos, float startPoint, float length, float rawLength)
    {
        return pos / rawLength * length + startPoint;
    }

    /// <summary>
    /// 值是否处于中点范围内
    /// </summary>
    /// <param name="value">检查值</param>
    /// <param name="center">中点</param>
    /// <param name="radius">半径</param>
    /// <returns>是否处于中点范围内</returns>
    public static bool IsInCenterRange(float value, float center, float radius)
    {
        return IsInRange(value, center - radius, center + radius);
    }

    /// <summary>
    /// 值是否处于中点范围内（坐标）
    /// </summary>
    /// <param name="value">检查值</param>
    /// <param name="center">中点</param>
    /// <param name="radius">半径</param>
    /// <returns>是否处于中点范围内</returns>
    public static bool IsInCenterRange(Vector2 value, Vector2 center, float radius)
    {
        var distance = (value - center).magnitude;
        return distance <= radius;
    }

    /// <summary>
    /// 值是否处于距离下限的一定距离内
    /// </summary>
    /// <param name="value">值</param>
    /// <param name="min">下限</param>
    /// <param name="length">距离</param>
    /// <returns></returns>
    public static bool IsInRadiusRange(float value, float min, float length)
    {
        return IsInRange(value, min, min + length);
    }

    /// <summary>
    /// 值是否处于距离下限的一定距离内
    /// </summary>
    /// <param name="value">值</param>
    /// <param name="min">下限</param>
    /// <param name="length">距离</param>
    /// <returns></returns>
    public static bool IsInRadiusRange(int value, int min, int length)
    {
        return IsInRange(value, min, min + length);
    }

    /// <summary>
    /// 值是否处于指定范围内
    /// </summary>
    /// <param name="value">值</param>
    /// <param name="min">下限</param>
    /// <param name="max">上限</param>
    /// <returns>是否处于指定范围内</returns>
    public static bool IsInRange(float value, float min, float max)
    {
        return value - min > 1e-3 && max - value >= 1e-3;
    }

    /// <summary>
    /// 值是否处于指定范围内
    /// </summary>
    /// <param name="value">值</param>
    /// <param name="min">下限</param>
    /// <param name="max">上限</param>
    /// <returns>是否处于指定范围内</returns>
    public static bool IsInRange(int value, int min, int max)
    {
        return min <= value && value <= max;
    }

    /// <summary>
    /// 值是否在指定范围内（不规定两端的大小值）
    /// </summary>
    /// <param name="value"></param>
    /// <param name="range1"></param>
    /// <param name="range2"></param>
    /// <returns>处于指定范围内</returns>
    public static bool IsInArea(float value, float range1, float range2)
    {
        var res1 = value - range1 > 0;
        var res2 = value - range2 > 0;
        return res1 ^ res2;
    }

    /// <summary>
    /// 将0-1的值缩放到实际的区间
    /// </summary>
    /// <param name="value"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static float Scale01ValueToRealRange(float value, float min, float max)
    {
        return (max - min) * value + min;
    }

    /// <summary>
    /// 将值按实际的区间单位化（0-1）
    /// </summary>
    /// <param name="value"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static float NormalizeValue(float value, float min, float max)
    {
        return (value - min) / (max - min);
    }

    public static bool IsInPath(float value, IEnumerable<float> path)
    {
        return path.Any(v => v > value) && path.Any(v => v < value);
    }

    public static bool IsInPath<T>(float value, IEnumerable<T> path, Func<T, float> selectFunc)
    {
        var valuePath = path.Select(v => selectFunc(v));
        return valuePath.Any(v => v > value) && valuePath.Any(v => v < value);
    }

    /// <summary>
    /// 获取关于<paramref name="symmetryValue"/>的水平对称坐标
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="symmetryValue"></param>
    /// <returns></returns>
    public static Vector2 GetHorizontalSymmetryPos(Vector2 pos, float symmetryValue)
    {
        var offset = symmetryValue - pos.x;
        pos.x = symmetryValue + offset;
        return pos;
    }

    /// <summary>
    /// 缩放游戏场景内坐标到游戏窗口坐标（左下角为原点）
    /// </summary>
    /// <param name="pos">实际场景内坐标</param>
    /// <returns>游戏窗口坐标</returns>
    public static Vector2 ScaleToGameWindowPosition(Vector2 pos)
    {
        return (pos + EditingData.RealGameZeroPos) / EditingData.GameSceneScale;
    }

    /// <summary>
    /// 缩放游戏窗口坐标到游戏场景内坐标
    /// </summary>
    /// <param name="pos">窗口内坐标</param>
    /// <returns>实际游戏场景内坐标</returns>
    public static Vector2 ScaleToGameScenePosition(Vector2 pos)
    {
        return pos * EditingData.GameSceneScale - EditingData.RealGameZeroPos;
    }

    /// <summary>
    /// 按网格约束坐标，网格的两轴步进为常量
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public static Vector2 LimitPosition(Vector2 pos)
    {
        var limit = ConstData.LimitPos;
        var offsetX = pos.x % limit.x;
        var offsetY = pos.y % limit.y;
        var intX = pos.x - offsetX;
        var intY = pos.y - offsetY;

        return new Vector2(intX + limit.x * (int)(offsetX / limit.x + 0.5f), intY + limit.y * (int)(offsetY / limit.y + 0.5f));
    }
}
