using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Resux;

[CreateAssetMenu(fileName = "LevelDetailSetting")]
public class LevelDetailSetting : ScriptableObject
{
    /// <summary>成绩分级配置详情</summary>
    [Header("每一个成绩对应的分数区间")]
    [Tooltip("分数区间列表")]
    public List<LevelDetail> LevelDetails = new List<LevelDetail>();
}

[Serializable]
public struct LevelDetail
{
    /// <summary>成绩</summary>
    [Header("成绩")]
    [Tooltip("成绩")]
    public ScoreGrade Grade;

    /// <summary>上限</summary>
    [Header("上限")]
    [Tooltip("上限")]
    public int up;
    /// <summary>下限</summary>
    [Header("下限")]
    [Tooltip("下限")]
    public int down;
}