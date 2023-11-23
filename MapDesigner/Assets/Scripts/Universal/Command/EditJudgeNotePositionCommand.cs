using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 移动判定点的空间位置
/// </summary>
public class EditJudgeNotePositionCommand : BaseCommand
{
    #region properties

    public override string Description => $"移动判定点位置：time: {judgeNoteInfo.judge.judgeTime}, old pos: {lastPos}, new pos: {newPos}";

    private InGameJudgeNoteInfo judgeNoteInfo;
    private Vector2 lastPos;
    private Vector2 newPos;

    #endregion

    public EditJudgeNotePositionCommand(InGameJudgeNoteInfo judgeNoteInfo, Vector2 lastPos, Vector2 newPos)
    {
        this.judgeNoteInfo = judgeNoteInfo;
        this.lastPos = lastPos;
        this.newPos = newPos;
    }

    public override void Execute()
    {
        base.Execute();

        judgeNoteInfo.judge.judgePosition = newPos;
    }

    public override void Undo()
    {
        base.Undo();

        judgeNoteInfo.judge.judgePosition = lastPos;
    }
}
