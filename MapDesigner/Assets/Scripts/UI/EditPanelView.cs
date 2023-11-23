using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditPanelView : BaseView
{
    #region inner Type

    public enum EditStep
    {
        Judge,
        HalfNote,
        HoldJudge,
        HoldPath
    }

    #endregion

    public static EditPanelView Instance;

    #region Properties

    [SerializeField] private Button showPanelButton;
    [SerializeField] private Sprite rightArrow;
    [SerializeField] private Sprite leftArrow;
    private Animator animator;

    [Space]
    [SerializeField] private GameObject menu;
    [SerializeField] private Button editJudgeBtn;
    [SerializeField] private Button editHalfNoteBtn;
    [SerializeField] private Button editHoldPathBtn;
    [Space]

    [SerializeField] private JudgeNotePanelView judgeNotePanelView;
    [SerializeField] private HalfNotePanelView halfNotePanelView;
    [SerializeField] private HoldPathPanelView holdPathPanelView;
    [Space]

    [SerializeField] private GameObject friendlyText;

    private bool isShowing;

    #endregion

    #region Unity

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        ResetView();
    }

    private void Update()
    {
        // if (EditingData.IsInGameWindow)
        // {
        //     SceneImageRenderManager.Instance.DrawCoordinateGuideLine();
        // }
    }

    #endregion

    #region Public Method

    public override void Initialize()
    {
        showPanelButton.onClick.AddListener(OnShowPanelButton);
        var image = showPanelButton.GetComponent<Image>();
        image.sprite = rightArrow;
        isShowing = false;
        animator = GetComponent<Animator>();
        menu.SetActive(false);

        judgeNotePanelView.Initialize();
        halfNotePanelView.Initialize();
        holdPathPanelView.Initialize();

        editJudgeBtn.onClick.AddListener(OnEditJudgeButton);
        editHalfNoteBtn.onClick.AddListener(OnEditHalfNoteButton);
        editHoldPathBtn.onClick.AddListener(OnEditHoldPathButton);

        MapEditManager.Instance.AddEditJudgeNoteListener(OnEditJudgeNote);

        ResetView();
    }

    public override void ResetView()
    {
        // 显示的时候重新从第一步开始
        editJudgeBtn.onClick?.Invoke();
    }

    public override void ShowView()
    {
        if (!isShowing)
        {
            showPanelButton.onClick?.Invoke();
        }
        ResetView();
    }

    public void OnEditJudgeButton()
    {
        EditingData.CurrentEditStep = EditStep.Judge;
        judgeNotePanelView.ShowView();
        halfNotePanelView.HideView();
        holdPathPanelView.HideView();

        friendlyText.SetActive(true);

        editJudgeBtn.interactable = false;
        editHalfNoteBtn.interactable = true;
        editHoldPathBtn.interactable = true;
    }

    public void OnEditHalfNoteButton()
    {
        judgeNotePanelView.HideView();
        halfNotePanelView.ShowView();
        holdPathPanelView.HideView();

        friendlyText.SetActive(false);

        editJudgeBtn.interactable = true;
        editHalfNoteBtn.interactable = false;
        editHoldPathBtn.interactable = true;
    }

    public void OnEditHoldPathButton()
    {
        if (EditingData.CurrentEditingJudgeNote == null)
        {
            Popup.ShowMessage("没有正在编辑的判定", Color.red);
        }
        else if (EditingData.CurrentEditingJudgeNote.judgeType == JudgeType.Hold)
        {
            EditingData.CurrentEditStep = EditStep.HoldJudge;
            judgeNotePanelView.HideView();
            halfNotePanelView.HideView();
            holdPathPanelView.ShowView();

            friendlyText.SetActive(false);

            editJudgeBtn.interactable = true;
            editHalfNoteBtn.interactable = true;
            editHoldPathBtn.interactable = false;
        }
        else
        {
            Popup.ShowMessage("现在编辑中的note不是hold，别点了", Color.red);
        }
    }

    #endregion

    #region Private Method

    private void OnShowPanelButton()
    {
        var image = showPanelButton.GetComponent<Image>();
        if (image.sprite == rightArrow)
        {
            Debug.Log("On show");
            // 显示面板
            image.sprite = leftArrow;
            menu.SetActive(true);
            animator.SetTrigger("cutIn");
            isShowing = true;
        }
        else
        {
            Debug.Log("On hide");
            // 收回面板
            image.sprite = rightArrow;
            menu.SetActive(false);
            animator.SetTrigger("cutOut");
            isShowing = false;
        }
    }

    private void OnEditJudgeNote(InGameJudgeNoteInfo judgeNoteInfo)
    {
        // 显示面板
        if (!isShowing)
        {
            showPanelButton.onClick.Invoke();
        }
        editJudgeBtn.onClick.Invoke();
        judgeNotePanelView.LoadJudgeData(judgeNoteInfo);
    }

    #endregion
}
