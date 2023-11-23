using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapRangeYPosMoveToolView : BaseToolBarView
{
    #region 谱面局部空间y平移

    [Header("谱面局部空间y平移")]
    [SerializeField] private InputField rangeStartYSpaceInput;
    [SerializeField] private InputField rangeEndYSpaceInput;
    [SerializeField] private InputField rangeMoveYInput;

    #endregion

    public override void Initialize(Action onOk = null)
    {
        if(EditingData.SectionSelected1 && EditingData.SectionSelected2){
            rangeStartYSpaceInput.text = ((int)(EditingData.SectionTime1+0.5f)).ToString();
            rangeEndYSpaceInput.text = ((int)(EditingData.SectionTime2+0.5f)).ToString();
        }
    }

    public override void OnOk()
    {
        var start = Tools.TransStringToInt(rangeStartYSpaceInput.text, 0);
        var end = Tools.TransStringToInt(rangeEndYSpaceInput.text, 0);

        var offset = Tools.TransStringToInt(rangeMoveYInput.text, 0);
        if (start == 0 || end == 0 || end < start)
        {
            Popup.ShowMessage("先填好时间啊啊啊啊！", Color.red);
            return;
        }

        var judgeInRange = EditingData.GetJudgeNotesInTimeRange(start, end);
        var command = new MoveJudgeNotePositionCommand(judgeInRange, new Vector2(0, offset), onOkButton);
        command.Execute();
    }
}
