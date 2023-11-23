using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Resux.GamePlay
{
    /// <summary>
    /// 判定特效（画面、音效等）
    /// </summary>
    public static class JudgeEffectSetting
    {
        #region 特效预制体

        private const string EffectPrefabPathAssets = "Prefabs/GameCore/Effects";

        private static GameObject EarlyPerfectEffectPrefab;
        private static GameObject LatePerfectEffectPrefab;
        private static GameObject PerfectEffectPrefab;
        private static GameObject GoodEffectPrefab;
        private static GameObject BadEffectPrefab;
        private static GameObject FullComboEffectPrefab;
        private static GameObject AllPerfectEffectPrefab;

        public static GameObject ScaleCirclePrefab { get; private set; }
        public static GameObject HoldLoopEffectPrefab { get; private set; }

        #endregion

        static JudgeEffectSetting()
        {
            // 加载特效预制体
            PerfectEffectPrefab = Resources.Load<GameObject>($"{EffectPrefabPathAssets}/PerfectEffect");
            ScaleCirclePrefab = Resources.Load<GameObject>($"{EffectPrefabPathAssets}/CircleScaleReduce");
            HoldLoopEffectPrefab = Resources.Load<GameObject>($"{EffectPrefabPathAssets}/HoldEffect");
        }

        #region Public Method

        /// <summary>
        /// 获取对应判定结果的特效预制体
        /// </summary>
        /// <param name="result">判定结果</param>
        /// <returns>特效预制体</returns>
        public static GameObject GetEffectPrefab(JudgeResult result)
        {
            // switch穿透
            switch (result)
            {
                case JudgeResult.Bad:
                    return BadEffectPrefab;
                case JudgeResult.Good:
                    return GoodEffectPrefab;
                case JudgeResult.Perfect:
                case JudgeResult.PERFECT:
                    return PerfectEffectPrefab;
                case JudgeResult.None:
                case JudgeResult.Miss:
                default:
                    return null;
            }
        }

        /// <summary>
        /// 获取对应的early / late判定特效预制体
        /// </summary>
        /// <param name="perfectType"></param>
        /// <returns></returns>
        public static GameObject GetELPrefab(PerfectType perfectType)
        {
            switch (perfectType)
            {
                case PerfectType.Early:
                    return EarlyPerfectEffectPrefab;
                case PerfectType.Late:
                    return LatePerfectEffectPrefab;
                case PerfectType.Just:
                case PerfectType.None:
                default:
                    return null;
            }
        }

        #endregion
    }
}