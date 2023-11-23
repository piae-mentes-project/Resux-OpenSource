using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Resux.GamePlay
{
    public enum NoteTimeState
    {
        None,
        Enter,
        Keep,
        Leave,
        End
    }

    public class BaseJudgeNote : IJudgeNote
    {
        #region static

        public static ObjectPool<UIFrameAnimation> effectPool;
        public static ObjectPool<UIFrameAnimation> holdEffectPool;
        public static ObjectPool<GameObject> holdJudgePointPool;

        #endregion

        #region properties

        ///<summary>判定类型</summary> 
        public JudgeType JudgeType { get; protected set; }

        /// <summary>
        /// 当前时间（用于进入判定区间内的判断）
        /// </summary>
        protected int currentTime => GamePlayer.Instance.timeWithOffset;

        /// <summary>判定区间（大）</summary>
        protected Vector2 maxJudgeRange;
        protected Vector2 minJudgeRange;

        protected HalfNote upHalfNote;
        protected HalfNote downHalfNote;

        protected NoteTimeState CurrentTimeState = NoteTimeState.None;

        /// <summary>
        /// 首次判定时间，Hold为首次判定时间，其他直接判定时间即可
        /// </summary>
        public int FirstJudgeTime { get; protected set; } = 0;

        public Vector2 FirstJudgePos { get; protected set; }

        /// <summary>
        /// 已完成判定
        /// </summary>
        public bool IsJudged { get; protected set; } = false;

        /// <summary>
        /// 如果被设置为true，则不会再执行Update（注意是子类中override的Update而不是OnUpdate）
        /// </summary>
        public bool IsReleased { get; protected set; } = false;

        public int FirstHalfNoteStartTime { get; protected set; }


        #endregion

        #region Public Method

        /// <summary>
        /// 主要是给hold半note用的
        /// </summary>
        /// <param name="time"></param>
        public virtual void FixedUpdate(int time)
        {
            upHalfNote.FixedUpdate(time);
            downHalfNote.FixedUpdate(time);
        }

        public NoteTimeState OnUpdate(int overwriteCurrentTime = -1)
        {
            var lockedCurrentTime = overwriteCurrentTime == -1 ? currentTime : overwriteCurrentTime; // 读取一次currentTime并保存，下面使用这个值进行操作，减小getter调用开销

            if (JudgeType != JudgeType.Flick)
            {
                var rangeTime = lockedCurrentTime - FirstJudgeTime;
                if (rangeTime >= maxJudgeRange.x && CurrentTimeState < NoteTimeState.Keep)
                {
                    if (CurrentTimeState == NoteTimeState.Enter)
                    {
                        CurrentTimeState = NoteTimeState.Keep;
                    }
                    else if (CurrentTimeState == NoteTimeState.None)
                    {
                        CurrentTimeState = NoteTimeState.Enter;
                    }
                }
                else if (rangeTime >= maxJudgeRange.y && CurrentTimeState < NoteTimeState.Leave)
                {
                    CurrentTimeState = NoteTimeState.Leave;
                }
                else if (CurrentTimeState == NoteTimeState.Leave)
                {
                    CurrentTimeState = NoteTimeState.End;
                }
            }

            if (!IsReleased)
            {
                Update(lockedCurrentTime);
            }

            return CurrentTimeState;
        }

        public void OnDestroy()
        {
            Destroy();
        }

        public virtual void ResetJudge(int time)
        {
            if (time < FirstJudgeTime + minJudgeRange.x)
            {
                IsJudged = IsReleased = false;
            }

            upHalfNote.Reset(time);
            downHalfNote.Reset(time);
        }

        #endregion

        #region Protected Method

        protected void Initialize(JudgeNoteInfo judgeNoteInfo, bool isMulti)
        {
            // bad的判定范围就是判定区间
            maxJudgeRange = JudgeMethods.JudgeSetting[JudgeType][JudgeResult.Bad];
            minJudgeRange = JudgeMethods.JudgeSetting[JudgeType][JudgeResult.PERFECT];

            // 半note的hold跟踪只需要一个，所以只用上半就行
            upHalfNote = new HalfNote(judgeNoteInfo.halfNote1, judgeNoteInfo.judgeType, isMulti, true);
            downHalfNote = new HalfNote(judgeNoteInfo.halfNote2, judgeNoteInfo.judgeType, isMulti, false);

            FirstHalfNoteStartTime = judgeNoteInfo.GetEarlyestHalfNoteTime();
        }

        protected virtual void SendJudgeResult(JudgeResult judgeResult, PerfectType perfectType)
        {
            // 记录成绩
            MusicScoreRecorder.Instance.AddRecord(judgeResult, perfectType);
        }

        protected virtual void ShowEffect(JudgeResult result, PerfectType perfectType, Vector2 position)
        {
            GameObject effect = GamePlayer.Instance.GetEffect(result);

            if (!effect)
            {
                return;
            }
            effect.transform.localPosition = position;
            effect.SetActive(true);
        }

        /// <summary>
        /// 缩圈特效显示
        /// </summary>
        protected virtual void ShowScaleEffect(Vector2 pos)
        {
            var scaleAnimation = effectPool.GetObject();
            scaleAnimation.transform.position = pos;
            scaleAnimation.gameObject.SetActive(true);
        }

        protected virtual void Update(int time)
        {
            upHalfNote.Update(time);
            downHalfNote.Update(time);
        }

        protected virtual void Destroy()
        {

        }

        #endregion
    }
}
