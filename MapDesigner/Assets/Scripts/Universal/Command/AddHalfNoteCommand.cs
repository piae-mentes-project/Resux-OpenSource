using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 添加半note命令
/// </summary>
public class AddHalfNoteCommand : BaseCommand
{
    #region properties

    public override string Description => $"添加半note: halfType: {halfType}, position: {pos}, time: {time}";

    private HalfType halfType;
    private int time;
    private Vector2 pos;

    private EditNoteInfo noteInfo;
    private EditNoteInfo lastEditNoteInfo;

    #endregion

    public AddHalfNoteCommand(HalfType halfType, Vector2 pos, int time)
    {
        this.halfType = halfType;
        this.pos = pos;
        this.time = time;
    }

    public override void Execute()
    {
        base.Execute();

        if (noteInfo == null)
        {
            var halfInfo = new NoteInfo(WeightType.Light,
                halfType,
                new NoteMoveInfo(Vector2.zero, pos, time,
                    time + 1000, false),
                new List<Vector2>());
            noteInfo = new EditNoteInfo(halfInfo);
            lastEditNoteInfo = EditingData.CurrentEditingHalfNote;
            noteInfo.CalculatePath();
        }

        if (halfType == HalfType.UpHalf)
        {
            EditingData.CurrentEditingJudgeNote.movingNotes.up = noteInfo;
        }
        else
        {
            EditingData.CurrentEditingJudgeNote.movingNotes.down = noteInfo;
        }

        EditingData.CurrentEditingHalfNote = noteInfo;
        GamePreviewManager.Instance.UpdatePreview();
    }

    public override void Undo()
    {
        base.Undo();
        if (halfType == HalfType.UpHalf)
        {
            EditingData.CurrentEditingJudgeNote.movingNotes.up = null;
        }
        else
        {
            EditingData.CurrentEditingJudgeNote.movingNotes.down = null;
        }

        EditingData.CurrentEditingHalfNote = lastEditNoteInfo;
        GamePreviewManager.Instance.UpdatePreview();
    }
}
