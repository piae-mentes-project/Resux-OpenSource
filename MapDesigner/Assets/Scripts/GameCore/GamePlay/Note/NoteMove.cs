using System.Collections;
using System;
using System.Collections.Generic;
using Resux.UI;
using UnityEngine;

namespace Resux.GamePlay
{
    /// <summary>
    /// note移动类
    /// </summary>
    public static class NoteMove
    {
        #region properties

        public static GamePlaySetting GamePlaySetting => NoteMoveForEdit.GamePlaySetting;

        private static int DeltaTime;
        private static float MillionSeconds;
        private static float DeltaSecond;
        private static List<int> additiveTimeOffset;

        #endregion

        static NoteMove()
        {
            DeltaTime = 10;
            MillionSeconds = 1000;
            DeltaSecond = DeltaTime / MillionSeconds;
            additiveTimeOffset = new List<int>(3);
            additiveTimeOffset.Add((int)(JudgeMethods.JudgeSetting[JudgeType.Tap][JudgeResult.Bad].y + 0.5f));
            additiveTimeOffset.Add((int)(JudgeMethods.JudgeSetting[JudgeType.Hold][JudgeResult.Bad].y + 0.5f));
            additiveTimeOffset.Add((int)(JudgeMethods.JudgeSetting[JudgeType.Flick][JudgeResult.Bad].y + 0.5f));
        }

        #region Public Method

        /// <summary>
        /// 计算note的全程的运动轨迹
        /// </summary>
        /// <param name="noteInfo">note信息</param>
        /// <param name="startTime">开始时间</param>
        /// <returns>运动轨迹(位置列表)</returns>
        public static Vector2[] CalculatePositionList(NoteInfo noteInfo, int startTime, JudgeType judgeType, out int lengthWithoutHold)
        {
            var posList = new List<Vector2>();
            // 运动区间
            var moveInfo = noteInfo.noteMoveInfo;

            // 对每个区间做计算
            var posListInRange = CalculatePositionList(moveInfo, startTime, noteInfo.weightType, judgeType);

            lengthWithoutHold = posListInRange.Count - additiveTimeOffset[(int)judgeType] / 10;

            for (int i = 0; i < noteInfo.holdPath.Count; i++)
            {
                // noteInfo.holdPath[i] = ScreenAdaptation.AdaptationNotePosition(noteInfo.holdPath[i]);
                posListInRange.Add(noteInfo.holdPath[i]);
            }

            // 是否反向运动
            if (moveInfo.isReverse)
            {
                posListInRange.Reverse();
                // Array.Reverse(posListInRange);
            }
            for (int i = 0; i < posListInRange.Count; i++)
            {
                var posPair = posListInRange[i];
                posList.Add(posPair);
            }

            return posList.ToArray();
        }


        #endregion

        #region Private Method

        /// <summary>
        /// 计算一个区间的运动轨迹（结果为正向）
        /// </summary>
        /// <param name="noteMoveInfo">区间移动信息</param>
        /// <param name="timeOffset">时间偏移</param>
        /// <param name="weightType">重量类型</param>
        /// <returns>运动轨迹列表 <c>int时间</c>与<c>Vector2位置</c>的成对的列表</returns>
        private static List<Vector2> CalculatePositionList(NoteMoveInfo noteMoveInfo, int timeOffset, WeightType weightType, JudgeType judgeType)
        {
            var posList = new List<Vector2>();
            // 计算每帧的位置
            var pos = noteMoveInfo.p0;
            // 初速度
            var v0 = noteMoveInfo.v0;
            // 配置信息
            var setting = GamePlaySetting;

            var waterSurface = setting.WaterSurface;
            // 每10ms计算一次(FixedUpdate)
            int currentTime;
            int endTime = noteMoveInfo.endTime;
            // 非Hold的话再加上一段运动，直到bad区间过去
            if (judgeType != JudgeType.Hold)
            {
                endTime += additiveTimeOffset[(int)judgeType];
            }
            // 添加起始位置
            posList.Add(noteMoveInfo.p0);
            for (currentTime = timeOffset + DeltaTime; currentTime <= endTime; currentTime += DeltaTime)
            {
                // 加速度
                var a = setting.Gravity;
                // 这样,主要的任务就变成了每10ms区间内受到的力的影响
                if (pos.y <= waterSurface)
                {
                    // 入水,要受到水中的力
                    a += setting.Bouyancy[(int)weightType];
                    a += GetAcc(v0.y, weightType);
                }

                // 具体位置计算
                Vector2 offset = Vector2.zero;
                // m/s作为速度单位
                offset.x += v0.x * DeltaSecond;
                // y轴分段加速，在一个区段（10ms）内的加速度保持不变，作为类变加速处理
                // v0t + 1/2 at²，t的单位是s，距离单位是像素（或者想象成m）
                offset.y += v0.y * DeltaSecond + 0.5f * a * DeltaSecond * DeltaSecond;

                pos += offset;
                // 更新为下一帧的初速度
                v0.y += a * DeltaSecond;
                // Debugger.Log($"v0: {v0}, pos: {pos}, a: {a}");
                // var resPos = ScreenAdaptation.AdaptationNotePosition(pos);
                //赋值
                posList.Add(pos);
                // posList.Add(resPos);
            }

            return posList;
        }

        /// <summary>
        /// 获取速度对应的阻力加速度
        /// </summary>
        /// <param name="v">速度(正方向为上)</param>
        /// <returns>阻力加速度</returns>
        private static float GetAcc(float v, WeightType weight)
        {
            float a = 0;
            // 速度方向，大于0是向上
            var dir = v > 0;
            var _v = Mathf.Abs(v);

            List<ResistanceSet> resistance;

            switch (weight)
            {
                case WeightType.Light:
                    resistance = GamePlaySetting.LightResistance;
                    break;
                case WeightType.Weight:
                    resistance = GamePlaySetting.WeightResistance;
                    break;
                case WeightType.Middle:
                default:
                    resistance = GamePlaySetting.MiddleResistance;
                    break;
            }

            // 计算加速度
            // 从大到小的计算
            for (int i = resistance.Count - 1; i >= 0; i--)
            {
                ResistanceSet resist = resistance[i];

                if (_v < resist.speed)
                {
                    continue;
                }
                a = resist.GetResistance(_v);
            }

            return dir ? -a : a;
        }

        #endregion
    }
}