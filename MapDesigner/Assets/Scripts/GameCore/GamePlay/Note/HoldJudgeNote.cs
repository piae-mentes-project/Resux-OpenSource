using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Resux.UI;
using UnityEngine;
using System.Threading.Tasks;

namespace Resux.GamePlay
{
    /// <summary>
    /// Hold判定
    /// </summary>
    public class HoldJudgeNote : BaseJudgeNote
    {
        #region properties

        protected JudgeNoteInfo info;

        ///<summary>optimized - 判定信息列表 Array </summary> 
        public JudgeResultData[] judges = null;

        #region extra properties

        protected Transform noteTransform => upHalfNote.note.transform;
        private UIFrameAnimation holdEffect;

        protected int currentJudgeIndex;
        /// <summary>准确的判定索引</summary>
        protected int currentExactJudgeIndex;
        protected int LastJudgeTime;
        // protected Vector2 notePosForJudge => ScreenAdaptation.AdaptationJudgePosition(noteTransform.position);
        protected Vector2 notePosForJudge => noteTransform.position;
        /// <summary>提前展示hold内容的ms时间</summary>
        protected const int preShowHoldTime = 900;

        protected bool isInJudge;

        /// <summary>已处于判定状态</summary>
        public bool IsInJudge => isInJudge;
        
        #endregion

        #endregion

        public HoldJudgeNote(JudgeNoteInfo info, bool isMulti)
        {
            this.info = info;
            this.JudgeType = info.judgeType;
            var judgesList = new List<JudgeResultData>(info.judges.Count);
            foreach (var judge in info.judges)
            {
                var tempJudge = new JudgePair(judge);
                tempJudge.judgePosition = judge.judgePosition;
                var rawPosition = judge.judgePosition;
                // var judgePosAfterNotePosAdaptation = ScreenAdaptation.AdaptationNotePosition(judge.judgePosition);
                // tempJudge.judgePosition = ScreenAdaptation.AdaptationJudgePosition(judgePosAfterNotePosAdaptation);
                // var rawPosition = judgePosAfterNotePosAdaptation;

                judgesList.Add(new JudgeResultData()
                {
                    judgePair = tempJudge,
                    judgeResult = JudgeResult.None,
                    judgePoint = null,
                    rawPos = rawPosition
                });
            }

            currentJudgeIndex = 0;
            currentExactJudgeIndex = 0;
            judges = judgesList.ToArray();
            var firstJudge = judges[0].judgePair;
            FirstJudgeTime = firstJudge.judgeTime;
            FirstJudgePos = firstJudge.judgePosition;
            var lastJudge = judges.Last().judgePair;
            LastJudgeTime = lastJudge.judgeTime;
            base.Initialize(info, isMulti);
            
            // hold在路径走完后释放
            void OnHalfNoteMoveEnd()
            {
                IsReleased = true;
            }

            upHalfNote.OnMoveEnd += OnHalfNoteMoveEnd;
            downHalfNote.OnMoveEnd += OnHalfNoteMoveEnd;
        }

        #region Updates
        
        protected override void Update(int lockedCurrentTime)
        {
            base.Update(lockedCurrentTime);
            var lengthOfJudgesArray = judges.Length;
            for (int i = 0; i < lengthOfJudgesArray; i++)
            {
                // hold判定点展示时间控制
                var judge = judges[i];
                var toJudgeTime = judge.judgePair.judgeTime - lockedCurrentTime;
                var isShow = toJudgeTime <= preShowHoldTime && toJudgeTime >= 0 && judge.judgeResult == JudgeResult.None;
                if (isShow)
                {
                    if (judge.judgePoint == null)
                    {
                        judge.judgePoint = holdJudgePointPool.GetObject();
                        judge.judgePoint.transform.localPosition = judge.rawPos;
                    }
                }

                judge.judgePoint?.SetActive(isShow);
            }

            if (IsJudged)
            {
                return;
            }

            var currentJudge = judges[currentJudgeIndex];
            var timeOffset = lockedCurrentTime - currentJudge.judgePair.judgeTime;

            // 自动
            if (GlobalSettings.IsPreviewAutoPlayOn)
            {
                if (JudgeMethods.IsInRange(maxJudgeRange, timeOffset) && lockedCurrentTime >= currentJudge.judgePair.judgeTime)
                {
                    SendJudgeResult(JudgeResult.PERFECT, PerfectType.Just);
                }
            }

            if (isInJudge)
            {
                if (holdEffect)
                {
                    // 每次都set，以防回溯的时候的异常
                    holdEffect.gameObject.SetActive(lockedCurrentTime >= FirstJudgeTime);
                    holdEffect.transform.localPosition = noteTransform.position;
                    if (currentExactJudgeIndex > 0)
                    {
                        var prevJudgeTime = judges[currentExactJudgeIndex - 1].judgePair.judgeTime;
                        var offset = Mathf.Abs(lockedCurrentTime - prevJudgeTime);
                        // Debugger.Log($"current judge index: {currentExactJudgeIndex}, current time: {lockedCurrentTime}, prev judgeTime: {prevJudgeTime}", Color.green);
                        var scale = Mathf.Clamp01((float)offset / 100);
                        upHalfNote.MultiScale(scale);
                        downHalfNote.MultiScale(scale);
                    }
                }
            }

            // 超出最大判定区间 miss
            if (timeOffset > maxJudgeRange.y)
            {
                SendMissResult();
            }
        }

