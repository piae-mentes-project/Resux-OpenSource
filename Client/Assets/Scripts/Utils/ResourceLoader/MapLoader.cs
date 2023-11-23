using System.Collections;
using System.Collections.Generic;
using System.IO;
using Resux.LevelData;
using UnityEngine;

namespace Resux.Assets
{
    /// <summary>
    /// 谱面加载器
    /// </summary>
    public static class MapLoader
    {
        #region properties

        private static AssetBundleResourceLoader<TextAsset> musicMapJsonLoader;

        #endregion

        static MapLoader()
        {

        }

        #region Public Method

        public static void LoadMapBundle(int chapterId)
        {
            var path = AssetsFilePathDM.GetChapterAssetBundlePath(chapterId);
            if (musicMapJsonLoader == null || !musicMapJsonLoader.BundlePath.Equals(path))
            {
                musicMapJsonLoader = new AssetBundleResourceLoader<TextAsset>(path);
            }
        }

        public static void UnloadMapBundle()
        {
            musicMapJsonLoader?.UnloadResource(false);
        }

        public static MusicMap GetMusicMap(LevelData.LevelDetail levelDetail, Difficulty difficulty)
        {
            var path = $"{levelDetail._songName}_{difficulty}";
            var mapJsonText = musicMapJsonLoader.GetResource(path);
            if (mapJsonText == null)
            {
                return null;
            }

            return LoadMap(mapJsonText.text);
        }

        /// <summary>
        /// 卸载谱面缓存并清理
        /// </summary>
        public static void ClearMapCache()
        {
            musicMapJsonLoader.UnloadResource(true, () =>
            {
                musicMapJsonLoader = null;
                System.GC.Collect();
            });
        }

        #endregion

        #region Private Method

        private static MusicMap LoadMap(string json)
        {
            MusicMap map;
            try
            {
                map = Utils.ConvertJsonToObject<MusicMap>(json);
            }
            catch (System.Exception e)
            {
                Logger.LogException(e);
                return null;
            }
            return map;
        }

        #endregion
    }
}