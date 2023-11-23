using System.Collections;
using UnityEngine;
using Resux.LevelData;
using Resux.Manager;

namespace Resux
{
    /// <summary>
    /// 枚举类型的扩展
    /// </summary>
    public static class EnumExtension
    {
        public static bool TypeEquals(this ScoreType score, ScoreType result1)
        {
            return (int)score >= (int)result1;
        }

        public static Language ToLanguage(this SystemLanguage systemLanguage)
        {
            switch (systemLanguage)
            {
                case SystemLanguage.Japanese:
                    return Language.Japanese;
                case SystemLanguage.ChineseTraditional:
                    return Language.ChineseTraditional;
                case SystemLanguage.Chinese:
                case SystemLanguage.ChineseSimplified:
                    return Language.ChineseSimplified;
                case SystemLanguage.Unknown:
                case SystemLanguage.Korean:
                case SystemLanguage.English:
                default:
                    return Language.English;
            }
        }

        public static string GetLanguageStr(this Language language)
        {
            return "LANGUAGE".Localize(language);
        }

        public static string GetName(this Difficulty difficulty)
        {
            switch (difficulty)
            {
                case Difficulty.Tale:
                    return "SONG_DIFFICULTY_1".Localize();
                case Difficulty.Romance:
                    return "SONG_DIFFICULTY_2".Localize();
                case Difficulty.History:
                    return "SONG_DIFFICULTY_3".Localize();
                case Difficulty.Revival:
                    return "SONG_DIFFICULTY_4".Localize();
                case Difficulty.Story:
                    return "SONG_DIFFICULTY_S".Localize();
                default:
                    return "";
            }
        }
    }
}