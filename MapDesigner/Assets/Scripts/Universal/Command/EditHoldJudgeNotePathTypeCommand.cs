using System;
using System.Collections.Generic;

/// <summary>
/// 编辑hold的曲线状态命令
/// </summary>
public class EditHoldJudgeNotePathTypeCommand : BaseCommand
{
    #region properties

    public override string Description => $"修改hold路径类型：{oldType.GetName()} -> {newType.GetName()}, start at {holdJudgeNoteInfo.judge.judgeTime}ms";

    private InGameJudgeNoteInfo holdJudgeNoteInfo;
    private HoldPathType oldType;
    private HoldPathType newType;
    private Action<HoldPathType> onEdit;

    #endregion

    public EditHoldJudgeNotePathTypeCommand(InGameJudgeNoteInfo holdJudgeNoteInfo, HoldPathType newType, Action<HoldPathType> onEdit = null)
    {
        this.holdJudgeNoteInfo = holdJudgeNoteInfo;
        this.oldType = holdJudgeNoteInfo.pathType;
        this.newType = newType;
        this.onEdit = onEdit;
    }

    public override void Execute()
    {
        // 没有发生状态变化就不执行，并且不放入撤回列表
        if (newType == oldType)
        {
            return;
        }

        base.Execute();

        holdJudgeNoteInfo.pathType = newType;
        HoldPathEditManager.Instance.RefreshNoteHoldPath(holdJudgeNoteInfo);
        onEdit?.Invoke(newType);
    }

    public override void Undo()
    {
        base.Undo();

        holdJudgeNoteInfo.pathType = oldType;
        HoldPathEditManager.Instance.RefreshNoteHoldPath(holdJudgeNoteInfo);
        onEdit?.Invoke(oldType);
    }
}
