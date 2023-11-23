using System;
using System.Collections.Generic;
using System.Linq;

public class MirrorJudgeNoteInTimeRangeCommand : BaseCommand
{
    public override string Description => $"批量镜像判定：from {startTime}ms to {endTime}ms";

    private readonly int startTime;
    private readonly int endTime;
    private readonly InGameJudgeNoteInfo[] judgeNoteInfos;
    private readonly Action onEdit;

    public MirrorJudgeNoteInTimeRangeCommand(int startTime, int endTime, Action onEdit = null)
    {
        this.startTime = startTime;
        this.endTime = endTime;
        this.onEdit = onEdit;

        judgeNoteInfos = EditingData.GetJudgeNotesInTimeRange(startTime, endTime);
    }

    public override void Execute()
    {
        base.Execute();

        foreach (var judgeNoteInfo in judgeNoteInfos)
        {
            judgeNoteInfo.MirrorByCenter();
        }

        onEdit?.Invoke();
    }

    public override void Undo()
    {
        base.Undo();

        foreach (var judgeNoteInfo in judgeNoteInfos)
        {
            judgeNoteInfo.MirrorByCenter();
        }

        onEdit?.Invoke();
    }
}
