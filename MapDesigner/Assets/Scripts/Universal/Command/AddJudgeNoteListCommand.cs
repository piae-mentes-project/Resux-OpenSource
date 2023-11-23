using System;
using System.Collections.Generic;

/// <summary>
/// 添加判定列表
/// </summary>
public class AddJudgeNoteListCommand : BaseCommand
{
    #region

    public override string Description => $"添加判定列表：共计{judgeNoteInfos.Count}个";

    private ICollection<InGameJudgeNoteInfo> judgeNoteInfos;
    private Action onEdit;

    #endregion

    public AddJudgeNoteListCommand(ICollection<InGameJudgeNoteInfo> judgeNoteInfos, Action onEdit = null)
    {
        this.judgeNoteInfos = judgeNoteInfos;
        this.onEdit = onEdit;
    }

    public override void Execute()
    {
        base.Execute();

        foreach (var judgeNote in judgeNoteInfos)
        {
            EditingData.CurrentEditingMap.judgeNotes.Add(judgeNote);
        }

        onEdit?.Invoke();
    }

    public override void Undo()
    {
        base.Undo();

        foreach (var judgeNote in judgeNoteInfos)
        {
            EditingData.CurrentEditingMap.judgeNotes.Remove(judgeNote);
        }

        onEdit?.Invoke();
    }
}
