using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HoldPreviewInfoView : BaseView
{
    public JudgePair JudgePair { get; private set; }

    /// <summary>坐标x文本</summary>
    [SerializeField] private Text PosXText;
    /// <summary>坐标y文本</summary>
    [SerializeField] private Text PosYText;
    /// <summary>时间文本</summary>
    [SerializeField] private Text TimeText;
    /// <summary>编辑按钮</summary>
    [SerializeField] private Button EditButton;
    /// <summary>删除按钮</summary>
    [SerializeField] private Button DeleteButton;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="time"></param>
    public void Initialize(JudgePair judgePair)
    {
        this.JudgePair = judgePair;
        PosXText.text = $"{judgePair.judgePosition.x}";
        PosYText.text = $"{judgePair.judgePosition.y}";
        TimeText.text = Tools.TimeFormat(judgePair.judgeTime);
    }

    public void AddDeleteButtonListener(UnityAction<HoldPreviewInfoView, JudgePair> onDelete)
    {
        DeleteButton.onClick.AddListener(() =>
        {
            onDelete(this, JudgePair);
        });
    }

    public void AddEditButtonListener(UnityAction<HoldPreviewInfoView, JudgePair> onEdit)
    {
        EditButton.onClick.AddListener(() =>
        {
            onEdit(this, JudgePair);
        });
    }

    public override void ResetView()
    {
        PosXText.text = PosYText.text = TimeText.text = "";
        EditButton.onClick.RemoveAllListeners();
        DeleteButton.onClick.RemoveAllListeners();
    }
}