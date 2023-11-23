using System;
using System.Collections.Generic;

/// <summary>
/// 偏移判定时间命令
/// </summary>
public class MoveJudgeNoteTimeCommand : BaseCommand
{
    #region properties

    public override string Description => $"偏移判定时间：偏移量{offset}ms, 数量{judgeNoteInfos.Count}个";

    private int offset;
    private ICollection<InGameJudgeNoteInfo> judgeNoteInfos;
    private Action onEdit;

    #endregion

    public MoveJudgeNoteTimeCommand(ICollection<InGameJudgeNoteInfo> judgeNoteInfos, int offset, Action onEdit = null)
    {
        this.judgeNoteInfos = judgeNoteInfos;
        this.offset = offset;
        this.onEdit = onEdit;
    }

    public override void Execute()
    {
        base.Execute();

        foreach (var judgeNote in judgeNoteInfos)
        {
            judgeNote.MoveTimeOffset(offset);
        }

        onEdit?.Invoke();
    }

    public override void Undo()
    {
        base.Undo();

        foreach (var judgeNote in judgeNoteInfos)
        {
            judgeNote.MoveTimeOffset(-offset);
        }

        onEdit?.Invoke();
    }
}
