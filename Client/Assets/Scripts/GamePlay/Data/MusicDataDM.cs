using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Resux
{
    /// <summary>
    /// 曲目数据
    /// </summary>
    public static class MusicDataDM
    {
        #region properties

        public static int MusicCount { get; private set; }
        public static List<ChapterDetail> MusicGroups { get; private set; }

        #endregion

        static MusicDataDM()
        {
            LoadMusicConfigs();
        }

        #region Public Methos

        /// <summary>
        /// 初始化已解锁的歌
        /// </summary>
        /// <param name="unlockedMusicIds">已解锁的歌的id列表</param>
        public static void Initialize(IEnumerable<int> unlockedMusicIds)
        {
            // 所有未解锁的歌
            var totalMusics = MusicGroups.SelectMany(g => g.musicConfigs).Where(m => !m.isUnLocked);
            foreach (var id in unlockedMusicIds)
            {
                var music = totalMusics.FirstOrDefault(m => m._id == id);
                if (music == null)
                {
                    continue;
                }

                music.isUnLocked = true;
            }
        }

        /// <summary>
        /// 解锁指定歌曲
        /// </summary>
        /// <param name="id">歌曲id</param>
        public static void UnlockMusic(int id)
        {
            var chapterId = (id / 100) % 1000;
            var music = MusicGroups.Find(g => g.id == chapterId).musicConfigs.Find(mc => mc._id == id);
            if (music == null)
            {
                return;
            }

            music.isUnLocked = true;
        }

        #endregion

        #region Private Methos

        /// <summary>
        /// 加载音乐的配置信息
        /// </summary>
        private static void LoadMusicConfigs()
        {
            MusicGroups = Resources.Load<MusicDataConfig>("ScriptableAsset/MusicDataConfig").MusicGroups;
            MusicGroups.Sort((left, right) => left.id.CompareTo(right.id));
            MusicGroups.ForEach(group => group.musicConfigs.Sort((left, right) => left._id.CompareTo(right._id)));
            MusicCount = MusicGroups.Sum(group => group.musicConfigs.Count);
        }

        #endregion
    }
}