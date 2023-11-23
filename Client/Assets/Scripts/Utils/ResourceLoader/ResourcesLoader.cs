using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Resux.Assets
{
    /// <summary>
    /// Resources资源加载
    /// </summary>
    public class ResourcesLoader<TResource> where TResource : UnityEngine.Object
    {
        #region properties

        private Func<string, TResource> loadResourceFunc;
        private Func<string, ResourceRequest> loadResourceAsyncFunc;

        private string rootPath;

        private Dictionary<string, TResource> resourcesCache;

        #endregion

        #region Public Method

        public ResourcesLoader(string rootPath)
        {
            loadResourceFunc = Resources.Load<TResource>;
            loadResourceAsyncFunc = Resources.LoadAsync<TResource>;
            this.rootPath = rootPath;
            resourcesCache = new Dictionary<string, TResource>();
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

        public ResourceRequest LoadResourceAsync(string name)
        {
            return loadResourceAsyncFunc(name);
        }

        #endregion

        #region Private Method

        private TResource LoadResource(string name)
        {
            var path = $"{rootPath}/{name}";
            var resource = loadResourceFunc(path);
            resourcesCache.Add(name, resource);
            return resource;
        }

        #endregion
    }
}
