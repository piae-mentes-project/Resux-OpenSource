using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Resux.UI;
using System.Threading.Tasks;
using Resux.GamePlay.Judge;
using Resux.LevelData;

namespace Resux.GamePlay
{
    /// <summary>
    /// Tap、Flick判定
    /// </summary>
    public class NormalJudgeNote : BaseJudgeNote
    {
        #region properties

        /// <summary>判定位置</summary>
        protected Vector2 judgePos;

        /// <summary>
        /// 分辨率缩放后的判定点位置
        /// </summary>
        public Vector2 scaledJudgePos;

        /// <summary>
        /// note的起始判定时间，单位ms
        /// </summary>
        public int judgeTime;

        /// <summary>判定结果</summary>
        protected JudgeResult judgeResult;
        protected PerfectType perfectType;

        protected bool isValidResult => JudgeResult.None != judgeResult && JudgeResult.Miss != judgeResult;

        #region extra properties

        public bool isInStartJudgeRange => JudgeMethods.IsInRange(maxJudgeRange, currentTime - judgeTime);

        /// <summary>是否是浮现式</summary>
        protected bool isPopupReverse;
        public bool IsPopupReverse => isPopupReverse;
        /// <summary>是否正在显示缩圈动画</summary>
        protected bool isShowingScaleAnimation;

        #endregion

        #endregion

        #region Updates
        
        protected override void Update(int time)
        {
            base.Update(time);
            // 浮现式，需要提前显示缩圈
            if (isPopupReverse)
            {
                if (!isShowingScaleAnimation && time >= judgeTime - 1000)
                {
                    ShowScaleEffect(judgePos);
                    isShowingScaleAnimation = true;
                }
            }

            if (time < maxJudgeRange.x + judgeTime)
            {
                return;
            }

            // 是否是自动
            if (MusicScoreSelection.isAutoPlay)
            {
                // 在判定范围内
                var isInMinRange = JudgeMethods.IsInRange(minJudgeRange, time - judgeTime);
                if (isInMinRange && time >= judgeTime)
                {
                    // 自动大P
                    judgeResult = JudgeResult.PERFECT;
                    perfectType = PerfectType.Just;
                    SendJudgeResult(judgeResult, perfectType);
                }
            }
            else
            {
                // 非自动下，存在判定结果(非hold)时发送判定成绩
                if (JudgeType != JudgeType.Hold && judgeResult != JudgeResult.None)
                {
                    SendJudgeResult(judgeResult, perfectType);
                }
                // 超出note出现后判定的持续时间
                else if (time - judgeTime > maxJudgeRange.y)
                {
                    // 结尾超出区间就是miss
                    SendMissResult();
                }
            }
        }

        #endregion

        /// <summary>
        /// 构造 初始化
        /// </summary>
        /// <param name="judgeNoteInfo">判定信息</param>
        /// <param name="isMulti">是否是多押</param>
        public NormalJudgeNote(JudgeNoteInfo judgeNoteInfo, bool isMulti)
        {
            JudgeType = judgeNoteInfo.judgeType;

            var judge = judgeNoteInfo.judges[0];
            this.judgeTime = judge.judgeTime;
            this.judgePos = ScreenAdaptation.AdaptationNotePosition(judge.judgePosition);
            judgeResult = JudgeResult.None;
            FirstJudgePos = scaledJudgePos = ScreenAdaptation.AdaptationJudgePosition(judgePos);

            if (judgeNoteInfo.halfNote1.noteMoveInfo.isReverse || judgeNoteInfo.halfNote2.noteMoveInfo.isReverse)
            {
                isPopupReverse = true;
                isShowingScaleAnimation = false;
            }

            FirstJudgeTime = judgeTime;
            base.Initialize(judgeNoteInfo, isMulti);
        }

        #region Public Method

        /// <summary>
        /// 触摸判定（Tap）
        /// </summary>
        /// <returns>判定是否成功</returns>
        public override bool Judge(TouchInputInfo touch)
        {
            if (IsJudged)
            {
                return false;
            }

            if (touch.State != TouchInputState.Static && JudgeType == JudgeType.Tap)
            {
                // 手不能动，动了就是Flick了（笑
                return false;
            }

            // 继续执行判定逻辑，已经有判定结果的话就不予理会了
            if (judgeResult != JudgeResult.None)
            {
                return false;
            }
            judgeResult = JudgeMethods.Judge(new JudgeParameter(judgeTime, currentTime, JudgeType, scaledJudgePos, touch), out perfectType);

            if (isValidResult)
            {
                touch.State = TouchInputState.Static;
            }
            return isValidResult;
        }

        #endregion

        #region Protected Method

        /// <summary>
        /// 发送判定结果
        /// </summary>
        /// <param name="result">判定结果</param>
        protected override void SendJudgeResult(JudgeResult result, PerfectType perfectType)
        {
            // 防止回溯的时候发生问题，不准重复发送判定信息！
            if (IsJudged)
            {
                return;
            }
            Logger.Log(result);
            base.SendJudgeResult(result, perfectType);
            IsJudged = true;

            void OnJudgeOver()
            {
                if (isValidResult)
                {
                    MusicPlayer.Instance.PlayEffect(JudgeType);
                    // 显示特效
                    ShowEffect(result, perfectType, judgePos);
                }

                // tap、flick在判定结束后释放
                IsReleased = true;
                upHalfNote.HideNoteByJudge();
                downHalfNote.HideNoteByJudge();
                // upHalfNote.HideNoteByJudge();
                // downHalfNote.HideNoteByJudge();
            }

            // 播放音效
            if (JudgeType == JudgeType.Flick)
            {
                // 需要≥0ms的时候发送成绩并播放音效特效
                CoroutineUtils.RunDelay(OnJudgeOver, (judgeTime - currentTime) / 1000.0f);
            }
            else
            {
                OnJudgeOver();
            }
        }

        #endregion

        #region Private Method

        private void SendMissResult()
        {
            IsReleased = true;
            judgeResult = JudgeResult.Miss;
            perfectType = PerfectType.None;
            SendJudgeResult(judgeResult, perfectType);
        }

        #endregion

        #region Coroutine



        #endregion
    }
}
