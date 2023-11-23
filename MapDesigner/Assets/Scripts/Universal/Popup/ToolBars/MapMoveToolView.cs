using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapMoveToolView : BaseToolBarView
{
    #region ���������ƶ�ms

    [Header("��������ƽ��")]
    [SerializeField] private InputField moveMillionSecondsInput;

    #endregion

    public override void OnOk()
    {
        var offset = Tools.TransStringToInt(moveMillionSecondsInput.text, 0);
        if (offset == 0)
        {
            Popup.ShowMessage("����ʱ�䰡��������", Color.red);
            return;
        }

        var command = new MoveJudgeNoteTimeCommand(EditingData.CurrentEditingMap.judgeNotes, offset, onOkButton);
        command.Execute();
    }
}
