using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JudgeNotePanelView : BaseView
{
    #region Properties

    /// <summary>�ж�����</summary>
    [SerializeField] private ButtonRadio judgeTypeButtonRadio;
    [SerializeField] private InputField judgeTimeInput;
    [SerializeField] private InputField judgePosXInput;
    [SerializeField] private InputField judgePosYInput;
    [SerializeField] private Button okButton;
    [SerializeField] private Button copyButton;
    [SerializeField] private Text statusHintText;

    [SerializeField] private Button setUpHalfAsPresetButton;
    [SerializeField] private Button setDownHalfAsPresetButton;

    [SerializeField] private Button toolButton;
    [SerializeField] private Button deleteButton;

    /// <summary>�Ƿ��Ǵ���</summary>
    private bool isCreating;

    #endregion

    #region Unity



    #endregion

    #region Public Method

    public override void ResetView()
    {
        UpdateView();
    }

    public override void Initialize()
    {
        judgeTypeButtonRadio.Initialize();
        okButton.onClick.AddListener(OnOkButton);
        copyButton.onClick.AddListener(OnCopyButton);
        toolButton.onClick.AddListener(OnToolButton);
        deleteButton.onClick.AddListener(OnDeleteButton);

        setUpHalfAsPresetButton.onClick.AddListener(() => SetHalfNoteToPreset(true));
        setDownHalfAsPresetButton.onClick.AddListener(() => SetHalfNoteToPreset(false));
    }

    public override void UpdateView()
    {
        isCreating = EditingData.CurrentEditingJudgeNote == null;
        if (isCreating)
        {
            statusHintText.text = $"״̬��<color=#ff0000>����</color>";
        }
        else
        {
            statusHintText.text = $"״̬��<color=#ff0000>�༭</color>";
            LoadJudgeData(EditingData.CurrentEditingJudgeNote);
        }
    }

    public void LoadJudgeData(InGameJudgeNoteInfo judgeNoteInfo)
    {
        judgeTimeInput.text = judgeNoteInfo == null ? "" : judgeNoteInfo.judge.judgeTime.ToString();
        judgePosXInput.text = judgeNoteInfo == null ? "" : judgeNoteInfo.judge.judgePosition.x.ToString();
        judgePosYInput.text = judgeNoteInfo == null ? "" : judgeNoteInfo.judge.judgePosition.y.ToString();
    }

    #endregion

    #region Private Method

    private void OnOkButton()
    {
        var info = GetJudgeDataFromInput();

        static void OnEditJudge()
        {
            SceneImageRenderManager.Instance.RefreshJudgeNotePreview();
            GamePreviewManager.Instance.UpdatePreview();
        }

        Debug.Log($"isCreating: {isCreating}, info: {info.judgeType} - {info.judgePos} - {info.judgeTime}");
        // ��Ҫ�����Ǳ༭���Ǵ���
        if (isCreating)
        {
            var command = new AddJudgeNoteCommand(info.judgeType, info.judgePos, info.judgeTime, judgeNote => OnEditJudge());
            command.Execute();
        }
        else
        {
            var command = new EditJudgeNoteCommand(EditingData.CurrentEditingJudgeNote, info.judgeType, info.judgePos, info.judgeTime, judgeNote => OnEditJudge());
            command.Execute();
        }
    }

    private void OnCopyButton()
    {
        Popup.ShowSpecialWindow<CopyPastePopupView>("CopyPastePopupView");
    }

    private void OnToolButton()
    {
        var toolView = Popup.ShowSpecialWindow<SingleJudgeNoteToolPopupView>("SingleJudgeNoteToolPopupView");
        toolView.SetJudgeNote(EditingData.CurrentEditingJudgeNote);
    }

    private void OnDeleteButton(){
        var command = new DeleteJudgeNoteCommand(EditingData.CurrentEditingJudgeNote, judgeNote => {UpdateView();});
        command.Execute();
    }

    private void SetHalfNoteToPreset(bool isUpHalf)
    {
        var popup = Popup.ShowSpecialWindow<PresetInfoPopupView>("PresetInfoPopupView");
        popup.Initialize(isUpHalf ? EditingData.CurrentEditingJudgeNote.movingNotes.up : EditingData.CurrentEditingJudgeNote.movingNotes.down);
    }

    private (JudgeType judgeType, int judgeTime, Vector2 judgePos) GetJudgeDataFromInput()
    {
        var judgeType = (JudgeType) judgeTypeButtonRadio.GetCurrentSelection();
        var judgeTime = Tools.TransStringToInt(judgeTimeInput.text);
        var posX = Tools.TransStringToFloat(judgePosXInput.text);
        var posY = Tools.TransStringToFloat(judgePosYInput.text);

        return (judgeType, judgeTime, new Vector2(posX, posY));
    }

    #endregion
}
