using System;
using System.Collections.Generic;
using UnityEngine;

public class DeleteHoldJudgePointCommand : BaseCommand
{
    public override string Description => $"删除hold判定点，{judgeNoteInfo.judge.judgeTime}ms处判定的{{{judgePair.judgeTime}ms，{judgePair.judgePosition}}}";

    private InGameJudgeNoteInfo judgeNoteInfo;
    private JudgePair judgePair;
    private (float angle, float value) param;
    private int curveIndex;
    private (AnimationCurve xCurve, AnimationCurve yCurve) curves;
    private Action onEdit;

    public DeleteHoldJudgePointCommand(InGameJudgeNoteInfo judgeNoteInfo,
        JudgePair judgePair, Action onEdit = null)
    {
        this.judgeNoteInfo = judgeNoteInfo;
        this.judgePair = judgePair;
        var index = judgeNoteInfo.judges.IndexOf(judgePair);
        this.param = judgeNoteInfo.pathParameters[index];
        this.onEdit = onEdit;
    }

    public override void Execute()
    {
        base.Execute();

        judgeNoteInfo.DeleteJudge(judgePair, out curveIndex, out curves);
        onEdit?.Invoke();
    }

    public override void Undo()
    {
        base.Undo();

        judgeNoteInfo.AddJudge(judgePair, param, curveIndex, curves);
        onEdit?.Invoke();
    }
}
