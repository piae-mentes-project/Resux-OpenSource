using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapBpmScaleToolView : BaseToolBarView
{
    #region properties

    [Header("谱面BPM缩放")]
    [SerializeField] private InputField targetSingleBpmInput;

    #endregion

    public override void OnOk()
    {
        var currentBpm = MapDesignerSettings.BpmList[0].bpm;
        var targetBpm = Tools.TransStringToFloat(targetSingleBpmInput.text);
        if (targetBpm == 0)
        {
            Popup.ShowMessage("请输入目标bpm", Color.red);
            return;
        }

        var scale = targetBpm / currentBpm;
        var command = new ScaleJudgeNoteTimeCommand(EditingData.CurrentEditingMap.judgeNotes, scale, onOkButton);
        command.Execute();
    }
}