        #endregion

        #region Public Method

        public override void ResetJudge(int time)
        {
            if (time < FirstJudgeTime + minJudgeRange.x)
            {
                IsJudged = IsReleased = false;
                isInJudge = false;
            }
            else if (time >= FirstJudgeTime + minJudgeRange.x && time < LastJudgeTime + minJudgeRange.x)
            {
                IsJudged = IsReleased = false;
                isInJudge = true;
            }
            else
            {
                IsJudged = IsReleased = true;
                isInJudge = false;
            }

            for (int i = 0; i < judges.Length; i++)
            {
                var judgeData = judges[i];
                var judgePair = judgeData.judgePair;
                if (time - judgePair.judgeTime > maxJudgeRange.x)
                {
                    judgeData.judgeResult = JudgeResult.PERFECT;

                    if (time > judgePair.judgeTime)
                    {
                        continue;
                    }
                    else
                    {
                        currentExactJudgeIndex = i;
                    }

                    continue;
                }
                else
                {
                    judgeData.judgeResult = JudgeResult.None;
                    currentJudgeIndex = i;
                }
            }

            upHalfNote.Reset(time);
            downHalfNote.Reset(time);
        }

        #endregion

        #region Protected Method

        protected override void SendJudgeResult(JudgeResult result, PerfectType perfectType)
        {
            base.SendJudgeResult(result, perfectType);
            var judge = judges[currentJudgeIndex];
            // 有效成绩
            if (JudgeResult.None != result && JudgeResult.Miss != result)
            {
                CoroutineUtils.StartCoroutine(CoroutineUtils.DelayExecute(
                    () =>
                    {
                        // 播放音效
                        MusicPlayManager.Instance.PlayEffect(JudgeType);
                        // 显示特效
                        ShowEffect(result, perfectType, judge.rawPos);
                        // 回收判定点
                        if (judge.judgePoint != null)
                        {
                            holdJudgePointPool.ReturnToPool(judge.judgePoint);
                            judge.judgePoint = null;
                        }
                        currentExactJudgeIndex++;
                        if (currentExactJudgeIndex >= judges.Length)
                        {
                            upHalfNote.HideNoteByJudge();
                            downHalfNote.HideNoteByJudge();
                        }
                    },
                    (judge.judgePair.judgeTime - currentTime) / 1000.0f));
            }

            judge.judgeResult = result;
            judge.perfectType = perfectType;
            currentJudgeIndex++;
            if (currentJudgeIndex >= judges.Length)
            {
                JudgeEnd();
            }
        }

        protected override void Destroy()
        {
            upHalfNote.HideNoteByJudge();
            downHalfNote.HideNoteByJudge();

            foreach (var judge in judges)
            {
                if (judge.judgePoint != null)
                {
                    holdJudgePointPool.ReturnToPool(judge.judgePoint);
                    judge.judgePoint = null;
                }
            }
        }

        #endregion

        #region Private Method

        /// <summary>
        /// 发送剩余所有的判定结果，默认全miss
        /// </summary>
        private void SendMissResult()
        {
            if (IsJudged)
            {
                return;
            }

            upHalfNote.ScaleSmall();
            downHalfNote.ScaleSmall();
            // upNote.MultiScale(-1);
            // downNote.MultiScale(-1);

            for (; currentJudgeIndex < judges.Length;)
            {
                SendJudgeResult(JudgeResult.Miss, PerfectType.None);
            }
        }

        private void JudgeEnd()
        {
            isInJudge = false;
            IsJudged = true;
            // 回收特效
            holdEffect?.ForceLoopOver();
            holdEffect = null;
            // upHalfNote.HideNoteByJudge();
            // downHalfNote.HideNoteByJudge();
        }

        #endregion
    }
}