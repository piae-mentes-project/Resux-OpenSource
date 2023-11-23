using System;
using System.Collections;
using System.Collections.Generic;
using Resux.Data;
using Resux.Configuration;
using Resux.LevelData;

namespace Resux.GamePlay
{
    /// <summary>
    /// 玩家选择的歌曲和谱面之类的配置
    /// </summary>
    public static class MusicScoreSelection
    {
        #region properties

        private static LevelData.LevelDetail levelDetail;

        /// <summary>谱面配置</summary>
        public static LevelData.LevelDetail LevelDetail
        {
            get => levelDetail;
            set
            {
                levelDetail = value;
                InvokeListener();
            }
        }

        /// <summary>音乐id</summary>
        public static int MusicId => LevelDetail._id;

        public static string MusicName => LevelDetail._songName;

        /// <summary>谱面难度</summary>
        private static Difficulty difficulty;

        public static Difficulty Difficulty
        {
            get => difficulty;
            set
            {
                difficulty = value;
                UserLocalSettings.SetInt(ConstConfigs.LocalKey.SelectDifficulty, (int)value);
                InvokeListener();
            }
        }

        public static ChapterDetail ChapterDetail => MusicDataDM.MusicGroups[MusicGroupIndex];
        public static int MusicGroupIndex;

        /// <summary>是否自动完成</summary>
        public static bool isAutoPlay;

        #endregion

        #region Delegate

        private static Action<LevelData.LevelDetail, Difficulty> onSelectChange;

        #endregion

        static MusicScoreSelection()
        {
            LevelDetail = new LevelData.LevelDetail();
            difficulty = (Difficulty) UserLocalSettings.GetInt(ConstConfigs.LocalKey.SelectDifficulty);
            isAutoPlay = GameConfigs.GameBaseSetting.isAutoPlay;
        }

        #region Public Method

        public static void AddSelectChangeListener(Action<LevelData.LevelDetail, Difficulty> selectEvent)
        {
            onSelectChange += selectEvent;
        }

        public static void RemoveAllListener()
        {
            if (onSelectChange == null)
            {
                return;
            }

            var events = onSelectChange.GetInvocationList();
            foreach (Action<LevelData.LevelDetail, Difficulty> e in events)
            {
                onSelectChange -= e;
            }
        }

        #endregion

        #region Private Method

        private static void InvokeListener()
        {
            onSelectChange?.Invoke(LevelDetail, difficulty);
        }

        #endregion
    }
}