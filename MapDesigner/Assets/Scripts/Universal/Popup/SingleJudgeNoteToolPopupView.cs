using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SingleJudgeNoteToolPopupView : BasePopupView
{
    #region properties

    #region 复制

    [SerializeField] private InputField targetTimeInputField;
    [SerializeField] private Button copyButton;

    #endregion

    #region 镜像

    [Space]
    [SerializeField] private Button mirrorButton;

    #endregion

    #region 跨难度复制

    [Space]
    [SerializeField] private Dropdown difficultySelection;
    [SerializeField] private Button copyToDifficultyButton;

    #endregion

    private InGameJudgeNoteInfo judgeNoteInfo;

    #endregion

    public override void Initialize()
    {
        base.Initialize();

        difficultySelection.options.Clear();
        var difficultyNames = new List<string>();
        foreach (Difficulty difficulty in System.Enum.GetValues(typeof(Difficulty)))
        {
            difficultyNames.Add(difficulty.ToString());
        }

        difficultySelection.AddOptions(difficultyNames);

        copyButton.onClick.AddListener(OnCopyButton);
        mirrorButton.onClick.AddListener(OnMirrorButton);
        copyToDifficultyButton.onClick.AddListener(OnCopyToDifferentDifficultyButton);
    }

    public void SetJudgeNote(InGameJudgeNoteInfo judgeNoteInfo)
    {
        this.judgeNoteInfo = judgeNoteInfo;
    }

    private void OnCopyButton()
    {
        var targetTime = Tools.TransStringToInt(targetTimeInputField.text);
        var copyJudgeNote = new InGameJudgeNoteInfo(judgeNoteInfo);
        copyJudgeNote.MoveTimeTo(targetTime);
        var addJudgeNotes = new InGameJudgeNoteInfo[] { copyJudgeNote };
        var command = new AddJudgeNoteListCommand(addJudgeNotes, OnJudgeNoteChange);
        command.Execute();
    }

    private void OnMirrorButton()
    {
        var command = new MirrorJudgeNoteCommand(judgeNoteInfo, OnJudgeNoteChange);
        command.Execute();
    }

    private void OnCopyToDifferentDifficultyButton()
    {
        var targetDifficulty = (Difficulty)difficultySelection.value;

        var command = new CopyToDifferentDifficultyCommand(judgeNoteInfo, targetDifficulty);
        command.Execute();
    }

    private void OnJudgeNoteChange()
    {
        GamePreviewManager.Instance.InitJudgeNoteQueue();
        GamePreviewManager.Instance.UpdatePreview();
    }
}
