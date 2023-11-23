using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolsPopupView : BasePopupView
{
    #region properties

    [SerializeField] private BaseToolBarView[] toolBarViews;

    #endregion

    #region Public Method

    public override void Initialize()
    {
        base.Initialize();

        foreach (var toolBarView in toolBarViews)
        {
            toolBarView.Initialize(OnJudgeNoteChange);
        }
    }

    #endregion

    #region Private Method

    private void OnJudgeNoteChange()
    {
        GamePreviewManager.Instance.InitJudgeNoteQueue();
        GamePreviewManager.Instance.UpdatePreview();
    }

    #endregion
}
