using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Resux.LevelData
{
    ///<summary>判定类型</summary>
    public enum JudgeType
    {
        Tap,
        Hold,
        Flick
    }

    /// <summary>
    /// 判定信息对
    /// </summary>
    public class JudgePair
    {
        public Vector2 judgePosition;

        public int judgeTime;

        public JudgePair()
        {

        }

        public JudgePair(JudgePair pair)
        {
            judgePosition = pair.judgePosition;
            judgeTime = pair.judgeTime;
        }
    }

    /// <summary>
    /// note判定信息
    /// </summary>
    public class JudgeNoteInfo
    {
        ///<summary>判定类型</summary> 
        public JudgeType judgeType;

        /// <summary>对应的移动半note</summary>
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

        public JudgeNoteInfo(JudgeType judgeType, List<JudgePair> judges, NoteInfo halfNote1, NoteInfo halfNote2)
        {
            this.judgeType = judgeType;
            this.judges = judges ?? throw new ArgumentNullException(nameof(judges));
            this.halfNote1 = halfNote1;
            this.halfNote2 = halfNote2;
        }

        /// <summary>
        /// 获取最早的半note的开始时间
        /// </summary>
        /// <returns></returns>
        public int GetEarlyestHalfNoteTime()
        {
            return Mathf.Min(halfNote1.noteMoveInfo.startTime, halfNote2.noteMoveInfo.startTime);
        }
    }
}
