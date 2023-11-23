using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Resux.GamePlay;
using UnityEngine;

public static class NoteMoveForEdit
{
    public static GamePlaySetting GamePlaySetting;

    static NoteMoveForEdit()
    {
        GamePlaySetting = Resources.Load<GamePlaySetting>("GameConfig/GamePlaySetting");
    }

    public static Vector2[] CalculatePositionList(NoteInfo noteInfo, int startTime)
    {
        var moveInfo = noteInfo.noteMoveInfo;
        var posListInRange = CalculatePositionList(moveInfo, startTime, noteInfo.weightType);

        for (int i = 0; i < noteInfo.holdPath.Count; i++)
        {
            posListInRange.Add(noteInfo.holdPath[i]);
        }
        Vector2[] posList = new Vector2[posListInRange.Count];
        if (moveInfo.isReverse)
        {
            for (int i = posListInRange.Count - 1, j = 0, reverseTime = 0; i >= 0; i--, reverseTime += 10, j++)
            {
                var posPair = posListInRange[i];
                posList[j] = posPair;
            }
        }
        else
        {
            for (int i = 0; i < posListInRange.Count; i++)
            {
                var posPair = posListInRange[i];
                posList[i] = posPair;
            }
        }

        return posList;
    }
    private static List<Vector2> CalculatePositionList(NoteMoveInfo noteMoveInfo, int timeOffset, WeightType weightType)
    {
        var posList = new List<Vector2>();
        var pos = noteMoveInfo.p0;
        var v0 = noteMoveInfo.v0;
        posList.Add(noteMoveInfo.p0);
        int currentTime;
        for (currentTime = timeOffset + 10; currentTime <= noteMoveInfo.endTime; currentTime += 10)
        {
            var a = GamePlaySetting.Gravity;
            if (pos.y <= GamePlaySetting.WaterSurface)
            {
                a += GamePlaySetting.Bouyancy[(int)weightType];
                a += GetAcc(v0.y, weightType);
            }
            Vector2 offset = Vector2.zero;
            offset.x += v0.x * 0.01f;
            offset.y += v0.y * 0.01f + 0.5f * a * 0.01f * 0.01f;
            pos += offset;
            v0.y += a * 0.01f;
            posList.Add(pos);
        }

        return posList;
    }
    private static float GetAcc(float v, WeightType weight)
    {
        float a = 0;
        var dir = v > 0;
        var _v = Mathf.Abs(v);
        List<ResistanceSet> resisitance;
        switch (weight)
        {
            case WeightType.Light:
                resisitance = GamePlaySetting.LightResistance;
                break;
            case WeightType.Weight:
                resisitance = GamePlaySetting.WeightResistance;
                break;
            case WeightType.Middle:
            default:
                resisitance = GamePlaySetting.MiddleResistance;
                break;
        }
        for (int i = resisitance.Count - 1; i >= 0; i--)
        {
            ResistanceSet resist = resisitance[i];
            if (_v < resist.speed)
            {
                continue;
            }
            a = resist.GetResistance(_v);
        }
        return dir ? -a : a;
    }
}