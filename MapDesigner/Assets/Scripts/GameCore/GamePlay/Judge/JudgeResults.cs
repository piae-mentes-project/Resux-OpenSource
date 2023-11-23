using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Resux.GamePlay
{

    /// <summary>判定结果</summary>
    public enum JudgeResult
    {
        /// <summary>啥也没</summary>
        None = 0,
        /// <summary>你有一个小姐</summary>
        Miss = 1,
        /// <summary>你坏~</summary>
        Bad = 1 << 1,
        /// <summary>你有一个好</summary>
        Good = 1 << 2,
        /// <summary>小p</summary>
        Perfect = 1 << 3,
        /// <summary>大P</summary>
        PERFECT = 1 << 4
    }

    /// <summary>Perfect指示</summary>
    public enum PerfectType
    {
        /// <summary>非P</summary>
        None,
        Early,
        /// <summary>大P</summary>
        Just,
        Late
    }

    /// <summary>
    /// 判定结果数据
    /// </summary>
    public class JudgeResultData
    {
        public Vector2 rawPos;
        public JudgePair judgePair;
        public GameObject judgePoint;
        public JudgeResult judgeResult;
        public PerfectType perfectType;
    }

}
