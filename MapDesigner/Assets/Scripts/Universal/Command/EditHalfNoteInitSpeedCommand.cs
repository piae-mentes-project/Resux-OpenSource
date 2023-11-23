using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 编辑半note初速度的命令
/// </summary>
public class EditHalfNoteInitSpeedCommand : BaseCommand
{
    #region properties

    public override string Description => $"修改初速度：{oldV0} -> {newV0}";

    private Vector2 oldV0;
    private Vector2 newV0;
    private EditNoteInfo noteInfo;
    private Action<Vector2> onEdit;

    #endregion

    public EditHalfNoteInitSpeedCommand(Vector2 newV0, EditNoteInfo noteInfo, Action<Vector2> onEdit = null)
        : this(noteInfo.noteMoveInfo.v0, newV0, noteInfo, onEdit)
    {

    }

    public EditHalfNoteInitSpeedCommand(Vector2 oldV0, Vector2 newV0, EditNoteInfo noteInfo, Action<Vector2> onEdit)
    {
        this.oldV0 = oldV0;
        this.newV0 = newV0;
        this.noteInfo = noteInfo;
        this.onEdit = onEdit;
    }

    public override void Execute()
    {
        base.Execute();

        noteInfo.SetInitV0(newV0);
        onEdit?.Invoke(newV0);
    }

    public override void Undo()
    {
        base.Undo();

        noteInfo.SetInitV0(oldV0);
        onEdit?.Invoke(oldV0);
    }
}
