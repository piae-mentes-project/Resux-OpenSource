using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

namespace Resux.Manager
{
    /// <summary>本地化</summary>
    public static class LocalizedManager
    {
        #region properties

        /// <summary>当前语言</summary>
        public static Language CurrentLanguage { get; private set; }

        /// <summary>本地化语言包</summary>
        static Dictionary<Language, Dictionary<string, string>> languagePack;

        public static List<string> TipKeys { get; private set; }

        #endregion

        #region Public Method

        // 改为静态构造函数， 系统自动调用
        static LocalizedManager()
        {
            // 从配置文件加载
            CurrentLanguage = PlayerGameSettings.Setting.Language;
            languagePack = new Dictionary<Language, Dictionary<string, string>>();
            TipKeys = new List<string>();
            foreach (Language language in Enum.GetValues(typeof(Language)))
            {
                LoadLanguagePack(language);
            }
        }

        public static void RefreshSetting()
        {
            CurrentLanguage = PlayerGameSettings.Setting.Language;
        }

        /// <summary>
        /// 获取语言本地化结果
        /// </summary>
        /// <param name="key">本地化key</param>
        /// <returns>本地化结果</returns>
        public static string Localizer(string key)
        {
            return Localizer(key, CurrentLanguage);
        }

        /// <summary>
        /// 获取语言本地化结果
        /// </summary>
        /// <param name="key">本地化key</param>
        /// <param name="fromLanguage">语言</param>
        /// <returns>本地化结果</returns>
        public static string Localizer(string key, Language fromLanguage)
        {
            // 没翻译过的直接返回原key
            if (!languagePack[fromLanguage].ContainsKey(key))
            {
                return key;
            }

            // 对于换行的处理
            var value = languagePack[fromLanguage][key];
            return value.Replace("\\n", "\n");
        }

        #endregion

        #region Private Method

        /// <summary>
        /// 加载本地化语言资源
        /// </summary>
        private static void LoadLanguagePack(Language language)
        {
            // 从Resources/Languages下读取语言资源
            var languageContent = Resources.Load<TextAsset>($"Languages/{language}");
            var languageMap = new Dictionary<string, string>();
            Logger.Log($"Load Language: {language}");
            // 解析语言资源并放入languageMap中
            var separator = new[] { "[to]" };
            foreach (var translate in languageContent.text.Split('\n'))
            {
                if (string.IsNullOrEmpty(translate))
                {
                    continue;
                }

                try
                {
                    var sourceAndTranslatedText = translate.Split(separator, StringSplitOptions.None);
                    languageMap[sourceAndTranslatedText[0]] = sourceAndTranslatedText[1];
                    var key = sourceAndTranslatedText[0];
                    if (key.StartsWith("TIPS_"))
                    {
                        TipKeys.Add(key);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Error: Language: {language}, message: {ex.Message}");
                }
            }
            Logger.Log($"{language}: key-value count: {languageMap.Count}");
            // 最后赋值
            languagePack[language] = languageMap;
        }

        #endregion
    }
}