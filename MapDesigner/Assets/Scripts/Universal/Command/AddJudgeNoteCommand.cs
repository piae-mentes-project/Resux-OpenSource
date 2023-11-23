using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 添加判定点指令
/// </summary>
public class AddJudgeNoteCommand : BaseCommand
{
    #region properties

    public override string Description
    {
        get => $"添加判定点: pos={position}, time={time}, type: {judgeType}";
    }

    private Vector2 position;
    private JudgeType judgeType;
    private int time;

    private Action<InGameJudgeNoteInfo> onEditJudgeNote;

    private InGameJudgeNoteInfo judgeNoteInfo;
    public InGameJudgeNoteInfo JudgeNoteInfo => judgeNoteInfo;
    private InGameJudgeNoteInfo lastEditJudgeNoteInfo;

    #endregion

    public AddJudgeNoteCommand(Vector2 pos, int time, Action<InGameJudgeNoteInfo> onEditJudgeNote = null)
        : this(JudgeType.Tap, pos, time, onEditJudgeNote)
    {

    }

    public AddJudgeNoteCommand(JudgeType judgeType, Vector2 pos, int time, Action<InGameJudgeNoteInfo> onEditJudgeNote = null)
    {
        this.judgeType = judgeType;
        position = pos;
        this.time = time;
        this.onEditJudgeNote = onEditJudgeNote;
    }

    public override void Execute()
    {
        base.Execute();

        if (judgeNoteInfo == null)
        {
            judgeNoteInfo = new InGameJudgeNoteInfo(judgeType, position, time);
        }

        EditingData.CurrentEditingMap.judgeNotes.Add(judgeNoteInfo);
        lastEditJudgeNoteInfo = EditingData.CurrentEditingJudgeNote;
        EditingData.CurrentEditingJudgeNote = judgeNoteInfo;
        onEditJudgeNote?.Invoke(judgeNoteInfo);
    }

    public override void Undo()
    {
        base.Undo();
        EditingData.CurrentEditingMap.judgeNotes.Remove(judgeNoteInfo);
        EditingData.CurrentEditingJudgeNote = lastEditJudgeNoteInfo;
        onEditJudgeNote?.Invoke(lastEditJudgeNoteInfo);
    }
}
