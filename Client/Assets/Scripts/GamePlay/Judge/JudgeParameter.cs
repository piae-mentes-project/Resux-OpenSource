using Resux.LevelData;
using UnityEngine;

namespace Resux
{
    /// <summary>
    /// 判定传递的参数
    /// </summary>
    public class JudgeParameter
    {
        /// <summary>
        /// 判定时间（0ms）
        /// </summary>
        public int judgeTime;
        /// <summary>
        /// 触发判定的时间
        /// </summary>
        public int time;
        /// <summary>
        /// 判定类型
        /// </summary>
        public JudgeType judgeType;
        /// <summary>
        /// note判定位置
        /// </summary>
        public Vector2 judgePos;
        /// <summary>
        /// 触发判定位置
        /// </summary>
        public TouchInputInfo touch;

        public JudgeParameter(int judgeTime, int time, JudgeType judgeType, Vector2 judgePos, TouchInputInfo touch)
        {
            this.judgeTime = judgeTime;
            this.time = time;
            this.judgeType = judgeType;
            this.judgePos = judgePos;
            this.touch = touch;
        }
    }
}