using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Resux.Editor
{
    public class AssetEditor
    {
        private const string ScorePathTag = "Score";

        [MenuItem("Tools/RenameScoreAsset")]
        public static void RenameScores()
        {
            var allAssetsPath = AssetDatabase.GetAllAssetPaths();
            foreach (var assetPath in allAssetsPath)
            {
                if (!assetPath.Contains(ScorePathTag))
                {
                    continue;
                }

                // 把Score之前的路径部分摘掉
                var splits = assetPath.Substring(assetPath.IndexOf(ScorePathTag)).Split('/');
                if (splits.Length < 3)
                {
                    continue;
                }

                var length = splits.Length;
                var assetName = splits[length - 1];
                // 说明改过了
                if (assetName.Contains("_"))
                {
                    continue;
                }

                var newName = $"{splits[length - 2]}_{splits[length - 1]}";
                AssetDatabase.RenameAsset(assetPath, newName);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
