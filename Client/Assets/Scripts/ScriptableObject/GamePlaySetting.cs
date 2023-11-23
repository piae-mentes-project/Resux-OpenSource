using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum UnifiedType
{
    None,
    Light,
    Middle,
    Weight
}

/// <summary>
/// 游玩的配置资源，project面板下右键Create列表创建
/// </summary>
[CreateAssetMenu(fileName = "GamePlaySetting")]
public class GamePlaySetting : ScriptableObject
{
    /// <summary>
    /// 重力加速度
    /// </summary>
    [Header("重力加速度m/s²")][Tooltip("重力加速度")]
    public float Gravity = -9.8f;

    /// <summary>
    /// 浮力加速度
    /// </summary>
    [Header("浮力加速度m/s²")]
    [Tooltip("浮力加速度，按照轻/中/重的顺序来填")]
    public float[] Bouyancy;

    [Header("所有note使用统一的阻力参数类型")]
    [Tooltip("当为None时各用各的")]
    public UnifiedType UnifiedResistanceType;

    [Header("请从小到大填数（自行脑部金字塔），速度m/s，加速度m/s²")]
    [Header("resistance = acc*(speed（参数） - offset)，speed为该分段的区间阈值")]
    public List<ResistanceSet> LightResistance;

    public List<ResistanceSet> MiddleResistance;

    public List<ResistanceSet> WeightResistance;

    /// <summary>
    /// 水面位置
    /// </summary>
    [Header("水面位置")]
    [Tooltip("水面位置")]
    public int WaterSurface = 720;

    public void Init()
    {
        switch (UnifiedResistanceType)
        {
            case UnifiedType.Light:
                MiddleResistance = LightResistance;
                WeightResistance = LightResistance;
                break;
            case UnifiedType.Middle:
                LightResistance = MiddleResistance;
                WeightResistance = MiddleResistance;
                break;
            case UnifiedType.Weight:
                LightResistance = WeightResistance;
                MiddleResistance = WeightResistance;
                break;
            case UnifiedType.None:
            default:
                break;
        }
    }
}

[Serializable]
public class ResistanceSet
{
    [Tooltip("曲线偏移")]
    public float offset;

    [Tooltip("速度阈值")]
    public float speed;

    [Tooltip("阻力加速度的系数")]
    public float acceleration;

    public float GetResistance(float speed)
    {
        return acceleration * (speed - offset);
    }
}
