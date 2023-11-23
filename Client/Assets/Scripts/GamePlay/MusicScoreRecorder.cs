using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Resux.GamePlay.Judge;

namespace Resux.GamePlay
{
    /// <summary>
    /// 记录单曲成绩的类
    /// </summary>
    public class MusicScoreRecorder
    {
        #region properites

        /// <summary>不断连所需要的判定类型(int按位计算)</summary>
        private int comboJudge;
        private bool isBanned;
        /// <summary>是否作弊</summary>
        public bool IsBanned => isBanned;

        /// <summary>成绩变动事件，分别为combo、总分和fc、ap状态</summary>
        private Action<int, int, bool, bool> onScoreChanged;

        #region 记录

        /// <summary>判定记录</summary>
        private Dictionary<JudgeResult, int> record;
        private int earlyPerfectCount;
        private int latePerfectCount;

        private int maxCombo;

        /// <summary>最大连击</summary>
        public int MaxCombo
        {
            get => maxCombo;
        }

        private int combo;

        /// <summary>当前连击</summary>
        public int Combo
        {
            get => combo;
        }

        /// <summary>当前已判定的note数量</summary>
        private int currentJudgedNoteCount;

        /// <summary>总note数</summary>
        public int TotalNoteCounts;

        /// <summary>大P</summary>
        public int PERFECT
        {
            get => record[JudgeResult.PERFECT];
        }

        /// <summary>小p</summary>
        public int Perfect
        {
            get => record[JudgeResult.Perfect];
        }

        public int Good
        {
            get => record[JudgeResult.Good];
        }

        public int Bad
        {
            get => record[JudgeResult.Bad];
        }

        public int Miss
        {
            get => record[JudgeResult.Miss];
        }

        public bool isFC { get; private set; }
        public bool isAP { get; private set; }

        #endregion

        #region 每首歌都要计算的得分

        /// <summary>
        /// 每个note拿满（100%）的分
        /// </summary>
        private int scorePerNote;

        private int totalScore;

        /// <summary>需要补偿的note次数</summary>
        private int additiveScoreCount;

        /// <summary>
        /// 总分
        /// </summary>
        public int TotalScore => totalScore;

        #endregion

        #endregion

        #region Singleton

        private static MusicScoreRecorder instance;

        public static MusicScoreRecorder Instance
        {
            get { return instance ?? (instance = new MusicScoreRecorder()); }
        }

        private MusicScoreRecorder()
        {
            record = new Dictionary<JudgeResult, int>();
            comboJudge = (int)JudgeResult.PERFECT | (int)JudgeResult.Perfect | (int)JudgeResult.Good;
            Reset();
        }

        #endregion

        #region Private Method



        #endregion

        #region Public Method

        /// <summary>
        /// 重置
        /// </summary>
        public void Reset()
        {
            // 初始化所有判定结果的数量
            record.Clear();
            foreach (var value in Enum.GetValues(typeof(JudgeResult)))
            {
                record.Add((JudgeResult)value, 0);
            }

            TotalNoteCounts = 0;
            // 重置分数
            scorePerNote = 0;
            totalScore = 0;
            additiveScoreCount = 0;
            // 重置combo
            maxCombo = combo = 0;
            currentJudgedNoteCount = 0;
            isFC = isAP = true;
            earlyPerfectCount = latePerfectCount = 0;

            // 重置所有监听
            if (onScoreChanged != null)
            {
                RemoveListeners();
            }
        }

