using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class NoteListPanelView : BaseView
{
    #region Properties

    [SerializeField] private Button refreshButton;
    /// <summary>��ǰNote��ʾ��</summary>
    [SerializeField] private Transform currentNoteShowingArea;
    [SerializeField] private Transform allNoteShowingArea;
    /// <summary>note����Ԥ����</summary>
    private GameObject notePropertyPrefab;

    private List<NotePreviewInfoView> currentJudgeNotes;
    private List<NotePreviewInfoView> allJudgeNotes;

    #endregion

    #region UnityEngine

    private void OnEnable()
    {
        refreshButton.onClick.Invoke();
    }

    #endregion

    #region Public Method

    public override void Initialize()
    {
        currentJudgeNotes = new List<NotePreviewInfoView>();
        allJudgeNotes = new List<NotePreviewInfoView>();
        notePropertyPrefab = Resources.Load<GameObject>("NoteInfoPrefab");
        refreshButton.onClick.AddListener(ResetView);
    }

    public override void ResetView()
    {
        if (EditingData.CurrentEditingMap != null)
        {
            RefreshCurrentShowingNoteList();
            RefreshAllNotes();
        }
    }

    #endregion

    #region Private Method

    /// <summary>
    /// ˢ�µ�ǰ��ʾnote�б�
    /// </summary>
    private void RefreshCurrentShowingNoteList()
    {
        var showingNotes = EditingData.GetValidJudgeNotes().ToList();
        if (showingNotes.Count > 1)
        {
            showingNotes.Sort((a, b) => a.judge.judgeTime.CompareTo(b.judge.judgeTime));
        }

        var deltaCount = showingNotes.Count - currentJudgeNotes.Count;
        if (deltaCount > 0)
        {
            for (int i = 0; i < deltaCount; i++)
            {
                var view = Instantiate(notePropertyPrefab, currentNoteShowingArea).GetComponent<NotePreviewInfoView>();
                view.AddDeleteButtonClickListener(DeleteNote);
                view.AddEditButtonClickListener(EditNote);
                currentJudgeNotes.Add(view);
            }
        }
        else if (deltaCount < 0)
        {
            currentJudgeNotes.GetRange(0, -deltaCount).ForEach(view => Destroy(view.gameObject));
            currentJudgeNotes.RemoveRange(0, -deltaCount);
        }

        Debug.Log($"��ǰ�ж��б�showing count: {showingNotes.Count},  currentCount: {currentJudgeNotes.Count}");

        for (int i = 0; i < showingNotes.Count; i++)
        {
            var info = showingNotes[i];
            var view = currentJudgeNotes[i];
            // view.Reset();
            view.Initialize(info);
        }
    }

    /// <summary>
    /// ˢ��ȫ��Note�б�
    /// </summary>
    private void RefreshAllNotes()
    {
        var map = EditingData.CurrentEditingMap;
        var deltaCount = map.judgeNotes.Count - allJudgeNotes.Count;
        if (deltaCount > 0)
        {
            for (int i = 0; i < deltaCount; i++)
            {
                var view = Instantiate(notePropertyPrefab, allNoteShowingArea).GetComponent<NotePreviewInfoView>();
                view.AddDeleteButtonClickListener(DeleteNote);
                view.AddEditButtonClickListener(EditNote);
                allJudgeNotes.Add(view);
            }
        }
        else if (deltaCount < 0)
        {
            allJudgeNotes.GetRange(0, -deltaCount).ForEach(view => Destroy(view.gameObject));
            allJudgeNotes.RemoveRange(0, -deltaCount);
        }

        List<InGameJudgeNoteInfo> allNotesData = new List<InGameJudgeNoteInfo>();
        allNotesData.AddRange(map.judgeNotes);

        Debug.Log($"ȫ���ж��б�judge count: {allNotesData.Count},  View Count: {allJudgeNotes.Count}");
        
        if (allNotesData.Count > 1)
        {
            allNotesData.Sort((a, b) => a.judge.judgeTime.CompareTo(b.judge.judgeTime));
        }
        for (int i = 0; i < allNotesData.Count; i++)
        {
            var view = allJudgeNotes[i];
            var info = allNotesData[i];
            view.Initialize(info);
        }
    }

    /// <summary>
    /// ɾ��Note
    /// </summary>
    /// <param name="info">Ҫɾ�����ж�</param>
    private void DeleteNote(InGameJudgeNoteInfo info)
    {
        var command = new DeleteJudgeNoteCommand(info, judgeNote => ResetView());
        command.Execute();
    }

    /// <summary>
    /// �༭�ж�
    /// </summary>
    /// <param name="info">Ҫ�༭���ж�</param>
    private void EditNote(InGameJudgeNoteInfo info)
    {
        EditingData.CurrentEditingJudgeNote = info;
        EditPanelView.Instance.ShowView();
    }

    #endregion
}
