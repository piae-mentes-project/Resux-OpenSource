using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MirrorJudgeNoteCommand : BaseCommand
{
    public override string Description => $"镜像判定：{judgeNoteInfo.judge.judgeTime}ms";

    private InGameJudgeNoteInfo judgeNoteInfo;
    private Action onEdit;

    public MirrorJudgeNoteCommand(InGameJudgeNoteInfo judgeNoteInfo, Action onEdit = null)
    {
        this.judgeNoteInfo = judgeNoteInfo;
        this.onEdit = onEdit;
    }

    public override void Execute()
    {
        base.Execute();

        judgeNoteInfo.MirrorByCenter();
        onEdit?.Invoke();
    }

    public override void Undo()
    {
        base.Undo();

        judgeNoteInfo.MirrorByCenter();
        onEdit?.Invoke();
    }
}
