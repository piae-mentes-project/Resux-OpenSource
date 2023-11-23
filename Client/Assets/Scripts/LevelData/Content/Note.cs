using System;
using System.Collections.Generic;
using UnityEngine;

namespace Resux
{
    ///<summary>重量类型</summary> 
    public enum WeightType
    {
        Light,
        Middle,
        Weight
    }

    /// <summary>
    /// note的外观（上下半note，仅对外观有影响）
    /// </summary>
    public enum HalfType
    {
        /// <summary>上半note</summary>
        UpHalf,
        /// <summary>下半note</summary>
        DownHalf
    }

    /// <summary>
    /// note的基本信息
    /// </summary>
    public class NoteInfo
    {
        ///<summary>重量类型</summary> 
        public WeightType weightType;

        ///<summary>外观类型</summary> 
        public HalfType halfType;

        /// <summary>半note移动区间</summary>
        public NoteMoveInfo noteMoveInfo;

        /// <summary>hold的运动路径，可空</summary>
        public List<Vector2> holdPath;

        public NoteInfo()
        {
            weightType = default;
            halfType = default;
            noteMoveInfo = default;
            holdPath = new List<Vector2>();
        }

        public NoteInfo(WeightType weightType, HalfType halfType, NoteMoveInfo noteMoveInfo, List<Vector2> holdPath)
        {
            this.weightType = weightType;
            this.halfType = halfType;
            this.noteMoveInfo = noteMoveInfo ?? throw new ArgumentNullException(nameof(noteMoveInfo));
            this.holdPath = holdPath ?? new List<Vector2>();
        }
    }

    /// <summary>
    /// 半note的移动区间
    /// </summary>
    public class NoteMoveInfo
    {
        /// <summary>初速度</summary>
        public Vector2 v0;

        /// <summary>初始位置</summary>
        public Vector2 p0;

        /// <summary>开始时间</summary>
        public int startTime;

        /// <summary>结束时间</summary>
        public int endTime;

        /// <summary>是否反向</summary>
        public bool isReverse;

        public NoteMoveInfo()
        {
            v0 = default;
            p0 = default;
            startTime = default;
            endTime = default;
            isReverse = default;
        }

        public NoteMoveInfo(Vector2 v0, Vector2 p0, int startTime, int endTime, bool isReverse)
        {
            this.v0 = v0;
            this.p0 = p0;
            this.startTime = startTime;
            this.endTime = endTime;
            this.isReverse = isReverse;
        }
    }
}