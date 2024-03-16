using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Resux.Assets
{
    /// <summary>
    /// 音频加载
    /// </summary>
    public static class AudioLoader
    {
        #region properties

        private static AssetBundleResourceLoader<AudioClip> musicLoader;
        private static AssetBundleResourceLoader<AudioClip> effectLoader;
        public static AssetBundleResourceLoader<AudioClip> EffectLoader => effectLoader;
        private static AssetBundleResourceLoader<AudioClip> bgmLoader;
        public static AssetBundleResourceLoader<AudioClip> BgmLoader => bgmLoader;

        #endregion

        static AudioLoader()
        {
            effectLoader = new AssetBundleResourceLoader<AudioClip>(AssetsFilePathDM.AudioABPath);
            bgmLoader = new AssetBundleResourceLoader<AudioClip>(AssetsFilePathDM.AudioABPath);
        }

        #region Public Method

        public static void LoadMusicBundle(int chapterId)
        {
            var path = AssetsFilePathDM.GetChapterAssetBundlePath(chapterId);
            if (musicLoader == null || !musicLoader.BundlePath.Equals(path))
            {
                musicLoader = new AssetBundleResourceLoader<AudioClip>(path);
            }
        }

        public static AudioClip GetMusic(string name)
        {
            return musicLoader.GetResource(name);
        }

        public static void AddMusic(string name, AudioClip music)
        {
            musicLoader.AddResource(name, music);
        }

        public static bool IsMusicLoaded(string name)
        {
            return musicLoader.IsResourceLoaded(name);
        }

        public static AssetBundleRequest LoadMusicAsync(string name)
        {
            return musicLoader.LoadResourceAsync(name);
        }

        #endregion

        #region Private Method



        #endregion
    }
}
