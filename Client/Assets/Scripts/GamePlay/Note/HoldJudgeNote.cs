using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Resux.UI;
using UnityEngine;
using System.Threading.Tasks;
using Resux.GamePlay.Judge;
using Resux.LevelData;

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
        protected Vector2 notePosForJudge => ScreenAdaptation.AdaptationJudgePosition(noteTransform.position);

        public int IdleStartTime = -1;
        protected bool isInJudge;

        /// <summary>已处于判定状态</summary>
        public bool IsInJudge => isInJudge;

        /// <summary>当前hold note的触摸</summary>
        protected TouchInputInfo touch;
        
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
                var judgePosAfterNotePosAdaptation = ScreenAdaptation.AdaptationNotePosition(judge.judgePosition);
                tempJudge.judgePosition = ScreenAdaptation.AdaptationJudgePosition(judgePosAfterNotePosAdaptation);
                var rawPosition = judgePosAfterNotePosAdaptation;

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
                var isShow = toJudgeTime <= 900 && toJudgeTime >= 0 && judge.judgeResult == JudgeResult.None;
                if (isShow)
                {
                    if (judge.judgePoint == null)
                    {
                        judge.judgePoint = holdJudgePointPool.GetObject();
                        judge.judgePoint.transform.position = judge.rawPos;
                    }
                }

                judge.judgePoint?.SetActive(isShow);
            }

            if (IsJudged)
            {
                return;
            }

            var currentJudge = judges[currentJudgeIndex];

            if (isInJudge)
            {
                // 手已离开 或 当前触摸点不在判定范围内 0.08s的换手期间
                if (touch.Leaved || !JudgeMethods.IsInJudgeDistance(notePosForJudge, touch.Pos))
                {
                    Logger.Log($"touch leave or far away from Hold Path: notePos: {noteTransform.position}, touchPos: {touch?.Pos}");
                    ProtectForNewTouch();
                }

                if (holdEffect)
                {
                    // 每次都set，以防回溯的时候的异常
                    holdEffect.gameObject.SetActive(lockedCurrentTime >= FirstJudgeTime);
                    holdEffect.transform.position = noteTransform.position;
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

                // 换手期间，遇到判定点仍然会断
                var timeOffset = lockedCurrentTime - currentJudge.judgePair.judgeTime;
                if (IdleStartTime != -1)
                {
                     if (timeOffset > maxJudgeRange.y)
                    {
                        SendMissResult();
                    }
                }
                else if (JudgeMethods.IsInRange(maxJudgeRange, timeOffset))
                {
                    SendJudgeResult(JudgeResult.PERFECT, PerfectType.Just);
                }
            }

            if (IdleStartTime != -1 && lockedCurrentTime - IdleStartTime > GameConfigs.ChangeHandProtectTime)
            {
                // 手离开0.2s且没有继接，全miss
                SendMissResult();
                Logger.Log("换手失败");
            }
            // 超出最大判定区间 miss，上下条件只需要一个就可以了
            else if (lockedCurrentTime - currentJudge.judgePair.judgeTime > maxJudgeRange.y)
            {
                SendMissResult();
            }
        }

        #endregion

        #region Public Method

        /// <summary>
        /// hold的触摸判定，开始判定，只会被调用一次
        /// </summary>
        /// <returns>判定是否成功</returns>
        public override bool Judge(TouchInputInfo touch)
        {
            if (IsJudged || touch == null)
            {
                return false;
            }

            if (touch.State != TouchInputState.Static)
            {
                // 开始的时候手不能动（卡那一瞬间）
                return false;
            }

            var judge = judges[currentJudgeIndex];
            if (IsInJudge)
            {
                Logger.Log($"note pos: {notePosForJudge}, touch pos: {touch.Pos}", Color.yellow);
                // 接续判定 or 换手
                if (JudgeMethods.IsInJudgeDistance(notePosForJudge, touch.Pos))
                {
                    if (touch != this.touch)
                    {
                        this.touch = touch;
                        Logger.Log("换手成功");
                    }
                    this.IdleStartTime = -1;
                    return true;
                }
                else
                {
                    // 只有当前触摸点没能判上的时候才开启换手保护
                    if (touch == this.touch)
                    {
                        ProtectForNewTouch();
                    }
                }
            }
            else
            {
                // 初次判定
                var judgeResult = JudgeMethods.Judge(new JudgeParameter(judge.judgePair.judgeTime, currentTime, JudgeType, judge.judgePair.judgePosition, touch), out var perfectType);
                if (judgeResult != JudgeResult.None)
                {
                    this.touch = touch;
                    isInJudge = true;
                    SendJudgeResult(judgeResult, perfectType);
                    // 开始显示Hold特效
                    if (holdEffect == null && judges.Length > 1)
                    {
                        holdEffect = holdEffectPool.GetObject();
                        holdEffect.transform.position = noteTransform.position;
                        holdEffect.gameObject.SetActive(true);
                        // upHalfNote.ScaleLarge();
                        // downHalfNote.ScaleLarge();
                    }

                    return true;
                }
            }

            return false;
        }

        private void ProtectForNewTouch()
        {
            if (IdleStartTime == -1)
            {
                IdleStartTime = currentTime;
                Logger.Log("开始换手");
            }
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
                CoroutineUtils.RunDelay(
                    () =>
                    {
                        // 播放音效
                        MusicPlayer.Instance.PlayEffect(JudgeType);
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
                    (judge.judgePair.judgeTime - currentTime) / 1000.0f);
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
            Logger.Log("JudgeEnd");
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