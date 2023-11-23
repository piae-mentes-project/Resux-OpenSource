using System;
using System.Collections.Generic;

/// <summary>
/// 编辑hold判定点的路径参数
/// </summary>
public class EditHoldJudgeParameterCommand : BaseCommand
{
    #region

    public override string Description => $"编辑hold路径参数：({oldParam.angle} angle, {oldParam.value} value) -> ({newParam.angle} angle, {newParam.value} value)";

    private InGameJudgeNoteInfo holdJudgeNoteInfo;
    private (float angle, float value) oldParam;
    private (float angle, float value) newParam;
    private int index;
    private Action<(float angle, float value)> onEdit;

    #endregion

    public EditHoldJudgeParameterCommand(InGameJudgeNoteInfo holdJudgeNoteInfo, (float angle, float value) newParam, int index, Action<(float angle, float value)> onEdit = null)
        : this(holdJudgeNoteInfo, holdJudgeNoteInfo.pathParameters[index], newParam, index, onEdit)
    {

    }

    public EditHoldJudgeParameterCommand(InGameJudgeNoteInfo holdJudgeNoteInfo, (float angle, float value) oldParam, (float angle, float value) newParam, int index, Action<(float angle, float value)> onEdit = null)
    {
        this.holdJudgeNoteInfo = holdJudgeNoteInfo;
        this.oldParam = oldParam;
        this.newParam = newParam;
        this.index = index;
        this.onEdit = onEdit;
    }

    public override void Execute()
    {
        base.Execute();

        holdJudgeNoteInfo.pathParameters[index] = newParam;
        HoldPathEditManager.Instance.RefreshNoteHoldPath(holdJudgeNoteInfo);
        onEdit?.Invoke(newParam);
    }

    public override void Undo()
    {
        base.Undo();

        holdJudgeNoteInfo.pathParameters[index] = oldParam;
        HoldPathEditManager.Instance.RefreshNoteHoldPath(holdJudgeNoteInfo);
        onEdit?.Invoke(oldParam);
    }
}
