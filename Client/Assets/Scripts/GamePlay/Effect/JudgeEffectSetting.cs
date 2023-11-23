using System.Collections;
using System.Collections.Generic;
using Resux.GamePlay.Judge;
using UnityEngine;

namespace Resux.GamePlay
{
    /// <summary>
    /// 判定特效（画面、音效等）
    /// </summary>
    public static class JudgeEffectSetting
    {
        #region 特效预制体

        private static string EffectPrefabPathAssets => Assets.AssetsFilePathDM.EffectPrefabPath_Assets;

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
            EarlyPerfectEffectPrefab = Resources.Load<GameObject>($"{EffectPrefabPathAssets}/EarlyEffect");
            LatePerfectEffectPrefab = Resources.Load<GameObject>($"{EffectPrefabPathAssets}/LateEffect");
            PerfectEffectPrefab = Resources.Load<GameObject>($"{EffectPrefabPathAssets}/PerfectEffect");
            GoodEffectPrefab = Resources.Load<GameObject>($"{EffectPrefabPathAssets}/GoodEffect");
            BadEffectPrefab = Resources.Load<GameObject>($"{EffectPrefabPathAssets}/BadEffect");
            FullComboEffectPrefab = Resources.Load<GameObject>($"{EffectPrefabPathAssets}/FullComboEffect");
            AllPerfectEffectPrefab = Resources.Load<GameObject>($"{EffectPrefabPathAssets}/AllPerfectEffect");
            ScaleCirclePrefab = Resources.Load<GameObject>($"{EffectPrefabPathAssets}/CircleScaleReduce");
            HoldLoopEffectPrefab = Resources.Load<GameObject>($"{EffectPrefabPathAssets}/HoldEffect");

            Logger.Log("特效资源加载完成");
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

        /// <summary>
        /// 获取对应结算结果的特效预制体
        /// </summary>
        /// <param name="score">结算结果</param>
        /// <returns>预制体，无特效时返回 <c>null</c></returns>
        public static GameObject GetResultEffectPrefab(ScoreType score)
        {
            switch (score)
            {
                case ScoreType.Clear:
                    return null;
                case ScoreType.FullCombo:
                    return FullComboEffectPrefab;
                case ScoreType.AllPerfect:
                case ScoreType.Theory:
                    return AllPerfectEffectPrefab;
                default:
                    return null;
            }
        }

        #endregion
    }
}