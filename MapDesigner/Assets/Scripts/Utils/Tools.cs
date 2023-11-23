using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// ������
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
    /// ����ת����ʱ��
    /// </summary>
    /// <param name="beat">����</param>
    /// <param name="BPMTable">bpm��</param>
    /// <returns>ʱ��</returns>
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
    /// ʱ��ת���ɽ���
    /// ��ǰС��������С���������ŵ�ǰʱ�����ڵ�С��λ��
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
    /// BPM��ת����ʱ��Ϊ��׼
    /// </summary>
    /// <param name="BPMTable">����bpm��</param>
    /// <returns>ʱ��bpm��</returns>
    public static List<(float time, float bpm)> TransBpmTableToTimeBaseTable(List<(Beat beat, float bpm)> BPMTable)
    {
        List<(float time, float bpm)> tList = new List<(float time, float bpm)>();
        BPMTable.ForEach(i => tList.Add((TransBeatToTime(i.beat, BPMTable), i.bpm)));
        return tList;
    }

    /// <summary>
    /// ���׽���ת����ʱ��
    /// </summary>
    /// <param name="beat">����</param>
    /// <param name="BPM">bpm</param>
    /// <returns></returns>
    public static float TransSimpleBeatToTime(Beat beat, float BPM)
    {
        return 60000.0f * beat.Prase() / BPM;
    }

    /// <summary>
    /// ��ʽ��ʱ��
    /// </summary>
    /// <param name="time">ʱ��</param>
    /// <returns>��ʽ�����ʱ���ַ���</returns>
    public static string TimeFormat(float time)
    {
        return $"{(int)(time / 60):0}:{(int)time % 60:00}:{((time % 1) * 1000):000}";
    }

    /// <summary>
    /// ��ʽ��ʱ��
    /// </summary>
    /// <param name="time">�����ʱ��</param>
    /// <returns>��ʽ�����ʱ���ַ���</returns>
    public static string TimeFormat(int time)
    {
        var second = time / 1000;
        return $"{(second / 60):0}:{second % 60:00}:{(time % 1000):000}";
    }

    /// <summary>
    /// �Ӵ��ڵ�1080p��λ�����ţ�λ������ת����
    /// </summary>
    /// <param name="pos">λ��</param>
    /// <param name="startPoint">��ʼ�˵�</param>
    /// <param name="length">����</param>
    /// <param name="rawLength">ԭʼ����</param>
    /// <returns>ת����λ��</returns>
    public static float ScalePosToRawPos(float pos, float startPoint, float length, float rawLength)
    {
        return (pos - startPoint) / length * rawLength;
    }

    /// <summary>
    /// ��1080p�����ڵ�λ�����ţ�λ�÷���ת����
    /// </summary>
    /// <param name="pos">λ��</param>
    /// <param name="startPoint">��ʼ�˵�</param>
    /// <param name="length">����</param>
    /// <param name="rawLength">ԭʼ����</param>
    /// <returns>ת����λ��</returns>
    public static float ScaleRawPosToPos(float pos, float startPoint, float length, float rawLength)
    {
        return pos / rawLength * length + startPoint;
    }

    /// <summary>
    /// ֵ�Ƿ����е㷶Χ��
    /// </summary>
    /// <param name="value">���ֵ</param>
    /// <param name="center">�е�</param>
    /// <param name="radius">�뾶</param>
    /// <returns>�Ƿ����е㷶Χ��</returns>
    public static bool IsInCenterRange(float value, float center, float radius)
    {
        return IsInRange(value, center - radius, center + radius);
    }

    /// <summary>
    /// ֵ�Ƿ����е㷶Χ�ڣ����꣩
    /// </summary>
    /// <param name="value">���ֵ</param>
    /// <param name="center">�е�</param>
    /// <param name="radius">�뾶</param>
    /// <returns>�Ƿ����е㷶Χ��</returns>
    public static bool IsInCenterRange(Vector2 value, Vector2 center, float radius)
    {
        var distance = (value - center).magnitude;
        return distance <= radius;
    }

    /// <summary>
    /// ֵ�Ƿ��ھ������޵�һ��������
    /// </summary>
    /// <param name="value">ֵ</param>
    /// <param name="min">����</param>
    /// <param name="length">����</param>
    /// <returns></returns>
    public static bool IsInRadiusRange(float value, float min, float length)
    {
        return IsInRange(value, min, min + length);
    }

    /// <summary>
    /// ֵ�Ƿ��ھ������޵�һ��������
    /// </summary>
    /// <param name="value">ֵ</param>
    /// <param name="min">����</param>
    /// <param name="length">����</param>
    /// <returns></returns>
    public static bool IsInRadiusRange(int value, int min, int length)
    {
        return IsInRange(value, min, min + length);
    }

    /// <summary>
    /// ֵ�Ƿ���ָ����Χ��
    /// </summary>
    /// <param name="value">ֵ</param>
    /// <param name="min">����</param>
    /// <param name="max">����</param>
    /// <returns>�Ƿ���ָ����Χ��</returns>
    public static bool IsInRange(float value, float min, float max)
    {
        return value - min > 1e-3 && max - value >= 1e-3;
    }

    /// <summary>
    /// ֵ�Ƿ���ָ����Χ��
    /// </summary>
    /// <param name="value">ֵ</param>
    /// <param name="min">����</param>
    /// <param name="max">����</param>
    /// <returns>�Ƿ���ָ����Χ��</returns>
    public static bool IsInRange(int value, int min, int max)
    {
        return min <= value && value <= max;
    }

    /// <summary>
    /// ֵ�Ƿ���ָ����Χ�ڣ����涨���˵Ĵ�Сֵ��
    /// </summary>
    /// <param name="value"></param>
    /// <param name="range1"></param>
    /// <param name="range2"></param>
    /// <returns>����ָ����Χ��</returns>
    public static bool IsInArea(float value, float range1, float range2)
    {
        var res1 = value - range1 > 0;
        var res2 = value - range2 > 0;
        return res1 ^ res2;
    }

    /// <summary>
    /// ��0-1��ֵ���ŵ�ʵ�ʵ�����
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
    /// ��ֵ��ʵ�ʵ����䵥λ����0-1��
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
    /// ��ȡ����<paramref name="symmetryValue"/>��ˮƽ�Գ�����
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
    /// ������Ϸ���������굽��Ϸ�������꣨���½�Ϊԭ�㣩
    /// </summary>
    /// <param name="pos">ʵ�ʳ���������</param>
    /// <returns>��Ϸ��������</returns>
    public static Vector2 ScaleToGameWindowPosition(Vector2 pos)
    {
        return (pos + EditingData.RealGameZeroPos) / EditingData.GameSceneScale;
    }

    /// <summary>
    /// ������Ϸ�������굽��Ϸ����������
    /// </summary>
    /// <param name="pos">����������</param>
    /// <returns>ʵ����Ϸ����������</returns>
    public static Vector2 ScaleToGameScenePosition(Vector2 pos)
    {
        return pos * EditingData.GameSceneScale - EditingData.RealGameZeroPos;
    }

    /// <summary>
    /// ������Լ�����꣬��������Ჽ��Ϊ����
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
