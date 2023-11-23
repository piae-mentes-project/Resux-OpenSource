using Resux.UI;
using System.Collections;
using Resux.GamePlay.Judge;
using Resux.LevelData;
using UnityEngine;

namespace Resux.GamePlay
{
    public static class JudgeMethods
    {
        #region Static Method

        /// <summary>
        /// 判定方法
        /// </summary>
        /// <param name="judgeParameter">判定参数</param>
        /// <returns>判定结果</returns>
        public static JudgeResult Judge(JudgeParameter judgeParameter, out PerfectType perfectType)
        {
            JudgeResult result = JudgeResult.None;
            perfectType = PerfectType.None;

            // 这里是距离判定
            var touchPos = judgeParameter.touch.Pos;
            // var touchPos = judgeParameter.touch.pos;
            if (!IsInJudgeDistance(judgeParameter.judgePos, touchPos))
            {
                // 表示没进判定圈

                /* begin for debugging */
                // if (judgeParameter.judgeType == JudgeType.Flick) Debugger.Log($"judgePos: {judgeParameter.judgePos}, touchPos: {touchPos} 触摸点不在判定范围内", Color.yellow);
                /* end for debugging */
                return JudgeResult.None;
            }
            Logger.Log($"touch pos: {judgeParameter.touch.Pos}, judge pos: {judgeParameter.judgePos}, Distance: {GameConfigs.Distance}, scale distance: {GameConfigs.Distance * PlayerGameSettings.Setting.NoteSize}");

            // 时间判定
            var judgeType = judgeParameter.judgeType;
            var judgeRanges = GameConfigs.JudgeSetting[judgeType];
            var offset = (judgeParameter.time - judgeParameter.judgeTime);
            // Flick划到就是大p            
            if (judgeType == JudgeType.Flick)
            {
                if (IsInRange(judgeRanges[JudgeResult.Bad], offset))
                {
                    perfectType = PerfectType.Just;
                    return JudgeResult.PERFECT;
                }

                return JudgeResult.None;
            }

            if (IsInRange(judgeRanges[JudgeResult.PERFECT], offset))
            {
                // 大P
                result = JudgeResult.PERFECT;
                perfectType = PerfectType.Just;
            }
            else if (IsInRange(judgeRanges[JudgeResult.Perfect], offset))
            {
                // 小p
                result = JudgeResult.Perfect;
                perfectType = offset > 0 ? PerfectType.Late : PerfectType.Early;
            }
            else if (IsInRange(judgeRanges[JudgeResult.Good], offset))
            {
                result = JudgeResult.Good;
            }
            else if (IsInRange(judgeRanges[JudgeResult.Bad], offset))
            {
                result = JudgeResult.Bad;
            }
            return result;
        }

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
            var judgeRanges = GameConfigs.JudgeSetting[judgeType];

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
            return Vector2.Distance(judgePos, touchPos) <= GameConfigs.Distance;// (GameConfigs.Distance * PlayerGameSettings.Setting.NoteSize);
        }

        #endregion
    }
}
