using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 移动半note的c初始位置
/// </summary>
public class EditHalfNotePositionCommand : BaseCommand
{
    #region properties

    public override string Description => $"移动半note位置：time: {time}, old pos: {lastPos}, new pos: {newPos}";

    private EditNoteInfo noteInfo;
    private Vector2 lastPos;
    private Vector2 newPos;
    /// <summary>对应判定点的时间</summary>
    private int time;

    #endregion

    public EditHalfNotePositionCommand(EditNoteInfo noteInfo, Vector2 lastPos, Vector2 newPos, int time)
    {
        this.noteInfo = noteInfo;
        this.lastPos = lastPos;
        this.newPos = newPos;
        this.time = time;
    }

    public EditHalfNotePositionCommand(EditNoteInfo noteInfo, Vector2 newPos, int time)
    {
        this.noteInfo = noteInfo;
        this.lastPos = noteInfo.noteMoveInfo.p0;
        this.newPos = newPos;
        this.time = time;
    }

    public override void Execute()
    {
        base.Execute();

        noteInfo.SetInitPos(newPos);
        EditingData.CurrentEditingHalfNote = null;
    }

    public override void Undo()
    {
        base.Undo();

        noteInfo.SetInitPos(lastPos);
        EditingData.CurrentEditingHalfNote = null;
    }
}
