using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RuntimeCurveEditor;

public class RTAnimationCurveEditManager : MonoBehaviour
{
    public static RTAnimationCurveEditManager Instance;

    #region properties

    [SerializeField] private RTAnimationCurve RTAnimationCurve;
    [SerializeField] private Vector2 curveYRange = Vector2.up;

    private AnimationCurve currentCurve;

    #endregion

    private void Awake()
    {
        Instance = this;
        Initialize();
    }

    #region Public Methods

    /// <summary>
    /// 显示指定的曲线
    /// </summary>
    /// <param name="animationCurve"></param>
    public void ShowEditorView(AnimationCurve animationCurve)
    {
        if (null != currentCurve)
        {
            RTAnimationCurve.Remove(currentCurve);
        }

        RTAnimationCurve.Add(ref animationCurve);
        currentCurve = animationCurve;
        // 只对active的曲线生效，所以每次都要设置一下
        RTAnimationCurve.SetGradXRange(0, 1);
        RTAnimationCurve.SetGradYRange(curveYRange.x, curveYRange.y);
        RTAnimationCurve.ShowCurveEditor();
    }

    public void AddEditWindowOpenListener(Action onOpen)
    {
        RTAnimationCurve.AddWindowOpenListener(onOpen);
    }

    public void AddEditWindowCloseListener(Action onClose)
    {
        RTAnimationCurve.AddWindowCloseListener(onClose);
    }

    public void AddCurveChangedListener(Action onChanged)
    {
        RTAnimationCurve.AddAnimationCurveChangedListener(onChanged);
    }

    #endregion

    #region Private Methods

    private void Initialize()
    {
        // 只有init过之后才能set，所以先show，set完再close
        RTAnimationCurve.ShowCurveEditor();
        RTAnimationCurve.SetGradXRange(0, 1);
        RTAnimationCurve.SetGradYRange(curveYRange.x, curveYRange.y);
        RTAnimationCurve.SetUseInitLimitRect(true);
        RTAnimationCurve.SetCurveInitXRange(0, 1);
        RTAnimationCurve.SetCurveInitYRange(0, 1);
        RTAnimationCurve.CloseCurveEditor();

        AddCurveChangedListener(() =>
        {
            // Debug.Log("Animation Curve Edited");
            HoldPathEditManager.Instance.RefreshNoteHoldPath();
        });

        AddEditWindowOpenListener(() =>
        {
            Debug.Log("Curve Editor Window Open");
            // 编辑期间要禁用鼠标的一些操作以及编辑器的撤回系统，不然会撞到这个插件的撤回系统
            MapEditManager.Instance.EnableEdit = false;
            PlayerInputManager.Instance.EnableUndoAndRedo = false;
        });

        AddEditWindowCloseListener(() =>
        {
            Debug.Log("Curve Editor Window Close");
            MapEditManager.Instance.EnableEdit = true;
            PlayerInputManager.Instance.EnableUndoAndRedo = true;
            HoldPathEditManager.Instance.RefreshNoteHoldPath();
        });
    }

    #endregion
}
