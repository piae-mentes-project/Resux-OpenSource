using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 编辑整个判定信息命令
/// </summary>
public class EditJudgeNoteCommand : BaseCommand
{
    #region properties

    public override string Description => $"修改判定信息：(原)位于{oldJudgeTime}ms, 修改为{newJudgeTime}ms, pos {newJudgePos}, type {newJudgeType}";

    private InGameJudgeNoteInfo judgeNoteInfo;

    private JudgeType oldJudgeType;
    private Vector2 oldJudgePos;
    private int oldJudgeTime;

    private JudgeType newJudgeType;
    private Vector2 newJudgePos;
    private int newJudgeTime;

    private Action<InGameJudgeNoteInfo> onEdit;

    #endregion

    public EditJudgeNoteCommand(InGameJudgeNoteInfo judgeNoteInfo, JudgeType judgeType, Vector2 judgePos, int judgeTime, Action<InGameJudgeNoteInfo> onEdit = null)
    {
        this.judgeNoteInfo = judgeNoteInfo;

        oldJudgePos = judgeNoteInfo.judge.judgePosition;
        oldJudgeTime = judgeNoteInfo.judge.judgeTime;
        oldJudgeType = judgeNoteInfo.judgeType;

        newJudgePos = judgePos;
        newJudgeTime = judgeTime;
        newJudgeType = judgeType;

        this.onEdit = onEdit;
    }

    public override void Execute()
    {
        base.Execute();

        judgeNoteInfo.judgeType = newJudgeType;
        judgeNoteInfo.MovePosition(newJudgePos - oldJudgePos);
        judgeNoteInfo.MoveTimeOffset(newJudgeTime - oldJudgeTime);
        onEdit?.Invoke(judgeNoteInfo);
    }

    public override void Undo()
    {
        base.Undo();

        judgeNoteInfo.judgeType = oldJudgeType;
        judgeNoteInfo.MovePosition(oldJudgePos - newJudgePos);
        judgeNoteInfo.MoveTimeOffset(oldJudgeTime - newJudgeTime);
        onEdit?.Invoke(judgeNoteInfo);
    }
}
