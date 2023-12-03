using System.Collections;
using UnityEngine;

namespace Resux.Data
{
    /// <summary>
    /// 常量数据
    /// </summary>
    public static class ConstDM
    {
        #region AssetBundle Path

        public const string ChapterPath = "Resources/Chapters";
        public const string DefaultPath = "Resources/Default";

        public static string[] AssetBundlePathes = new string[]
        {
            DefaultPath,
            ChapterPath,
        };

        #endregion

        #region Public Method

        // 以下的一切不引起异常的“奇怪”内容均不奇怪
        public static string GetKey()
        {
            return "Rc!S-4*?Qa^fQ9xn";
        }

        #endregion
    }
}