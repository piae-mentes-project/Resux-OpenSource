using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NotePreviewInfoView : MonoBehaviour
{
    public InGameJudgeNoteInfo info { get; private set; }
    public JudgeType Type { get; private set; }

    /// <summary>类型文本</summary>
    [SerializeField] private Text TypeText;
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
    /// <param name="info"></param>
    public void Initialize(InGameJudgeNoteInfo info)
    {
        Type = info.judgeType;
        TypeText.text = info.judgeType.GetName();
        var pos = info.judge.judgePosition;
        var time = info.judge.judgeTime;
        PosXText.text = $"{pos.x}";
        PosYText.text = $"{pos.y}";
        this.info = info;
        TimeText.text = Tools.TimeFormat(time);
    }

    public void AddDeleteButtonClickListener(UnityAction<InGameJudgeNoteInfo> onDeleteClick)
    {
        DeleteButton.onClick.AddListener(() =>
        {
            onDeleteClick(info);
        });
    }

    public void AddEditButtonClickListener(UnityAction<InGameJudgeNoteInfo> onEditClick)
    {
        EditButton.onClick.AddListener(() =>
        {
            onEditClick(info);
        });
    }

    public void ResetView()
    {
        TypeText.text = PosXText.text = PosYText.text = TimeText.text = "";
        EditButton.onClick.RemoveAllListeners();
        DeleteButton.onClick.RemoveAllListeners();
    }
}