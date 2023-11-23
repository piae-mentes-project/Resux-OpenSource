using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 添加hold判定点命令
/// </summary>
public class AddHoldJudgeCommand : BaseCommand
{
    #region properties

    public override string Description => $"添加hold判定点：判定头位于 {holdJudgeInfo.judge.judgeTime}ms, pos: {judgePair.judgePosition}, time: {judgePair.judgeTime}";

    private InGameJudgeNoteInfo holdJudgeInfo;
    private JudgePair judgePair;
    private Action onEdit;

    #endregion

    public AddHoldJudgeCommand(InGameJudgeNoteInfo holdJudgeInfo, Vector2 pos, int time, Action onEdit = null)
    {
        this.holdJudgeInfo = holdJudgeInfo;
        this.judgePair = new JudgePair(pos, time);
        this.onEdit = onEdit;
    }

    public AddHoldJudgeCommand(InGameJudgeNoteInfo holdJudgeInfo, JudgePair judgePair, Action onEdit = null)
    {
        this.holdJudgeInfo = holdJudgeInfo;
        this.judgePair = judgePair;
        this.onEdit = onEdit;
    }

    public override void Execute()
    {
        base.Execute();

        holdJudgeInfo.AddJudge(judgePair);

        // 如果是对正在编辑的判定的编辑，就需要更新索引了，所以这里必须得更新一下
        EditingData.CurrentEditingHoldJudgeIndex = EditingData.CurrentEditingJudgeNote.judges.Count - 1;
        SceneImageRenderManager.Instance.RefreshJudgeNotePreview();
        onEdit?.Invoke();
    }

    public override void Undo()
    {
        base.Undo();
        holdJudgeInfo.DeleteJudge(judgePair, out var index, out var curves);

        EditingData.CurrentEditingHoldJudgeIndex = EditingData.CurrentEditingJudgeNote.judges.Count - 1;
        SceneImageRenderManager.Instance.RefreshJudgeNotePreview();
        onEdit?.Invoke();
    }
}
