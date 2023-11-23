using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 编辑bpm命令
/// </summary>
public class EditBPMCommand : BaseCommand
{
    #region properties

    public override string Description => $"编辑BPM：{oldBeatInfo.bpm} in {oldTime} -> {beatInfo.bpm} in {time}";

    private (Beat beat, float bpm) oldBeatInfo;
    private float oldTime;
    private (Beat beat, float bpm) beatInfo;
    // 在bpm更改后才能计算时间
    private float? time;
    private BPMInfoView infoView;
    private int index;

    #endregion

    public EditBPMCommand((Beat beat, float bpm) oldBeatInfo, (Beat beat, float bpm) beatInfo,
        float oldTime, BPMInfoView infoView, int index)
    {
        this.oldBeatInfo = oldBeatInfo;
        this.beatInfo = beatInfo;
        this.oldTime = oldTime;
        this.infoView = infoView;
        this.index = index;
    }

    public override void Execute()
    {
        base.Execute();

        MapDesignerSettings.BpmList[index] = beatInfo;
        if (!time.HasValue)
        {
            time = Tools.TransBeatToTime(beatInfo.beat, MapDesignerSettings.BpmList);
        }
        infoView.Initialize(beatInfo, time.Value);
    }

    public override void Undo()
    {
        base.Undo();

        MapDesignerSettings.BpmList[index] = oldBeatInfo;
        infoView.Initialize(oldBeatInfo, oldTime);
    }
}