        /// <summary>
        /// 添加判定记录
        /// </summary>
        /// <param name="result">判定结果</param>
        public void AddRecord(JudgeResult result, PerfectType perfectType)
        {
            currentJudgedNoteCount++;
            record[result]++;
            switch (perfectType)
            {
                case PerfectType.Early:
                    earlyPerfectCount++;
                    isAP = isAP && true;
                    break;
                case PerfectType.Late:
                    latePerfectCount++;
                    isAP = isAP && true;
                    break;
                case PerfectType.Just:
                    isAP = isAP && true;
                    break;
                // 不是perfect
                case PerfectType.None:
                    isAP = false;
                    break;
            }
            // 可以算combo
            var resultValue = (int)result;
            if ((comboJudge & resultValue) > 0)
            {
                combo++;
                // 这么写是为了在false的情况下避免多余的计算
                // 这样需要走两次dic的索引
                // isAP = isAP && (record[JudgeResult.Perfect] + record[JudgeResult.PERFECT] == currentJudgedNoteCount);
            }
            else
            {
                combo = 0;
                isFC = false;
                isAP = false;
            }

            // 累计最大combo
            if (Combo > MaxCombo)
            {
                maxCombo = Combo;
            }

            // 得分情况，之后会写到配置里读？或者就直接在这写死?
            switch (result)
            {
                case JudgeResult.PERFECT:
                    totalScore += scorePerNote + 1;
                    break;
                case JudgeResult.Perfect:
                    totalScore += scorePerNote;
                    break;
                case JudgeResult.Good:
                    totalScore += (int)(scorePerNote * 0.7f + 0.5f);
                    break;
                case JudgeResult.Bad:
                    totalScore += (int)(scorePerNote * 0.3f + 0.5f);
                    break;
                case JudgeResult.Miss:
                    break;
                default:
                    break;
            }

            if (additiveScoreCount > 0 && result > JudgeResult.Miss)
            {
                additiveScoreCount--;
                totalScore++;
            }

            // 发送成绩变动事件
            onScoreChanged?.Invoke(combo, TotalScore, isFC, isAP);
        }

        /// <summary>
        /// 获取最终成绩类型
        /// </summary>
        /// <returns>成绩类型</returns>
        public ScoreType GetScoreResult()
        {
            ScoreType res;

            if (TotalNoteCounts == MaxCombo)
            {
                // fc、ap、理论
                if (record[JudgeResult.PERFECT] == MaxCombo)
                {
                    res = ScoreType.Theory;
                }
                else if (record[JudgeResult.Perfect] + record[JudgeResult.PERFECT] == MaxCombo)
                {
                    res = ScoreType.AllPerfect;
                }
                else
                {
                    res = ScoreType.FullCombo;
                }
            }
            else
            {
                res = ScoreType.Clear;
            }

            return res;
        }

        /// <summary>
        /// 获取最终成绩的图标类型（
        /// </summary>
        /// <returns>图标类型</returns>
        public ScoreGrade GetScoreResultGrade()
        {
            return ScoreType.Theory == GetScoreResult() ? ScoreGrade.Maximum : GameConfigs.GetGrade(totalScore);
        }

        /// <summary>
        /// 获取最终成绩
        /// </summary>
        /// <returns>最终成绩</returns>
        public ResultRecordParameter GetResultRecord()
        {
            return new ResultRecordParameter(PERFECT, Perfect, Good, Bad, Miss,
                MaxCombo, TotalScore, GetScoreResultGrade(),
                earlyPerfectCount, latePerfectCount,
                GetScoreResult());
        }

        /// <summary>
        /// 设置得分配置
        /// </summary>
        /// <param name="totalNoteCount">note（判定）数量</param>
        public void SetScoreSetting(int totalNoteCount)
        {
            TotalNoteCounts = totalNoteCount;
            // 避免精度问题
            scorePerNote = 1000000 / totalNoteCount;
            additiveScoreCount = 1000000 % totalNoteCount;
        }

        public void SetBannedState(bool isBanned)
        {
            this.isBanned = isBanned;
            Logger.Log($"isBanned: {isBanned}", Color.yellow);
        }

        /// <summary>
        /// 添加对成绩变化的监听
        /// </summary>
        /// <param name="onScoreChanged">成绩变化委托，参数为combo和总分</param>
        public void AddScoreChangedListener(Action<int, int, bool, bool> onScoreChanged)
        {
            this.onScoreChanged += onScoreChanged;
        }

        public void RemoveListeners()
        {
            foreach (var action in onScoreChanged.GetInvocationList())
            {
                onScoreChanged -= action as Action<int, int, bool, bool>;
            }
        }

        #endregion
    }
}