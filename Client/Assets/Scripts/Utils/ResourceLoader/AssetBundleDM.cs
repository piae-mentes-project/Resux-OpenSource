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
            Loaded,
            WaitForUnload
        }

        public class AssetBundleInfo
        {
            /// <summary>ab包路径</summary>
            public string Path { get; set; }
            /// <summary>ab包</summary>
            public AssetBundle AssetBundle { get; set; }
            /// <summary>引用计数，当计数<=0时，启动延迟卸载</summary>
            public int RefCount { get; set; }
            /// <summary>ab包的状态</summary>
            public AssetBundleState State { get; set; }
        }

        #endregion

        #region properties

        private static Dictionary<string, AssetBundleInfo> assetBundleCache;
        private static Dictionary<AssetBundleInfo, Coroutine> unloadingActions;

        #endregion

        static AssetBundleDM()
        {
            assetBundleCache = new Dictionary<string, AssetBundleInfo>();
            unloadingActions = new Dictionary<AssetBundleInfo, Coroutine>();
        }

        #region Public Method

        public static AssetBundle GetAssetBundle(string path)
        {
            if (assetBundleCache.ContainsKey(path))
            {
                var abInfo = assetBundleCache[path];
                abInfo.RefCount++;
                switch (abInfo.State)
                {
                    case AssetBundleState.NotLoad:
                        abInfo.AssetBundle = AssetBundle.LoadFromFile(path);
                        break;
                    case AssetBundleState.Loaded:
                        break;
                    case AssetBundleState.WaitForUnload:
                        CoroutineUtils.Stop(unloadingActions[abInfo]);
                        unloadingActions.Remove(abInfo);
                        abInfo.State = AssetBundleState.Loaded;
                        break;
                    default:
                        break;
                }

                return abInfo.AssetBundle;
            }

            var assetbundle = AssetBundle.LoadFromFile(path);
            if (assetbundle != null)
            {
                var abInfo = new AssetBundleInfo()
                {
                    Path = path,
                    AssetBundle = assetbundle,
                    RefCount = 1,
                    State = AssetBundleState.Loaded,
                };

                assetBundleCache.Add(path, abInfo);
                return assetbundle;
            }

            return null;
        }

        public static void UnLoadAssetBundle(AssetBundle assetBundle, bool isForce = false, Action onUnload = null)
        {
            var abInfo = assetBundleCache.Values.FirstOrDefault(info => info.AssetBundle == assetBundle);
            if (abInfo == null)
            {
                return;
            }

            abInfo.RefCount--;
            if (abInfo.RefCount <= 0 && !unloadingActions.ContainsKey(abInfo))
            {
                abInfo.State = AssetBundleState.WaitForUnload;
                var coroutine = CoroutineUtils.RunDelay(
                    () =>
                    {
                        abInfo.AssetBundle.Unload(isForce);
                        abInfo.State = AssetBundleState.NotLoad;
                        onUnload?.Invoke();
                        unloadingActions.Remove(abInfo);
                    }, Data.ConstConfigs.WaitABUnloadSecond);
                unloadingActions.Add(abInfo, coroutine);
            }
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
