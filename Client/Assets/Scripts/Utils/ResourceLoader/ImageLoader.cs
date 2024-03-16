using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Resux.Assets
{
    /// <summary>
    /// 图像加载
    /// </summary>
    public static class ImageLoader
    {
        #region properties

        private static ResourcesLoader<Texture2D> texture2DLoader;
        private static ResourcesLoader<Sprite> spriteLoader;

        private static AssetBundleResourceLoader<Texture2D> musicCoverLoader;
        public static AssetBundleResourceLoader<Texture2D> GlobalImageLoader;

        #endregion

        static ImageLoader()
        {
            // 临时
            // texture2DLoader = new ResourcesLoader<Texture2D>(AssetsFilePathDM.StoryImagePath_Assets);
            // spriteLoader = new ResourcesLoader<Sprite>(AssetsFilePathDM.StoryImagePath_Assets);

            GlobalImageLoader = new AssetBundleResourceLoader<Texture2D>(AssetsFilePathDM.ImageABPath);
        }

        #region Public Method

        public static void LoadMusicCoverBundle(int chapterId)
        {
            var path = AssetsFilePathDM.GetChapterAssetBundlePath(chapterId);
            if (musicCoverLoader == null || !musicCoverLoader.BundlePath.Equals(path))
            {
                musicCoverLoader = new AssetBundleResourceLoader<Texture2D>(path);
            }
        }

        public static Texture2D GetTexture2D(string name)
        {
            return texture2DLoader.GetResource(name);
        }

        public static void AddTexture2D(string name, Texture2D tex2D)
        {
            texture2DLoader.AddResource(name, tex2D);
        }

        public static bool IsTexture2DLoaded(string name)
        {
            return texture2DLoader.IsResourceLoaded(name);
        }

        public static Sprite GetSprite(string name)
        {
            return spriteLoader.GetResource(name);
        }

        public static void AddSprite(string name, Sprite music)
        {
            spriteLoader.AddResource(name, music);
        }
        
        public static bool IsSpriteLoaded(string name)
        {
            return spriteLoader.IsResourceLoaded(name);
        }

        public static void LoadSpriteFromUrl(string url, Action<Sprite> onSuccess)
        {
            CoroutineUtils.DownloadImage(url, onSuccess);
        }

        #region MusicCover && MusicGroupCover

        public static Texture2D GetMusicCover(string name)
        {
            return musicCoverLoader.GetResource($"{name}_Cover");
        }

        public static void AddMusicCover(string name, Texture2D tex2D)
        {
            musicCoverLoader.AddResource($"{name}_Cover", tex2D);
        }

        public static bool IsMusicCoverLoaded(string name)
        {
            return musicCoverLoader.IsResourceLoaded($"{name}_Cover");
        }

        public static AssetBundleRequest LoadMusicCoverAsync(string name)
        {
            return musicCoverLoader.LoadResourceAsync($"{name}_Cover");
        }

        #endregion

        #endregion

        #region Private Method



        #endregion
    }
}
