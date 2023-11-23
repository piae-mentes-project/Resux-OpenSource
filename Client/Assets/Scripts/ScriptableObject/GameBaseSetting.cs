using System.Collections;
using UnityEngine;

/// <summary>
/// 游戏基础设置
/// </summary>
[CreateAssetMenu(fileName = "GameBaseSetting")]
public class GameBaseSetting : ScriptableObject
{
    /// <summary>
    /// debug模式开关
    /// </summary>
    [Header("是否开启Debug模式")]
    public bool debugMode;
    /// <summary>
    /// 自动游玩开关
    /// </summary>
    [Header("是否开启自动游玩")]
    public bool isAutoPlay;
}