using System;
using System.Collections.Generic;

/// <summary>
/// 判定时间缩放命令（基于BPM）
/// </summary>
public class ScaleJudgeNoteTimeCommand : BaseCommand
{
    #region properties

    public override string Description => $"缩放判定时间：比例{scale}, 数量{judgeNoteInfos.Count}个";

    private float scale;
    private ICollection<InGameJudgeNoteInfo> judgeNoteInfos;
    private Action onEdit;

    #endregion

    public ScaleJudgeNoteTimeCommand(ICollection<InGameJudgeNoteInfo> judgeNoteInfos, float scale, Action onEdit = null)
    {
        this.judgeNoteInfos = judgeNoteInfos;
        this.scale = scale;
        this.onEdit = onEdit;
    }

    public override void Execute()
    {
        base.Execute();

        foreach (var judgeNote in judgeNoteInfos)
        {
            judgeNote.ScaleTime(scale);
        }

        onEdit?.Invoke();
    }

    public override void Undo()
    {
        base.Undo();

        foreach (var judgeNote in judgeNoteInfos)
        {
            judgeNote.ScaleTime(1f / scale);
        }

        onEdit?.Invoke();
    }
}
