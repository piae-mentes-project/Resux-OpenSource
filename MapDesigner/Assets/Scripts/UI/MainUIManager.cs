using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 主画布UI管理
/// </summary>
public class MainUIManager : BaseView
{
    public static MainUIManager Instance;

    #region Properties

    [SerializeField] UpPanelView upPanelView;
    [SerializeField] LeftPanelView leftPanelView;
    [SerializeField] RightPanelView rightPanelView;
    /// <summary>设置界面</summary>
    [SerializeField] SettingPanelView settingPanelView;
    /// <summary>创建/打开文件界面</summary>
    [SerializeField] private InputPanelView inputPanelView;

    [SerializeField] private RectTransform popupParent;
    [SerializeField] private UIEventTrigger gameAreaEventTrigger;

    #endregion

    #region Unity Engine

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Initialize();
        upPanelView.Initialize();
        leftPanelView.Initialize();
        rightPanelView.Initialize();
        settingPanelView.Initialize();
        inputPanelView.Initialize();
    }

    private void Update()
    {
        OnUpdate();
    }

    #endregion

    #region Public Method

    public override void Initialize()
    {
        Application.targetFrameRate = 60;
        if (!Directory.Exists(GlobalSettings.MapDataPath))
        {
            Directory.CreateDirectory(GlobalSettings.MapDataPath);
        }

        Popup.SetPopupParent(popupParent);
        gameAreaEventTrigger.onPointerEnter.AddListener(e => EditingData.IsInGameWindow = true);
        gameAreaEventTrigger.onPointerExit.AddListener(e => EditingData.IsInGameWindow = false);
    }

    public override void ResetView()
    {
        leftPanelView.ResetView();
        upPanelView.ResetView();
        rightPanelView.ResetView();
    }

    public override void OnUpdate()
    {
        upPanelView.OnUpdate();
        leftPanelView.OnUpdate();
        inputPanelView.OnUpdate();
    }

    #endregion

    #region Private Method



    #endregion
}
