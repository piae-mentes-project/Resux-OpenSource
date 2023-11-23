using System.Collections;
using Resux.LevelData;
using UnityEngine;

namespace Resux.Data
{
    /// <summary>
    /// 玩家个人信息数据
    /// </summary>
    public static class PlayerInfoManager
    {
        #region properties

        #region Record

        public static int PlayedMusicCount => PlayerRecordManager.TotalPlayMusicCount;
        public static int ClearCount => PlayerRecordManager.TotalClearCount;
        public static int FullComboCount => PlayerRecordManager.FullComboCount;
        public static int AllPerfectCount => PlayerRecordManager.AllPerfectCount;
        public static int TheoryCount => PlayerRecordManager.TheoryCount;

        #endregion

        #endregion

        static PlayerInfoManager()
        {

        }

        #region Public Method

        /// <summary>
        /// 获取<paramref name="difficulty"/>难度的<paramref name="scoreType"/>次数
        /// </summary>
        /// <param name="difficulty">难度</param>
        /// <param name="scoreType">成绩类型</param>
        /// <returns>次数</returns>
        public static int GetRecordCount(Difficulty difficulty, ScoreType scoreType = ScoreType.Clear)
        {
            return PlayerRecordManager.QueryPlayResultRecordCount(difficulty, scoreType);
        }

        #endregion
    }
}