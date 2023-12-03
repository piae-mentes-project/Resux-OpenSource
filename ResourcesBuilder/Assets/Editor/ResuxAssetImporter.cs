using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ResuxAssetImporter : AssetPostprocessor
{
    private const string ScorePathTag = "Score";
    private const string MusicPathTag = "Music";
    private static string[] MusicExtensions = new string[]
    {
        "ogg",
        "mp3",
        "wav",
    };

    // 必须为静态的，所有资源的导入后处理
    static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        foreach (var importedAsset in importedAssets)
        {
            var ext = Path.GetExtension(importedAsset).ToLower();

            // 对谱面命名的处理
            if (importedAsset.Contains(ScorePathTag))
            {
                // 把Score之前的路径部分摘掉
                var splits = importedAsset.Substring(importedAsset.IndexOf(ScorePathTag)).Split('/');
                // Score/歌名/谱面，比这个短的被认为是无需处理的文件夹
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
                AssetDatabase.RenameAsset(importedAsset, newName);
            }
        }
    }

    #region 音频处理

    public void OnPreprocessAudio()
    {
        Debug.Log("音频导入前预处理：导入设置");
        AudioImporter audio = assetImporter as AudioImporter;

        AudioImporterSampleSettings AudioSetting = new AudioImporterSampleSettings();
        //压缩方式选择
        AudioSetting.compressionFormat = AudioCompressionFormat.Vorbis;
        //设置播放质量
        // AudioSetting.quality = 0.5f;
        //优化采样率
        AudioSetting.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;

        // 如果是音效
        if (audio.assetPath.Contains("Effect"))
        {
            AudioSetting.loadType = AudioClipLoadType.DecompressOnLoad;
            audio.loadInBackground = false;
        }
        else
        {
            AudioSetting.loadType = AudioClipLoadType.CompressedInMemory;
            audio.loadInBackground = true;
        }

        //开启单声道 
        audio.forceToMono = false;
        audio.preloadAudioData = false;
        audio.defaultSampleSettings = AudioSetting;
    }

    public void OnPostprocessAudio(AudioClip clip)
    {

    }

    #endregion
}
