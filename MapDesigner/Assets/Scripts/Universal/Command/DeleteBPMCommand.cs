using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 删除bpm命令
/// </summary>
public class DeleteBPMCommand : BaseCommand
{
    #region properties

    public override string Description => $"删除BPM：bpm-{beatInfo.bpm} in {time}";

    private (Beat beat, float bpm) beatInfo;
    private float time;
    private BPMInfoView infoView;
    private Action<BPMInfoView, (Beat beat, float bpm)> deleteAction;
    private Action<(Beat beat, float bpm)> addAction;

    #endregion

    public DeleteBPMCommand((Beat beat, float bpm) beatInfo, float time, BPMInfoView infoView, Action<BPMInfoView, (Beat beat, float bpm)> deleteAction, Action<(Beat beat, float bpm)> addAction)
    {
        this.beatInfo = beatInfo;
        this.time = time;
        this.infoView = infoView;
        this.deleteAction = deleteAction;
        this.addAction = addAction;
    }

    public override void Execute()
    {
        base.Execute();

        deleteAction(infoView, beatInfo);
    }

    public override void Undo()
    {
        base.Undo();

        addAction(beatInfo);
    }
}