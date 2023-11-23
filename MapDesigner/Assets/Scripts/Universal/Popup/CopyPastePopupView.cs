using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CopyPastePopupView : BasePopupView
{
    #region properties

    [SerializeField] private InputField copyStartTime;
    [SerializeField] private InputField copyEndTime;
    [SerializeField] private InputField pasteStartTime;
    [SerializeField] private Toggle horizontalSymmetryCopy;

    [SerializeField] private Button okButton;

    #endregion

    #region Public Method

    public override void Initialize()
    {
        base.Initialize();
        okButton.onClick.AddListener(CopyPaste);

        if(EditingData.SectionSelected1 && EditingData.SectionSelected2){
            copyStartTime.text = ((int)(EditingData.SectionTime1+0.5f)).ToString();
            copyEndTime.text = ((int)(EditingData.SectionTime2+0.5f)).ToString();
            pasteStartTime.text = ((int)(MusicPlayManager.Instance.MusicTime*1000+0.5f)).ToString();
        }
    }

    #endregion

    #region Private Method

    private void CopyPaste()
    {
        var copyFrom = Tools.TransStringToInt(copyStartTime.text);
        var copyEnd = Tools.TransStringToInt(copyEndTime.text);

        var pasteFrom = Tools.TransStringToInt(pasteStartTime.text);
        var isHorizontalSymmetryCopy = horizontalSymmetryCopy.isOn;

        if (copyEnd < copyFrom)
        {
            Popup.ShowMessage("ʱ�䷶Χ�ú��kora��", Color.red);
        }

        var judgeNotesInRange = EditingData.GetJudgeNotesInTimeRange(copyFrom, copyEnd);
        if (judgeNotesInRange == null || judgeNotesInRange.Length == 0)
        {
            Popup.ShowMessage("��ѡʱ��û���ж���", Color.red);
            return;
        }

        var timeOffset = pasteFrom - copyFrom;
        var appendJudgeNotes = new List<InGameJudgeNoteInfo>();

        foreach (var judgeNote in judgeNotesInRange)
        {
            var newJudgeNote = new InGameJudgeNoteInfo(judgeNote);

            // ʱ���ƶ�
            newJudgeNote.MoveTimeOffset(timeOffset);

            // ���᾵��
            if (isHorizontalSymmetryCopy)
            {
                newJudgeNote.MirrorByCenter();
            }
            else
            {
                if (newJudgeNote.judgeType == JudgeType.Hold)
                {
                    // ��Ҫ�������߲����ĶԳ�
                    var length = newJudgeNote.pathParameters.Count;
                    for (int i = 0; i < length; i++)
                    {
                        var newAngle = newJudgeNote.pathParameters[i].angle;
                        newJudgeNote.pathParameters[i] = (newAngle, newJudgeNote.pathParameters[i].value);
                    }
                }
            }

            newJudgeNote.RefreshPath();

            Debug.Log($"raw path length: {judgeNote.movingNotes.up.path.Length}, new path length: {newJudgeNote.movingNotes.up.path.Length}");
            Debug.Log($"raw total path length: {judgeNote.movingNotes.up.totalPath.Count}, new total path length: {newJudgeNote.movingNotes.up.totalPath.Count}");
            appendJudgeNotes.Add(newJudgeNote);
        }

        var command = new AddJudgeNoteListCommand(appendJudgeNotes);
        command.Execute();

        Popup.ShowMessage("����ճ���ɹ���", Color.blue);
    }

    #endregion
}
