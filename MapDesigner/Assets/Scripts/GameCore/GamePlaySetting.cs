using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "GamePlaySetting")]
public class GamePlaySetting : ScriptableObject
{
    [Header("重力加速度m/s²")]
    [Tooltip("重力加速度")]
    public float Gravity = -1200f;
    [Header("浮力加速度m/s²")]
    [Tooltip("浮力加速度，按照轻/中/重的顺序来填")]
    public List<float> Bouyancy = new List<float> { 2400, 1200, 600 };

    [Header("请从小到大填数（自行脑部金字塔）")]
    [Header("速度m/s，加速度m/s²")]
    [Header("resistance = acc*(speed（参数） - offset)，speed为该分段的区间阈值")]
    public List<ResistanceSet> LightResistance;
    public List<ResistanceSet> MiddleResistance;
    public List<ResistanceSet> WeightResistance;
    
    [Header("水面位置")]
    [Tooltip("水面位置")]
    public int WaterSurface = 720;
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

    public ResistanceSet(float of, float sp, float ac)
    {
        offset = of;
        speed = sp;
        acceleration = ac;
    }
    public float GetResistance(float speed)
    {
        return acceleration * (speed - offset);
    }
}
