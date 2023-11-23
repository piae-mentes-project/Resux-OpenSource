using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapRangeMoveToolView : BaseToolBarView
{
    #region properties

    [Header("谱面局部平移")]
    [SerializeField] private InputField rangeStartMsInput;
    [SerializeField] private InputField rangeEndMsInput;
    [SerializeField] private InputField rangeMoveMillionSecondsInput;

    #endregion

    public override void Initialize(Action onOk = null)
    {
        if(EditingData.SectionSelected1 && EditingData.SectionSelected2){
            rangeStartMsInput.text = ((int)(EditingData.SectionTime1+0.5f)).ToString();
            rangeEndMsInput.text = ((int)(EditingData.SectionTime2+0.5f)).ToString();
        }
    }

    public override void OnOk()
    {
        var start = Tools.TransStringToInt(rangeStartMsInput.text, 0);
        var end = Tools.TransStringToInt(rangeEndMsInput.text, 0);

        var offset = Tools.TransStringToInt(rangeMoveMillionSecondsInput.text, 0);
        if (start == 0 || end == 0 || end < start || offset == 0)
        {
            Popup.ShowMessage("先填好时间啊啊啊啊！", Color.red);
            return;
        }

        var judgeInRange = EditingData.GetJudgeNotesInTimeRange(start, end);
        var command = new MoveJudgeNoteTimeCommand(judgeInRange, offset, onOkButton);
        command.Execute();
    }
}
