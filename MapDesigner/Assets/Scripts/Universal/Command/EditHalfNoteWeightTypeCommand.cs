using System;
using System.Collections.Generic;

/// <summary>
/// 修改半note重量命令
/// </summary>
public class EditHalfNoteWeightTypeCommand : BaseCommand
{
    #region properties

    public override string Description => $"修改半note重量：{oldWeight} -> {newWeight}   start at {noteInfo.noteMoveInfo.startTime}ms";

    private WeightType oldWeight;
    private WeightType newWeight;
    private EditNoteInfo noteInfo;

    #endregion

    public EditHalfNoteWeightTypeCommand(EditNoteInfo noteInfo, WeightType newWeightType)
    {
        this.noteInfo = noteInfo;
        this.newWeight = newWeightType;
        this.oldWeight = noteInfo.weightType;
    }

    public override void Execute()
    {
        base.Execute();

        noteInfo.weightType = newWeight;
        noteInfo.CalculatePath();
    }

    public override void Undo()
    {
        base.Undo();

        noteInfo.weightType = oldWeight;
        noteInfo.CalculatePath();
    }
}