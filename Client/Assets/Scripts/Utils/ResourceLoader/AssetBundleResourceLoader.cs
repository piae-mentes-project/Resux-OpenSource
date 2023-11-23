using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Resux.Assets
{
    /// <summary>
    /// AssetBundle资源加载
    /// </summary>
    public class AssetBundleResourceLoader<TResource> where TResource : UnityEngine.Object
    {
        #region properties

        private AssetBundle bundle;
        private AssetBundle Bundle
        {
            get
            {
                if (bundle == null)
                {
                    bundle = AssetBundleDM.GetAssetBundle(BundlePath);
                }

                return bundle;
            }
        }

        public string BundlePath { get; private set; }
        private Dictionary<string, TResource> resourcesCache;

        #endregion

        #region Public Method

        public AssetBundleResourceLoader(AssetBundle assetBundle)
        {
            bundle = assetBundle;

            resourcesCache = new Dictionary<string, TResource>();
        }

        public AssetBundleResourceLoader(string path) : this(AssetBundleDM.GetAssetBundle(path))
        {
            BundlePath = path;
        }

        public TResource GetResource(string name)
        {
            return resourcesCache.ContainsKey(name) ? resourcesCache[name] : LoadResource(name);
        }

        public void AddResource(string name, TResource music)
        {
            if (resourcesCache.ContainsKey(name))
            {
                return;
            }

            resourcesCache.Add(name, music);
        }

        public bool IsResourceLoaded(string name)
        {
            return resourcesCache.ContainsKey(name);
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        public AssetBundleRequest LoadResourceAsync(string name)
        {
            return Bundle.LoadAssetAsync(name);
        }

        /// <summary>
        /// 卸载资源，<paramref name="forceUnload"/>为 <c>true</c> 时强制卸载已加载的资源
        /// </summary>
        /// <param name="forceUnload">是否强制卸载已加载资源</param>
        /// <param name="onUnload">卸载后的回调</param>
        public void UnloadResource(bool forceUnload = false, Action onUnload = null)
        {
            if (forceUnload)
            {
                resourcesCache.Clear();
            }

            if (bundle == null)
            {
                return;
            }

            AssetBundleDM.UnLoadAssetBundle(bundle, forceUnload, onUnload);
        }

        #endregion

        #region Private Method

        private TResource LoadResource(string name)
        {
            var resource = Bundle.LoadAsset<TResource>(name);
            resourcesCache.Add(name, resource);
            return resource;
        }

        #endregion
    }
}