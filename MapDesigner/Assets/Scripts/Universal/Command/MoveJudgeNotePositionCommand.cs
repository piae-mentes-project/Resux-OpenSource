using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 水平平移判定命令
/// </summary>
public class MoveJudgeNotePositionCommand : BaseCommand
{
    #region properties

    public override string Description => $"移动判定：平移{offset}, 数量{judgeNoteInfos.Count}个";

    private Vector2 offset;
    private ICollection<InGameJudgeNoteInfo> judgeNoteInfos;
    private Action onEdit;

    #endregion

    public MoveJudgeNotePositionCommand(ICollection<InGameJudgeNoteInfo> judgeNoteInfos, Vector2 offset, Action onEdit = null)
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
            judgeNote.MovePosition(offset);
        }

        onEdit?.Invoke();
    }

    public override void Undo()
    {
        base.Undo();

        foreach (var judgeNote in judgeNoteInfos)
        {
            judgeNote.MovePosition(-offset);
        }

        onEdit?.Invoke();
    }
}
