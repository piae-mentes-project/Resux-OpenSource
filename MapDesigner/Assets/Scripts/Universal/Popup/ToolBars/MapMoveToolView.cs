using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapMoveToolView : BaseToolBarView
{
    #region 谱面整体移动ms

    [Header("谱面整体平移")]
    [SerializeField] private InputField moveMillionSecondsInput;

    #endregion

    public override void OnOk()
    {
        var offset = Tools.TransStringToInt(moveMillionSecondsInput.text, 0);
        if (offset == 0)
        {
            Popup.ShowMessage("先填时间啊啊啊啊！", Color.red);
            return;
        }

        var command = new MoveJudgeNoteTimeCommand(EditingData.CurrentEditingMap.judgeNotes, offset, onOkButton);
        command.Execute();
    }
}
