using System.Collections;
using UnityEngine;

namespace Resux.Assets
{
    /// <summary>
    /// 资源文件的路径数据
    /// </summary>
    public static class AssetsFilePathDM
    {
        #region LocalDataPath

        public static string DataPath = Application.persistentDataPath;
        public static string TempDataPath = Application.temporaryCachePath;
        public static string StreamingAssetPath = Application.streamingAssetsPath;

        public static string PlayerSettingPath = $"{DataPath}/GamePreSetting.conf";

        public static string MusicDataPath = $"{DataPath}/Songs";
        public static string MusicConfigPath = $"{DataPath}/music.conf";

        public static string StoryDataPath = $"{DataPath}/Story";
        public static string StoryConfigPath = $"{StoryDataPath}/story.conf";
        public static string StoryImagePath = $"{StoryDataPath}/Image";

        public static string AllLogsPath = $"{TempDataPath}/AllLogs.log";
        public static string ErrorLogsPath = $"{TempDataPath}/ErrorLogs.log";
        public static string LogBufferPath = $"{TempDataPath}/LocalLogBuffer.log";

        public static string AudioABPath = $"{StreamingAssetPath}/audio";
        public static string ImageABPath = $"{StreamingAssetPath}/image";
        public static string ScoreABPath = $"{StreamingAssetPath}/score";
        public static string ChapterABPath = $"{StreamingAssetPath}/chapter";
        public static string StoryABPath = $"{StreamingAssetPath}/story";

        public static string dataDBPath = $"{StreamingAssetPath}/global.db";

        #endregion

        #region AssetsDataPath

        public const string MusicCoverPath_Assets = "Image/Cover/Songs";
        public const string MusicGroupCoverPath_Assets = "Image/Cover/MusicGroup";
        public const string MapDifficultyFramePath_Assets = "Image/MusicFrame";
        public const string StoryCoverPath_Assets = "Image/Cover/Chapter";
        public const string StoryImagePath_Assets = "Image/Feature/Story";
        public const string HalfNoteImagePath_Assets = "Image/Note";
        public const string MapPlotImagePath_Assets = "Image/Map";

        public const string UIAssetsPath_Assets = "Image/UIAssets";
        public const string UniversalUIAssetsPath_Assets = "Image/UIAssets/Universal";

        public const string MusicAudioPath_Assets = "Audio/Music";
        public const string EffectAudioPath_Assets = "Audio/Effect";

        public const string StoryDataPath_Assets = "Feature/Story";
        public const string StoryModelPath_Assets = "Prefabs/Feature/Story";

        public const string EffectPrefabPath_Assets = "Prefabs/Effects";

        public const string MaterialPath_Assets = "Materials";

        #endregion

        #region Public Method

        public static string GetChapterAssetBundlePath(int chapterId)
        {
            return $"{ChapterABPath}{chapterId}";
        }

        public static string GetStoryChapterAssetBundlePath(int chapterId)
        {
            return $"{ChapterABPath}_story{chapterId}";
        }

        #endregion
    }
}