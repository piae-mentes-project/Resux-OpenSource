using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#region enum

public enum HoldPathType
{
    /// <summary>直线</summary>
    DirectLine,
    /// <summary>贝塞尔</summary>
    BezierLine,
    /// <summary>自定义曲线</summary>
    Curve,
}

#endregion

#region class/struct

[Serializable]
public class SerializableAnimationCurve
{
    public KeyFrame[] Keyframes;

    public SerializableAnimationCurve()
    {
        Keyframes = new KeyFrame[2];
    }

    public SerializableAnimationCurve(AnimationCurve animationCurve)
    {
        Keyframes = animationCurve.keys.Select(key => new KeyFrame(key)).ToArray();
    }

    public static implicit operator AnimationCurve(SerializableAnimationCurve serializableAnimationCurve)
    {
        return new AnimationCurve(serializableAnimationCurve.Keyframes.Select(key => key.ToUnityKeyframe()).ToArray());
    }
}

[Serializable]
public struct KeyFrame
{
    /// <summary>
    ///   <para>The time of the keyframe.</para>
    /// </summary>
    public float time { get; set; }
    /// <summary>
    ///   <para>The value of the curve at keyframe.</para>
    /// </summary>
    public float value { get; set; }
    /// <summary>
    ///   <para>Sets the incoming tangent for this key. The incoming tangent affects the slope of the curve from the previous key to this key.</para>
    /// </summary>
    public float inTangent { get; set; }
    /// <summary>
    ///   <para>Sets the outgoing tangent for this key. The outgoing tangent affects the slope of the curve from this key to the next key.</para>
    /// </summary>
    public float outTangent { get; set; }
    /// <summary>
    ///   <para>Sets the incoming weight for this key. The incoming weight affects the slope of the curve from the previous key to this key.</para>
    /// </summary>
    public float inWeight { get; set; }
    /// <summary>
    ///   <para>Sets the outgoing weight for this key. The outgoing weight affects the slope of the curve from this key to the next key.</para>
    /// </summary>
    public float outWeight { get; set; }
    /// <summary>
    ///   <para>Weighted mode for the keyframe.</para>
    /// </summary>
    public WeightedMode weightedMode { get; set; }

    /// <summary>
    ///   <para>Create a keyframe.</para>
    /// </summary>
    /// <param name="time"></param>
    /// <param name="value"></param>
    /// <param name="inTangent"></param>
    /// <param name="outTangent"></param>
    /// <param name="inWeight"></param>
    /// <param name="outWeight"></param>
    public KeyFrame(
        float time,
        float value,
        float inTangent,
        float outTangent,
        float inWeight,
        float outWeight)
    {
        this.time = time;
        this.value = value;
        this.inTangent = inTangent;
        this.outTangent = outTangent;
        this.inWeight = inWeight;
        this.outWeight = outWeight;
        weightedMode = default;
    }

    public KeyFrame(Keyframe keyframe)
    {
        this.time = keyframe.time;
        this.value = keyframe.value;
        this.inTangent = keyframe.inTangent;
        this.outTangent = keyframe.outTangent;
        this.inWeight = keyframe.inWeight;
        this.outWeight = keyframe.outWeight;
        this.weightedMode = keyframe.weightedMode;
    }

    public Keyframe ToUnityKeyframe()
    {
        var keyframe = new Keyframe(time, value, inTangent, outTangent, inWeight, outWeight)
        {
            weightedMode = weightedMode
        };
        return keyframe;
    }
}

#endregion
