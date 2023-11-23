using System.Collections;
using System.Collections.Generic;
using Resux.LevelData;
using UnityEngine;

namespace Resux.GamePlay
{
    public interface IJudgeNote
    {
        #region

        ///<summary>判定类型</summary> 
        JudgeType JudgeType { get; }

        /// <summary>
        /// 首次判定时间，Hold为首次判定时间，其他直接判定时间即可
        /// </summary>
        int FirstJudgeTime { get; }

        Vector2 FirstJudgePos { get; }

        /// <summary>
        /// 最开始出现的半note的开始时间
        /// </summary>
        int FirstHalfNoteStartTime { get; }

        /// <summary>
        /// 已完成判定
        /// </summary>
        bool IsJudged { get; }

        /// <summary>
        /// 如果被设置为true，则不会再执行Update（注意是子类中override的Update而不是OnUpdate）
        /// </summary>
        bool IsReleased { get; }

        #endregion

        #region Method

        bool Judge(TouchInputInfo touch);

        void FixedUpdate(int time);

        NoteTimeState OnUpdate(int overwriteCurrentTime = -1);

        void OnDestroy();

        #endregion
    }
}
