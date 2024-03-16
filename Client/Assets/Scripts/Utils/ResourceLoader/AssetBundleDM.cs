using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Resux.Assets
{
    /// <summary>
    /// AB包的管理，防止重复加载
    /// </summary>
    public static class AssetBundleDM
    {
        #region inner structure

        public enum AssetBundleState
        {
            NotLoad,
            Loaded
        }

        public class AssetBundleInfo
        {
            /// <summary>ab包路径</summary>
            public string Path { get; set; }
            /// <summary>ab包</summary>
            public AssetBundle AssetBundle { get; set; }
            /// <summary>上次加载时间，以此为准的一定时间后进行卸载</summary>
            public float LastLoadTime { get; set; }
            /// <summary>ab包的状态</summary>
            public AssetBundleState State { get; set; }
        }

        #endregion

        #region properties

        private static Dictionary<string, AssetBundleInfo> assetBundleCache;
        private static Dictionary<AssetBundleInfo, Coroutine> unloadingActions;
        private static float CurrentTime => Time.unscaledTime;
        
        #endregion

        static AssetBundleDM()
        {
            assetBundleCache = new Dictionary<string, AssetBundleInfo>();
            unloadingActions = new Dictionary<AssetBundleInfo, Coroutine>();
        }

        #region Public Method

        public static AssetBundle GetAssetBundle(string path, Action onUnload = null)
        {
            AssetBundleInfo abInfo = null;
            if (assetBundleCache.TryGetValue(path, out abInfo))
            {
                abInfo.LastLoadTime = Time.unscaledTime;
                switch (abInfo.State)
                {
                    case AssetBundleState.NotLoad:
                        abInfo.AssetBundle = AssetBundle.LoadFromFile(path);
                        abInfo.State = AssetBundleState.Loaded;
                        break;
                    case AssetBundleState.Loaded:
                        break;
                    default:
                        break;
                }

                return abInfo.AssetBundle;
            }

            var assetbundle = AssetBundle.LoadFromFile(path);
            if (assetbundle != null)
            {
                abInfo = new AssetBundleInfo()
                {
                    Path = path,
                    AssetBundle = assetbundle,
                    LastLoadTime = Time.unscaledTime,
                    State = AssetBundleState.Loaded,
                };
                
                var coroutine = CoroutineUtils.RunWaitUntil(() => CurrentTime - abInfo.LastLoadTime > Data.ConstConfigs.WaitABUnloadSecond,
                    () =>
                    {
                        abInfo.AssetBundle.Unload(false);
                        abInfo.State = AssetBundleState.NotLoad;
                        onUnload?.Invoke();
                        unloadingActions.Remove(abInfo);
                    });

                assetBundleCache.Add(path, abInfo);
                unloadingActions.Add(abInfo, coroutine);
                return assetbundle;
            }

            return null;
        }

        /// <summary>
        /// 强制卸载所有等待卸载的ab包
        /// </summary>
        public static void ForceUnloadAllBundles()
        {
            foreach (var action in unloadingActions)
            {
                CoroutineUtils.Stop(action.Value);
                action.Key.AssetBundle.Unload(true);
            }
        }

        #endregion
    }
}
