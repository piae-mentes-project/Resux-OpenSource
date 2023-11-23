using System;
using System.Collections.Generic;

/// <summary>
/// 修改半note反向状态命令
/// </summary>
public class EditHalfNoteReverseCommand : BaseCommand
{
    #region properties

    public override string Description => $"修改半note反转状态：{oldReverse} -> {!oldReverse}, start at {noteInfo.noteMoveInfo.startTime}ms";

    private EditNoteInfo noteInfo;
    private bool oldReverse;

    #endregion

    public EditHalfNoteReverseCommand(EditNoteInfo noteInfo)
    {
        this.noteInfo = noteInfo;
        oldReverse = noteInfo.noteMoveInfo.isReverse;
    }

    public override void Execute()
    {
        base.Execute();

        noteInfo.noteMoveInfo.isReverse = !oldReverse;
        noteInfo.CalculatePath();
    }

    public override void Undo()
    {
        base.Undo();

        noteInfo.noteMoveInfo.isReverse = oldReverse;
        noteInfo.CalculatePath();
    }
}
