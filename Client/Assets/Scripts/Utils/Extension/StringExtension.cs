using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LocalizedManager = Resux.Manager.LocalizedManager;

namespace Resux
{
    /// <summary>
    /// 字符串扩展方法
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// 获取本地化结果
        /// </summary>
        /// <param name="key">本地化key</param>
        /// <param name="valuesDic">替换值对照表</param>
        /// <returns>本地化结果</returns>
        public static string Localize(this string key, Dictionary<string, string> valuesDic = null)
        {
            return Localize(key, LocalizedManager.CurrentLanguage, valuesDic);
        }

        /// <summary>
        /// 获取本地化结果
        /// </summary>
        /// <param name="key">本地化key</param>
        /// <param name="valuesDic">替换值对照表</param>
        /// <returns>本地化结果</returns>
        public static string Localize(this string key, Language fromLanguage, Dictionary<string, string> valuesDic = null)
        {
            if (valuesDic == null)
            {
                return LocalizedManager.Localizer(key, fromLanguage);
            }

            var localize = LocalizedManager.Localizer(key, fromLanguage);
            string result = localize;
            foreach (var pair in valuesDic)
            {
                result = result.Replace("{" + pair.Key + "}", pair.Value);
            }

            return result;
        }
    }
}
