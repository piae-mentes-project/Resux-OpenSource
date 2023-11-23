using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapRangeMirrorToolView : BaseToolBarView
{
    #region ï¿½ï¿½ï¿½ï¿½Ö²ï¿½ï¿½ï¿½ï¿½ï¿?

    [Header("ï¿½ï¿½ï¿½ï¿½Ö²ï¿½ï¿½ï¿½ï¿½ï¿?")]
    [SerializeField] private InputField mirrorRangeStartInput;
    [SerializeField] private InputField mirrorRangeEndInput;

    #endregion

    public override void Initialize(Action onOk = null)
    {
        if(EditingData.SectionSelected1 && EditingData.SectionSelected2){
            mirrorRangeStartInput.text = ((int)(EditingData.SectionTime1+0.5f)).ToString();
            mirrorRangeEndInput.text = ((int)(EditingData.SectionTime2+0.5f)).ToString();
        }
    }

    public override void OnOk()
    {
        var start = Tools.TransStringToInt(mirrorRangeStartInput.text);
        var end = Tools.TransStringToInt(mirrorRangeEndInput.text);

        var command = new MirrorJudgeNoteInTimeRangeCommand(start, end, onOkButton);
        command.Execute();
    }
}
