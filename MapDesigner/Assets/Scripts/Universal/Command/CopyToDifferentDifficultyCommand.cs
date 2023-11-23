using System;
using System.Collections.Generic;
using System.Linq;

public class CopyToDifferentDifficultyCommand : BaseCommand
{
    public override string Description => $"将{start}ms - {end}ms 从{fromDifficulty}复制到{toDifficulty}";

    private int start, end;
    private InGameJudgeNoteInfo[] judgeNoteInfos;
    private Difficulty fromDifficulty;
    private Difficulty toDifficulty;

    public CopyToDifferentDifficultyCommand(InGameJudgeNoteInfo[] inGameJudgeNoteInfos, Difficulty targetDifficulty)
    {
        judgeNoteInfos = inGameJudgeNoteInfos.OrderBy(judgeNote => judgeNote.judge.judgeTime)
            .Select(judgeNote => new InGameJudgeNoteInfo(judgeNote)).ToArray();
        start = judgeNoteInfos.First().judge.judgeTime;
        end = judgeNoteInfos.Last().judge.judgeTime;
        fromDifficulty = EditingData.CurrentMapDifficulty;
        toDifficulty = targetDifficulty;
    }

    public CopyToDifferentDifficultyCommand(InGameJudgeNoteInfo judgeNoteInfo, Difficulty targetDifficulty)
    {
        start = end = judgeNoteInfo.judge.judgeTime;
        judgeNoteInfos = new[] { new InGameJudgeNoteInfo(judgeNoteInfo) };
        fromDifficulty = EditingData.CurrentMapDifficulty;
        toDifficulty = targetDifficulty;
    }

    public CopyToDifferentDifficultyCommand(int start, int end, Difficulty targetDifficulty)
    {
        this.start = start;
        this.end = end;
        judgeNoteInfos = EditingData.GetJudgeNotesInTimeRange(start, end)
            .Select(judgeNote => new InGameJudgeNoteInfo(judgeNote)).ToArray();
        fromDifficulty = EditingData.CurrentMapDifficulty;
        toDifficulty = targetDifficulty;
    }

    public override void Execute()
    {
        base.Execute();

        var map = EditingData.EditingMapDic[toDifficulty];
        foreach (var judgeNote in judgeNoteInfos)
        {
            map.judgeNotes.Add(judgeNote);
        }
    }

    public override void Undo()
    {
        base.Undo();

        var map = EditingData.EditingMapDic[toDifficulty];
        foreach (var judgeNote in judgeNoteInfos)
        {
            map.judgeNotes.Remove(judgeNote);
        }
    }
}
