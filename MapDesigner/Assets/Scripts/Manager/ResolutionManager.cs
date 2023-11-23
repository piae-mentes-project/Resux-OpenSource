using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResolutionManager : MonoBehaviour
{
    public static ResolutionManager Instance;

    [SerializeField] private RectTransform MainCanvas;
    /// <summary>左下角</summary>
    [SerializeField] private RectTransform LeftDownCorner;
    /// <summary>右上角</summary>
    [SerializeField] private RectTransform RightUpCorner;
    /// <summary>游戏相机</summary>
    [SerializeField] private Camera GameCamera;
    /// <summary>ui相机</summary>
    [SerializeField] private Camera UICamera;
    [Tooltip("是否下帧更新")]
    [SerializeField] private bool IsNextFrameRefresh;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        RefreshData();
        IsNextFrameRefresh = true;
        RefreshScreenPosition();
    }

    private void Update()
    {
        OnUpdate();
    }

    public void OnUpdate()
    {
        if (IsNextFrameRefresh)
        {
            RefreshScreenPosition();
            IsNextFrameRefresh = false;
        }
        if (Math.Abs(Screen.width - EditingData.ScreenSize.width) > 1e-5 || Math.Abs(Screen.height - EditingData.ScreenSize.height) > 1e-5)
        {
            RefreshData();
            IsNextFrameRefresh = true;
        }
    }

    /// <summary>
    /// 刷新屏幕位置
    /// </summary>
    public void RefreshScreenPosition()
    {
        // 视口坐标为相机的归一化坐标，左下（0，0），右上（1，1）
        // 左下角屏幕位置
        Vector2 leftDownCornerScreenPos = UICamera.WorldToViewportPoint(LeftDownCorner.position);
        // 右上角屏幕位置
        Vector2 rightUpCornerScreenPos = UICamera.WorldToViewportPoint(RightUpCorner.position);
        // 屏幕大小缩放
        Vector2 screenSize = rightUpCornerScreenPos - leftDownCornerScreenPos;
        EditingData.GameWindowPosition = (leftDownCornerScreenPos.x * Screen.width, leftDownCornerScreenPos.y * Screen.height);
        EditingData.GameWindowSize = (screenSize.x * Screen.width, screenSize.y * Screen.height);
        GameCamera.rect = new Rect(leftDownCornerScreenPos.x, leftDownCornerScreenPos.y, screenSize.x, screenSize.y);
    }

    /// <summary>
    /// 转换成游戏坐标
    /// </summary>
    /// <param name="clickPosX">点击位置x</param>
    /// <param name="clickPosY">点击位置y</param>
    /// <returns>游戏坐标</returns>
    public (float x, float y) TransToGamePosition(float clickPosX, float clickPosY)
    {
        return (Tools.ScalePosToRawPos(clickPosX, EditingData.GameWindowPosition.x, EditingData.GameWindowSize.width, 1920), Tools.ScalePosToRawPos(clickPosY, EditingData.GameWindowPosition.y, EditingData.GameWindowSize.height, 1080));
    }

    /// <summary>
    /// 转换成场景坐标
    /// </summary>
    /// <param name="gamePosX">游戏坐标x</param>
    /// <param name="gamePosY">游戏坐标y</param>
    /// <returns>场景坐标</returns>
    public (float x, float y) TransToScenePosition(float gamePosX, float gamePosY)
    {
        return (Tools.ScaleRawPosToPos(gamePosX, EditingData.GameWindowPosition.x, EditingData.GameWindowSize.width, 1920), Tools.ScaleRawPosToPos(gamePosY, EditingData.GameWindowPosition.y, EditingData.GameWindowSize.height, 1080));
    }

    /// <summary>
    /// 转换成场景坐标
    /// </summary>
    /// <param name="gamePosX">游戏坐标x</param>
    /// <param name="gamePosY">游戏坐标y</param>
    /// <returns>场景坐标</returns>
    public Vector2 TransToScenePosition(Vector2 pos)
    {
        var res = TransToScenePosition(pos.x, pos.y);
        return new Vector2(res.x, res.y);
    }

    #region Private Method

    private void RefreshData()
    {
        EditingData.ScreenSize = (Screen.width, Screen.height);
        EditingData.MainCanvasSize = (MainCanvas.rect.width, MainCanvas.rect.height);
    }

    #endregion
}