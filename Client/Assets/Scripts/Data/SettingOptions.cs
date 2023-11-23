using System.Collections;
using System.Collections.Generic;
using Resux.GamePlay.Judge;
using TouchPhase = UnityEngine.TouchPhase;

namespace Resux
{
    /// <summary>所用的语言</summary>
    public enum Language
    {
        /// <summary>简中</summary>
        ChineseSimplified,
        /// <summary>繁中</summary>
        ChineseTraditional,
        English,
        Japanese,
    }

    /// <summary>计算方式</summary>
    public enum CalculateType
    {
        Multiply,
        Addition
    }
}