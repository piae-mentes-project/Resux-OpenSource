using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapRangeXPosMoveToolView : BaseToolBarView
{
    #region 谱面局部空间x平移

    [Header("谱面局部空间x平移")]
    [SerializeField] private InputField rangeStartXSpaceInput;
    [SerializeField] private InputField rangeEndXSpaceInput;
    [SerializeField] private InputField rangeMoveXInput;

    #endregion

    public override void Initialize(Action onOk = null)
    {
        if(EditingData.SectionSelected1 && EditingData.SectionSelected2){
            rangeStartXSpaceInput.text = ((int)(EditingData.SectionTime1+0.5f)).ToString();
            rangeEndXSpaceInput.text = ((int)(EditingData.SectionTime2+0.5f)).ToString();
        }
    }

    public override void OnOk()
    {
        var start = Tools.TransStringToInt(rangeStartXSpaceInput.text, 0);
        var end = Tools.TransStringToInt(rangeEndXSpaceInput.text, 0);

        var offset = Tools.TransStringToInt(rangeMoveXInput.text, 0);
        if (start == 0 || end == 0 || end < start)
        {
            Popup.ShowMessage("先填好时间啊啊啊啊！", Color.red);
            return;
        }

        var judgeInRange = EditingData.GetJudgeNotesInTimeRange(start, end);
        var command = new MoveJudgeNotePositionCommand(judgeInRange, new Vector2(offset, 0), onOkButton);
        command.Execute();
    }
}
