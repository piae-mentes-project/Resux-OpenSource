using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Resux.Data;
using UnityEditor;
using UnityEngine;

namespace Resux.Editor
{
    public class AssetBundleBuilder
    {
        private const string AssetBundleDirIOS = "AssetBundles/iOS";
        private const string AssetBundleDirAndroid = "AssetBundles/Android";

        private static BuildAssetBundleOptions BuildOptions =
            BuildAssetBundleOptions.ChunkBasedCompression |
            BuildAssetBundleOptions.EnableProtection;

        private static string Key => ConstDM.GetKey();

        [MenuItem("Assets/Build AssetBundle Android")]
        public static void BuildAssetBundlesAndroid()
        {
            AutoSetAssetBundleName();

            BuildPipeline.SetAssetBundleEncryptKey(Key);

            if (!Directory.Exists(AssetBundleDirAndroid))
            {
                Directory.CreateDirectory(AssetBundleDirAndroid);
            }

            // 应该是mac也能打包
            BuildPipeline.BuildAssetBundles(AssetBundleDirAndroid, BuildOptions, BuildTarget.Android);
            Debug.Log("Build AssetBundle, Target platform Android");
        }

        [MenuItem("Assets/Build AssetBundle iOS")]
        public static void BuildAssetBundlesIOS()
        {
            AutoSetAssetBundleName();

            BuildPipeline.SetAssetBundleEncryptKey(Key);

            if (!Directory.Exists(AssetBundleDirIOS))
            {
                Directory.CreateDirectory(AssetBundleDirIOS);
            }

#if UNITY_EDITOR_OSX
            BuildPipeline.BuildAssetBundles(AssetBundleDirIOS, BuildOptions, BuildTarget.iOS);
            Debug.Log("Build AssetBundle in Mac(OSX), Target platform iOS");
#endif
            AssetDatabase.Refresh();
        }

        // [MenuItem("Assets/AssetBundle AutoSet")]
        public static void AutoSetAssetBundleName()
        {
            var pathes = ConstDM.AssetBundlePathes;
            foreach (var path in pathes)
            {
                var totalPath = Path.Combine(Application.dataPath, path);
                var allSubDir = new DirectoryInfo(totalPath).GetDirectories();
                Debug.Log($"<color=yellow>execute dir: {path}</color>");
                foreach (var dir in allSubDir)
                {
                    Debug.Log($"<color=green>execute sub dir: {dir.Name}</color>");
                    var assetPath = dir.FullName.Substring(dir.FullName.IndexOf("Assets"));
                    var importer = AssetImporter.GetAtPath(assetPath);
                    importer.assetBundleName = dir.Name.ToLower();
                }
            }
        }
    }
}
