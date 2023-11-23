using System;
using System.Collections.Generic;

/// <summary>
/// 编辑hold上的判定点的命令
/// </summary>
public class EditHoldJudgeCommand : BaseCommand
{
    #region properties

    public override string Description => $"修改hold判定：index - {index}, ({oldJudge.judgeTime}ms, {oldJudge.judgePosition}) -> ({newJudge.judgeTime}ms, {newJudge.judgePosition})";

    private InGameJudgeNoteInfo holdJudgeNoteInfo;
    private int index;
    private JudgePair newJudge;
    private JudgePair oldJudge;
    private Action<JudgePair> onEdit;

    #endregion

    public EditHoldJudgeCommand(InGameJudgeNoteInfo holdJudgeNoteInfo, int index, JudgePair judgePair, Action<JudgePair> onEdit = null)
    {
        this.holdJudgeNoteInfo = holdJudgeNoteInfo;
        this.index = index;
        var oldJudge = holdJudgeNoteInfo.judges[index];
        this.oldJudge = new JudgePair(oldJudge.judgePosition, oldJudge.judgeTime);
        this.newJudge = judgePair;
        this.onEdit = onEdit;
    }

    public override void Execute()
    {
        base.Execute();

        // 不要让新的对象实例覆盖旧的
        holdJudgeNoteInfo.judges[index].judgeTime = newJudge.judgeTime;
        holdJudgeNoteInfo.judges[index].judgePosition = newJudge.judgePosition;
        onEdit?.Invoke(holdJudgeNoteInfo.judges[index]);
    }

    public override void Undo()
    {
        base.Undo();

        holdJudgeNoteInfo.judges[index].judgeTime = oldJudge.judgeTime;
        holdJudgeNoteInfo.judges[index].judgePosition = oldJudge.judgePosition;
        onEdit?.Invoke(holdJudgeNoteInfo.judges[index]);
    }
}
