using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 左侧面板UI
/// </summary>
public class LeftPanelView : BaseView
{
    #region inner type

    /// <summary>
    /// 面板
    /// </summary>
    public enum PanelType
    {
        JudgeList,
        BPMList,
        Setting
    }

    [Serializable]
    public class PanelData
    {
        public Button Button;
        public GameObject Panel;
        public PanelType Type;
    }

    #endregion

    #region Properties

    [SerializeField] private List<PanelData> panels = new List<PanelData>();
    [SerializeField] private EditPanelView editPanelView;
    [SerializeField] private NoteListPanelView noteListPanelView;
    [SerializeField] private BpmPanelView bpmPanelView;
    [SerializeField] private ToolsPanelView toolsPanelView;

    #endregion

    #region Public Method

    /// <summary>
    /// 初始化
    /// </summary>
    public override void Initialize()
    {
        panels.ForEach(panel => panel.Button.onClick.AddListener(() => ChangePanel(panel.Type)));
        editPanelView.Initialize();
        noteListPanelView.Initialize();
        bpmPanelView.Initialize();
        toolsPanelView.Initialize();
    }

    /// <summary>
    /// 重置面板
    /// </summary>
    public override void ResetView()
    {
        noteListPanelView.ResetView();
        bpmPanelView.ResetView();
        toolsPanelView.ResetView();
    }

    /// <summary>
    /// 面板切换
    /// </summary>
    /// <param name="type">面板类型</param>
    public void ChangePanel(PanelType type)
    {
        foreach (var panel in panels)
        {
            var isType = type == panel.Type;
            // 这样可以触发OnEnable
            panel.Panel.SetActive(isType);
            panel.Button.interactable = !isType;
        }
    }

    #endregion

    #region Private Method



    #endregion
}
