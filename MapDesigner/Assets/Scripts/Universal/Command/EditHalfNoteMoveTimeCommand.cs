using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 修改半note移动时间命令
/// </summary>
public class EditHalfNoteMoveTimeCommand : BaseCommand
{
    #region properties

    public override string Description => $"修改半note移动时长：{oldMoveTime}ms -> {newMoveTime}ms, start at {noteInfo.noteMoveInfo.startTime}ms";

    private int oldMoveTime;
    private int newMoveTime;
    private EditNoteInfo noteInfo;

    #endregion

    public EditHalfNoteMoveTimeCommand(EditNoteInfo noteInfo, int newMoveTime)
    {
        this.noteInfo = noteInfo;
        this.newMoveTime = newMoveTime;
        this.oldMoveTime = noteInfo.noteMoveInfo.endTime - noteInfo.noteMoveInfo.startTime;
    }

    public override void Execute()
    {
        base.Execute();

        noteInfo.noteMoveInfo.endTime = noteInfo.noteMoveInfo.startTime + newMoveTime;
        noteInfo.CalculatePath();
    }

    public override void Undo()
    {
        base.Undo();

        noteInfo.noteMoveInfo.endTime = noteInfo.noteMoveInfo.startTime + oldMoveTime;
        noteInfo.CalculatePath();
    }
}