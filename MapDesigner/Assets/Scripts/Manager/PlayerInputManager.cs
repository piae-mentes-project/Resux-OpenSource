using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 输入管理
/// </summary>
public class PlayerInputManager : MonoBehaviour
{
    public static PlayerInputManager Instance;

    #region Properties

    #region Delegate

    public event Action OnLeftMouseDownAction;
    public event Action OnLeftMouseKeepDownAction;
    public event Action OnLeftMouseUpAction;
    public event Action OnSpaceKeyDownAction;
    public event Action<float> OnScrollAction;
    public event Action<int> OnLeftCtrlAndNumKeyDown;
    /// <summary>事实上是撤回快捷键</summary>
    public event Action OnCtrlZAction;
    /// <summary>事实上是重做快捷键</summary>
    public event Action OnCtrlYAction;
    /// <summary>事实上是Preview快捷键</summary>
    public event Action OnCtrlPAction;
    /// <summary>事实上是CV工程师</summary>
    public event Action OnCtrlCAction;
    /// <summary>事实上还是CV工程师</summary>
    public event Action OnCtrlVAction;
    /// <summary>👈方向键，参数是“是否ctrl了”</summary>
    public event Action<bool> OnLeftArrow;
    /// <summary>👉方向键，参数是“是否ctrl了”</summary>
    public event Action<bool> OnRightArrow;

    #endregion

    public bool EnableUndoAndRedo = true;

    #endregion

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        RefreshData();

        var isLeftControlKeepDown = Input.GetKey(KeyCode.LeftControl);

        if (EditingData.IsInGameWindow)
        {
            SceneImageRenderManager.Instance.DrawCoordinateGuideLine();
        }

        if (Input.GetMouseButtonDown(0))
        {
            OnLeftMouseDownAction?.Invoke();
        }

        if (Input.GetMouseButton(0))
        {
            OnLeftMouseKeepDownAction?.Invoke();
        }

        if (Input.GetMouseButtonUp(0))
        {
            OnLeftMouseUpAction?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnSpaceKeyDownAction?.Invoke();
        }

        var scrollDeltaValue = Input.mouseScrollDelta;
        if (Mathf.Abs(scrollDeltaValue.y) > 1e-5)
        {
            OnScrollAction?.Invoke(scrollDeltaValue.y);
        }

        if (isLeftControlKeepDown)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                OnLeftCtrlAndNumKeyDown?.Invoke(0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                OnLeftCtrlAndNumKeyDown?.Invoke(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                OnLeftCtrlAndNumKeyDown?.Invoke(2);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                OnLeftCtrlAndNumKeyDown?.Invoke(3);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                OnLeftCtrlAndNumKeyDown?.Invoke(4);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                OnLeftCtrlAndNumKeyDown?.Invoke(5);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                OnLeftCtrlAndNumKeyDown?.Invoke(6);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                OnLeftCtrlAndNumKeyDown?.Invoke(7);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                OnLeftCtrlAndNumKeyDown?.Invoke(8);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                OnLeftCtrlAndNumKeyDown?.Invoke(9);
            }

            if (EnableUndoAndRedo)
            {
                if (Input.GetKeyDown(KeyCode.Z))
                {
                    OnCtrlZAction?.Invoke();
                }
                else if (Input.GetKeyDown(KeyCode.Y))
                {
                    OnCtrlYAction?.Invoke();
                }
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                OnCtrlPAction?.Invoke();
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                OnCtrlCAction?.Invoke();
            }

            if (Input.GetKeyDown(KeyCode.V))
            {
                OnCtrlVAction?.Invoke();
            }
        }

#if UNITY_EDITOR
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.P))
        {
            OnCtrlPAction?.Invoke();
        }
#endif

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            OnLeftArrow?.Invoke(isLeftControlKeepDown);
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            OnRightArrow?.Invoke(isLeftControlKeepDown);
        }
    }

#region Public Method



#endregion

#region Private Method

    /// <summary>
    /// 刷新数据
    /// </summary>
    private void RefreshData()
    {
        var mousePositionInMainWindow = (Input.mousePosition.x, Input.mousePosition.y);
        var gameWindowPosition = EditingData.GameWindowPosition;
        var gameWindowSize = EditingData.GameWindowSize;
        EditingData.MousePositionInMainPanel = ImageRenderManager.Instance.TransToCanvasPoint(Mathf.Clamp(mousePositionInMainWindow.x, gameWindowPosition.x, gameWindowPosition.x + gameWindowSize.width), Mathf.Clamp(mousePositionInMainWindow.y, gameWindowPosition.y, gameWindowPosition.y + gameWindowSize.height));
        EditingData.MousePositionInGameArea = ResolutionManager.Instance.TransToGamePosition(mousePositionInMainWindow.x, mousePositionInMainWindow.y);
        EditingData.MousePositionInGameAreaWithScale = Tools.ScaleToGameScenePosition(new Vector2(EditingData.MousePositionInGameArea.x, EditingData.MousePositionInGameArea.y));
        if (GlobalSettings.IsPosLimitActive)
        {
            EditingData.MousePositionInGameAreaWithScale = Tools.LimitPosition(EditingData.MousePositionInGameAreaWithScale);
            var posInGameArea = Tools.ScaleToGameWindowPosition(EditingData.MousePositionInGameAreaWithScale);
            var posInWindow = ResolutionManager.Instance.TransToScenePosition(posInGameArea);
            EditingData.MousePositionInMainPanel = ImageRenderManager.Instance.TransToCanvasPoint(Mathf.Clamp(posInWindow.x, gameWindowPosition.x, gameWindowPosition.x + gameWindowSize.width), Mathf.Clamp(posInWindow.y, gameWindowPosition.y, gameWindowPosition.y + gameWindowSize.height));
        }
        EditingData.GameAreaLeftDownPositionInMainWindow = ResolutionManager.Instance.TransToScenePosition(0, 0);
        EditingData.GameAreaRightUpPositionInMainWindow = ResolutionManager.Instance.TransToScenePosition(1920, 1080);
        EditingData.GameAreaLeftDownPositionInMainPanel = ImageRenderManager.Instance.TransToCanvasPoint(EditingData.GameAreaLeftDownPositionInMainWindow.x, EditingData.GameAreaLeftDownPositionInMainWindow.y);
        EditingData.GameAreaRightUpPositionInMainPanel = ImageRenderManager.Instance.TransToCanvasPoint(EditingData.GameAreaRightUpPositionInMainWindow.x, EditingData.GameAreaRightUpPositionInMainWindow.y);
    }

#endregion
}
