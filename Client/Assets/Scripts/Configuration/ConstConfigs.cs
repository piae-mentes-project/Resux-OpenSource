using System.Collections;
using UnityEngine;

namespace Resux.Data
{
    /// <summary>
    /// 常量数据
    /// </summary>
    public static class ConstConfigs
    {
        /// <summary>概率的最大值（即1）</summary>
        public const int MaxProbability = 10000;
        /// <summary>ab包卸载的等待时间</summary>
        public const float WaitABUnloadSecond = 300f;

        #region Key

        /// <summary>
        /// 本地注册表key
        /// </summary>
        public static class LocalKey
        {
            public const string TutorialKey = "IsTutorialFinished";
            public const string AccessToken = "AccessToken";
            public const string RefreshToken = "RefreshToken";
            public const string IsFirstPlay = "IsFirstPlay";
            public const string SelectDifficulty = "SelectDifficulty";
            public const string UnityLoginAccount = "UnityLoginAccount";
        }

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