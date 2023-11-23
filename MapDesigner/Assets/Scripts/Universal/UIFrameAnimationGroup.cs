using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 帧动画组
/// </summary>
public class UIFrameAnimationGroup : MonoBehaviour
{
    #region properties

    [SerializeField] private UIFrameAnimation[] frameAnimations;

    private Action onAllAnimationOver;
    /// <summary>已结束的帧动画数量</summary>
    private int _overCount;

    private int overCount
    {
        get => _overCount;
        set
        {
            _overCount = value;
            if (value == frameAnimations.Length)
            {
                onAllAnimationOver?.Invoke();
            }
        }
    }

    #endregion

    #region Unity Engine

    void OnEnable()
    {
        // 重置计数
        _overCount = 0;
    }

    #endregion

    #region Public Method

    public void Initialize()
    {
        foreach (var uiFrameAnimation in frameAnimations)
        {
            uiFrameAnimation.Initialize();
            uiFrameAnimation.AddAnimationCallback(AddOverCount);
        }
    }

    public void AddAnimationCallback(Action callback)
    {
        onAllAnimationOver += callback;
    }

    #endregion

    #region Private Method

    private void AddOverCount()
    {
        overCount++;
    }

    #endregion
}