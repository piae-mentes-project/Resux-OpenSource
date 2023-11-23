using Resux.UI;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace Resux.GamePlay
{
    public static class JudgeMethods
    {
        /// <summary>
        /// 判定配置
        /// </summary>
        public static Dictionary<JudgeType, Dictionary<JudgeResult, Vector2>> JudgeSetting { get; }

        /// <summary>
        /// 判定距离
        /// </summary>
        public static float Distance { get; private set; }

        /// <summary>
        /// 稳定触摸点距离
        /// </summary>
        public static float StableDistance { get; private set; }

        /// <summary>
        /// hold换手保护时间
        /// </summary>
        public static int ChangeHandProtectTime { get; private set; }

        static JudgeMethods()
        {
            // 判定配置初始化
            var judgeSetting = Resources.Load<JudgeSetting>("GameConfig/JudgeSetting");
            JudgeSetting = new Dictionary<JudgeType, Dictionary<JudgeResult, Vector2>>();
            Distance = judgeSetting.Distance;
            StableDistance = judgeSetting.StableDistance;
            ChangeHandProtectTime = judgeSetting.ChangeHandProtectTime;
            if (!judgeSetting.useDefault)
            {
                foreach (var judgeRange in judgeSetting.JudgeRanges)
                {
                    var type = judgeRange.type;
                    JudgeSetting.Add(type, new Dictionary<JudgeResult, Vector2>());
                    foreach (var rangePair in judgeRange.ranges)
                    {
                        JudgeSetting[type].Add(rangePair.result, rangePair.range);
                    }
                }
            }
            else
            {
                foreach (JudgeType type in Enum.GetValues(typeof(JudgeType)))
                {
                    JudgeSetting.Add(type, new Dictionary<JudgeResult, Vector2>());
                    foreach (var rangePair in judgeSetting.DefaultRangePairs)
                    {
                        JudgeSetting[type].Add(rangePair.result, rangePair.range);
                    }
                }
            }
        }

        #region Static Method

        /// <summary>
        /// 触摸时间是否在指定的<paramref name="judgeResult"/>判定结果时间范围内
        /// </summary>
        /// <param name="touchTime">触摸时间</param>
        /// <param name="judgeTime">判定时间</param>
        /// <param name="judgeType">判定类型</param>
        /// <param name="judgeResult">判定结果</param>
        /// <returns>是否处于范围内</returns>
        public static bool IsInJudgeRange(int touchTime, int judgeTime, JudgeType judgeType, JudgeResult judgeResult)
        {
            var judgeRanges = JudgeSetting[judgeType];

            // 下面是时间判定（有早晚）
            var offset = touchTime - judgeTime;

            return IsInRange(judgeRanges[judgeResult], offset);
        }

        public static bool IsInRange(Vector2 range, float t)
        {
            return range.x <= t && range.y >= t;
        }

        public static bool IsInJudgeDistance(Vector2 judgePos, Vector2 touchPos)
        {
            return Vector2.Distance(judgePos, touchPos) <= Distance;// (GameConfigs.Distance * PlayerGameSettings.Setting.NoteSize);
        }

        #endregion
    }
}
