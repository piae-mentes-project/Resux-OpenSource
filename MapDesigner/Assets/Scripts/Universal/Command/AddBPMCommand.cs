using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 添加BPM命令
/// </summary>
public class AddBPMCommand : BaseCommand
{
    #region properties

    public override string Description => $"添加BPM：bpm-{beatInfo.bpm} in {time}";

    private Action<(Beat beat, float bpm)> addAction;
    private (Beat beat, float bpm) beatInfo;
    private Action<BPMInfoView, (Beat beat, float bpm)> deleteAction;
    private BPMInfoView bpmInfoView;
    private float? time;

    #endregion

    public AddBPMCommand(Action<(Beat beat, float bpm)> addAction,
        (Beat beat, float bpm) beatInfo,
        Action<BPMInfoView, (Beat beat, float bpm)> deleteAction)
    {
        this.addAction = addAction;
        this.beatInfo = beatInfo;
        this.deleteAction = deleteAction;
    }

    public override void Execute()
    {
        base.Execute();

        addAction(beatInfo);
    }

    public override void Undo()
    {
        base.Undo();

        deleteAction(bpmInfoView, beatInfo);
    }
}
