using System;
using System.Collections.Generic;

/// <summary>
/// 删除时间范围内的判定的命令
/// </summary>
public class DeleteJudgeNoteInTimeRangeCommand : BaseCommand
{
    #region properties

    public override string Description => $"批量删除判定：from {startTime}ms to {endTime}ms";

    private readonly int startTime;
    private readonly int endTime;
    private readonly InGameJudgeNoteInfo[] judgeNoteInfos;
    private readonly Action onEdit;

    #endregion

    public DeleteJudgeNoteInTimeRangeCommand(int startTime, int endTime, Action onEdit = null)
    {
        this.startTime = startTime;
        this.endTime = endTime;
        this.onEdit = onEdit;

        judgeNoteInfos = EditingData.GetJudgeNotesInTimeRange(startTime, endTime);
    }

    public override void Execute()
    {
        base.Execute();

        foreach (var judgeNote in judgeNoteInfos)
        {
            EditingData.CurrentEditingMap.judgeNotes.Remove(judgeNote);
        }

        EditingData.RefreshEditingMapSorting();
        onEdit?.Invoke();
    }

    public override void Undo()
    {
        base.Undo();

        foreach (var judgeNote in judgeNoteInfos)
        {
            EditingData.CurrentEditingMap.judgeNotes.Add(judgeNote);
        }

        EditingData.RefreshEditingMapSorting();
        onEdit?.Invoke();
    }
}
