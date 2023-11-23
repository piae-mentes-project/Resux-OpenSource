using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapRangeDeleteToolView : BaseToolBarView
{
    #region 谱面局部删除

    [Header("谱面局部删除")]
    [SerializeField] private InputField deleteRangeStartInput;
    [SerializeField] private InputField deleteRangeEndInput;

    #endregion

    public override void Initialize(Action onOk = null)
    {
        if(EditingData.SectionSelected1 && EditingData.SectionSelected2){
            deleteRangeStartInput.text = ((int)(EditingData.SectionTime1+0.5f)).ToString();
            deleteRangeEndInput.text = ((int)(EditingData.SectionTime2+0.5f)).ToString();
        }
    }

    public override void OnOk()
    {
        var start = Tools.TransStringToInt(deleteRangeStartInput.text);
        var end = Tools.TransStringToInt(deleteRangeEndInput.text);

        var command = new DeleteJudgeNoteInTimeRangeCommand(start, end, onOkButton);
        command.Execute();
    }
}
