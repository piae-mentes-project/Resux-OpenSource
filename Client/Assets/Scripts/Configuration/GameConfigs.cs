using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Resux.GamePlay.Judge;
using Resux.LevelData;
using UnityEngine;

namespace Resux
{
    /// <summary>
    /// 游戏配置
    /// </summary>
    public static class GameConfigs
    {
        #region properties

        /// <summary>
        /// 游戏基础配置
        /// </summary>
        public static GameBaseSetting GameBaseSetting { get; }

        /// <summary>
        /// 游玩配置
        /// </summary>
        public static GamePlaySetting GamePlaySetting { get; }

        /// <summary>
        /// 判定配置
        /// </summary>
        public static Dictionary<JudgeType, Dictionary<JudgeResult, Vector2>> JudgeSetting { get; }

        /// <summary>
        /// 判定距离
        /// </summary>
        public static float Distance { get; private set; }

        /// <summary>
        /// 稳定触摸点距离
        /// </summary>
        public static float StableDistance { get; private set; }

        /// <summary>
        /// hold换手保护时间
        /// </summary>
        public static int ChangeHandProtectTime { get; private set; }

        /// <summary>
        /// 评分配置
        /// </summary>
        public static List<LevelDetail> LevelDetails { get; }

        #region GamePlayGlobalConfig

        public static bool IsOffline = false;

        #endregion

        #endregion

        static GameConfigs()
        {
            // 基础配置初始化
            var gameBaseSetting = Resources.Load<GameBaseSetting>("ScriptableAsset/GameBaseSetting");
            GameBaseSetting = gameBaseSetting;

            // 游玩配置初始化
            var gamePlaySetting = Resources.Load<GamePlaySetting>("ScriptableAsset/GamePlaySetting");
            gamePlaySetting.Init();
            GamePlaySetting = gamePlaySetting;

            // 判定配置初始化
            var judgeSetting = Resources.Load<JudgeSetting>("ScriptableAsset/JudgeSetting");
            JudgeSetting = new Dictionary<JudgeType, Dictionary<JudgeResult, Vector2>>();
            Distance = judgeSetting.Distance;
            StableDistance = judgeSetting.StableDistance;
            ChangeHandProtectTime = judgeSetting.ChangeHandProtectTime;
            if (!judgeSetting.useDefault)
            {
                foreach (var judgeRange in judgeSetting.JudgeRanges)
                {
                    var type = judgeRange.type;
                    JudgeSetting.Add(type, new Dictionary<JudgeResult, Vector2>());
                    foreach (var rangePair in judgeRange.ranges)
                    {
                        JudgeSetting[type].Add(rangePair.result, rangePair.range);
                    }
                }
            }
            else
            {
                foreach (JudgeType type in Enum.GetValues(typeof(JudgeType)))
                {
                    JudgeSetting.Add(type, new Dictionary<JudgeResult, Vector2>());
                    foreach (var rangePair in judgeSetting.DefaultRangePairs)
                    {
                        JudgeSetting[type].Add(rangePair.result, rangePair.range);
                    }
                }
            }

            // 评分配置初始化
            var levelDetailSetting = Resources.Load<LevelDetailSetting>("ScriptableAsset/LevelDetailSetting");
            LevelDetails = levelDetailSetting.LevelDetails;

            Logger.Log("初始化游戏配置");
        }

        #region Public Method

        /// <summary>
        /// 获取成绩评分
        /// </summary>
        /// <param name="score">分数</param>
        /// <returns>评分结果</returns>
        public static ScoreGrade GetGrade(int score)
        {
            for (var index = 0; index < LevelDetails.Count; index++)
            {
                if (score >= LevelDetails[index].down)
                {
                    return LevelDetails[index].Grade;
                }
            }

            return ScoreGrade.Maximum;
        }

        /// <summary>
        /// 根据适配缩放判定距离
        /// </summary>
        /// <param name="scale">缩放值</param>
        public static void AdaptationJudgeDistance(float scale)
        {
            Distance *= scale;
            StableDistance *= scale;
        }

        #endregion

        #region Private Method



        #endregion
    }
}