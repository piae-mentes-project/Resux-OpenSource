using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 删除判定点命令
/// </summary>
public class DeleteJudgeNoteCommand : BaseCommand
{
    #region properties

    public override string Description => $"删除判定：位于{judgeNoteInfo.judge.judgeTime}";

    private InGameJudgeNoteInfo judgeNoteInfo;
    private Action<InGameJudgeNoteInfo> onEdit;

    #endregion

    public DeleteJudgeNoteCommand(InGameJudgeNoteInfo judgeNoteInfo, Action<InGameJudgeNoteInfo> onEdit = null)
    {
        this.judgeNoteInfo = judgeNoteInfo;
        this.onEdit = onEdit;
    }

    public override void Execute()
    {
        base.Execute();

        Debug.Log($"删除判定：type: {judgeNoteInfo.judgeType}, time: {judgeNoteInfo.judge.judgeTime}");
        EditingData.CurrentEditingMap.judgeNotes.Remove(judgeNoteInfo);
        onEdit?.Invoke(judgeNoteInfo);
    }

    public override void Undo()
    {
        base.Undo();

        Debug.Log($"恢复被删除的判定：type: {judgeNoteInfo.judgeType}, time: {judgeNoteInfo.judge.judgeTime}");
        EditingData.CurrentEditingMap.judgeNotes.Add(judgeNoteInfo);
        onEdit?.Invoke(judgeNoteInfo);
    }
}
