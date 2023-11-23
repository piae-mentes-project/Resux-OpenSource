using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Resux.LevelData;

namespace Resux.Data
{
    /// <summary>
    /// 玩家打歌记录
    /// </summary>
    public static class PlayerRecordManager
    {
        #region properties

        /// <summary>
        /// 打歌记录，一个List了，使用LINQ
        /// </summary>
        private static List<ScoreRecord> playResults = new();

        /// <summary>
        /// 按音乐id分组
        /// </summary>
        private static IEnumerable<IGrouping<int, ScoreRecord>> playResultGroups => playResults.GroupBy(record => record.MusicId);


        /// <summary>总游玩歌曲数</summary>
        public static int TotalPlayMusicCount => playResultGroups.Count();
        /// <summary>总游玩/Clear谱面次数</summary>
        public static int TotalClearCount => playResults.Count;
        /// <summary>总AP谱面次数</summary>
        public static int AllPerfectCount => playResults.Count(record => record.ScoreType.TypeEquals(ScoreType.AllPerfect));
        /// <summary>总FC谱面次数</summary>
        public static int FullComboCount => playResults.Count(record => record.ScoreType.TypeEquals(ScoreType.FullCombo));
        /// <summary>总理论谱面次数</summary>
        public static int TheoryCount => playResults.Count(record => record.ScoreType.TypeEquals(ScoreType.Theory));

        #endregion

        #region Pubilc Methods

        /// <summary>
        /// 初始化
        /// </summary>
        static PlayerRecordManager()
        {

        }

        /// <summary>
        /// 初始化打歌的存档
        /// </summary>
        public static void InitRecords(List<ScoreRecord> scoreRecords)
        {
            playResults = scoreRecords;
        }

        /// <summary>
        /// 查询关卡游玩记录，没打返回null
        /// </summary>
        public static ScoreRecord QueryPlayResultRecord(int levelId, Difficulty difficulty)
        {
            var result = (from record in playResults
                          where record.MusicId == levelId && record.Difficulty == difficulty
                          select record);
            if (!result.Any())
            {
                // 无游玩记录
                return null;
            }
            return result.First();
        }

        /// <summary>
        /// 是否游玩了关卡（除非只查询，而不需要具体的数据，否则不推荐用）
        /// </summary>
        public static bool IsPlayed(int levelId, Difficulty difficulty)
        {
            return QueryPlayResultRecord(levelId, difficulty) != null;
        }

        /// <summary>
        /// 添加打歌记录（不new best不更新）
        /// </summary>
        /// <param name="id">歌曲id</param>
        /// <param name="difficulty">打歌难度</param>
        /// <param name="resultRecord">打歌记录</param>
        /// <returns>是否是new best, 分差多大</returns>
        public static (bool isNewBest, int diff) SetRecord(int id, Difficulty difficulty, ResultRecordParameter resultRecord)
        {
            var playResult = QueryPlayResultRecord(id, difficulty);
            if (playResult == null)
            {
                // 没记录则新建
                playResults.Add(new ScoreRecord
                {
                    MusicId = id,
                    Difficulty = difficulty,
                    Score = resultRecord.Score,
                    ScoreType = resultRecord.ScoreType
                });

                return (true, resultRecord.Score);
            }
            else
            {
                if (playResult.Score < resultRecord.Score)
                {
                    playResult.Score = resultRecord.Score;
                    playResult.ScoreType = resultRecord.ScoreType;
                    if (resultRecord.ScoreType > playResult.BestScoreType)
                    {
                        playResult.BestScoreType = resultRecord.ScoreType;
                    }
                    return (true, resultRecord.Score - playResult.Score);
                }
                else
                {
                    return (false, 0);
                }
            }
        }

        /// <summary>
        /// 查询<paramref name="difficulty"/>难度的<paramref name="scoreType"/>次数
        /// </summary>
        /// <param name="difficulty">难度</param>
        /// <param name="scoreType">成绩类型</param>
        /// <returns>次数</returns>
        public static int QueryPlayResultRecordCount(Difficulty difficulty, ScoreType scoreType)
        {
            return playResults.Count(record => record.Difficulty == difficulty && record.BestScoreType.TypeEquals(scoreType));
        }

        /// <summary>
        /// 计数给定列表中任何难度歌曲的对应计数方法下的总数
        /// </summary>
        /// <param name="musicConfigs"></param>
        /// <param name="countFunc"></param>
        /// <returns></returns>
        public static int QueryRecordCountInList(IEnumerable<LevelData.LevelDetail> musicConfigs, Func<ScoreRecord, bool> countFunc)
        {
            var count = 0;
            var resultGroup = playResultGroups.Where(group => musicConfigs.Any(config => config._id == group.Key));
            foreach (var resultRecordGroup in resultGroup)
            {
                foreach (var resultRecord in resultRecordGroup)
                {
                    if (countFunc(resultRecord))
                    {
                        count++;
                        break;
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// 计数给定音乐列表的给定难度下以给定计数方式计算的总数
        /// </summary>
        /// <param name="musicConfigs"></param>
        /// <param name="difficulty"></param>
        /// <param name="countFunc"></param>
        /// <returns></returns>
        public static int QueryRecordCountInList(IEnumerable<LevelData.LevelDetail> musicConfigs, Difficulty difficulty,
            Func<ScoreRecord, bool> countFunc)
        {
            var count = 0;

            foreach (var musicConfig in musicConfigs)
            {
                var record = playResults.Find(result =>
                    result.Difficulty == difficulty && result.MusicId == musicConfig._id);
                if (record == null)
                {
                    continue;
                }

                if (countFunc(record))
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// 临时做的简单存档
        /// </summary>
        public static void SaveRecord()
        {
            var savedArchive = new SavedArchive();
            savedArchive.ScoreRecords = playResults;
            FileUtils.SaveArchive(savedArchive);
        }

        #endregion

        #region Private Methods

        

        #endregion
    }
}