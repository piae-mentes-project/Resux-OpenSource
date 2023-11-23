using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Resux;
using Resux.GamePlay.Judge;
using Resux.LevelData;

/// <summary>
/// 判定的配置资源，project面板下右键Create列表创建
/// </summary>
[CreateAssetMenu(fileName = "JudgeSetting")]
public class JudgeSetting : ScriptableObject
{
    /// <summary>
    /// 判定距离
    /// </summary>
    [Header("判定距离")]
    [Tooltip("判定距离")]
    public float Distance;

    /// <summary>
    /// 稳定触摸点距离
    /// </summary>
    [Header("稳定触摸点距离")]
    [Tooltip("稳定触摸点距离")]
    public float StableDistance;

    /// <summary>
    /// hold换手保护时间
    /// </summary>
    [Header("hold换手保护时间")]
    [Tooltip("hold换手保护时间")]
    public int ChangeHandProtectTime;

    /// <summary>
    /// 判定区间配置
    /// </summary>
    [Header("判定区间配置")]
    [Tooltip("判定区间配置")]
    public List<JudgeRange> JudgeRanges = new List<JudgeRange>();

    /// <summary>
    /// 使用默认配置
    /// </summary>
    [Header("使用默认配置")]
    public bool useDefault;

    /// <summary>
    /// 默认判定区间配置
    /// </summary>
    [Header("默认判定区间配置")]
    [Tooltip("默认判定区间配置")]
    public List<RangePair> DefaultRangePairs = new List<RangePair>();
}

/// <summary>
/// 判定区间信息
/// </summary>
[Serializable]
public struct JudgeRange
{
    /// <summary>
    /// 判定类型
    /// </summary>
    [Tooltip("判定类型")]
    public JudgeType type;

    /// <summary>
    /// 判定区间
    /// </summary>
    public RangePair[] ranges;
}

/// <summary>
/// 判定区间对
/// </summary>
[Serializable]
public struct RangePair
{
    /// <summary>
    /// 判定结果
    /// </summary>
    [Tooltip("判定结果")]
    public JudgeResult result;

    /// <summary>
    /// 判定区间
    /// </summary>
    [Tooltip("判定区间")]
    public Vector2 range;
}
