using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Resux.Data
{
    /// <summary>
    /// 音频内容
    /// </summary>
    public static class Sounds
    {
        /// <summary>
        /// 音效
        /// </summary>
        public static class Effect
        {
            public const string ChangeMusic = "changeMusic";
            public const string StartPlay = "StartPlay";
        }

        public static class Bgm
        {
            public const string EntryScene = "Entry";
            public const string RootScene = "Main";
            public const string MusicGroupScene = "MusicGroup";
            public const string ResultScene = "Finished";
            public const string AudioDelay = "offset";
        }
    }
}
