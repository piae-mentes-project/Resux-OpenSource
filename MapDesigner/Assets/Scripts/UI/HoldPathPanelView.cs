using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HoldPathPanelView : BaseView
{
    #region Properties

    [SerializeField] private InputField holdPosXInput;
    [SerializeField] private InputField holdPosYInput;
    [SerializeField] private InputField holdJudgeTimeInput;
    [SerializeField] private Button addHoldPathButton;
    [SerializeField] private Button editJudgePointButton;
    /// <summary>Hold路径显示列表</summary>
    [SerializeField] private Transform holdPathShowingArea;
    /// <summary>模式选择</summary>
    [SerializeField] private ButtonRadio modeRadio;

    [Space]
    [Tooltip("路径的编辑模式选择")]
    [SerializeField] private Dropdown modeSelection;
    [SerializeField] private GameObject angleLabel;
    [SerializeField] private Slider angleSlider;
    [SerializeField] private UIEventTrigger angleSliderHandleTrigger;
    [SerializeField] private GameObject valueLabel;
    [SerializeField] private Slider valueSlider;
    [SerializeField] private UIEventTrigger valueSliderHandleTrigger;
    [SerializeField] private Button xCurveEditButton;
    [SerializeField] private Button yCurveEditButton;

    private List<HoldPreviewInfoView> holdPathViews;

    /// <summary>Hold属性预制体</summary>
    private GameObject holdPropertyPrefab;

    private InGameJudgeNoteInfo editingJudgeNote;
    private HoldPreviewInfoView editingView;
    private JudgePair editingJudgePair;
    private int editingIndex;
    private (float angle, float value) lastParam;

    #endregion

    #region Unity

    private void OnDisable()
    {
        
    }

    #endregion

    #region Public Method

    public override void Initialize()
    {
        holdPathViews = new List<HoldPreviewInfoView>();

        holdPropertyPrefab = Resources.Load<GameObject>("HoldPathPrefab");
        addHoldPathButton.onClick.AddListener(OnAddHoldPathButtonClick);
        editJudgePointButton.onClick.AddListener(OnEditJudgePointButton);

        modeRadio.Initialize();
        modeRadio.ResetRadio();
        modeRadio.AddAllButtonClickListener(index =>
        {
            // 选择Hold的编辑模式
            EditingData.CurrentEditStep = (EditPanelView.EditStep)(index + 2);
            Debug.Log($"mode: {EditingData.CurrentEditStep}");
            if (EditingData.CurrentEditStep == EditPanelView.EditStep.HoldPath)
            {
                HoldPathEditManager.Instance.RefreshNoteHoldPath();
            }
        });

        // 路径类型
        var types = System.Enum.GetValues(typeof(HoldPathType));
        var options = new List<Dropdown.OptionData>(types.Length);
        foreach (HoldPathType type in types)
        {
            options.Add(new Dropdown.OptionData(type.GetName()));
        }
        modeSelection.options = options;
        modeSelection.onValueChanged.AddListener(index =>
        {
            HoldPathType type = (HoldPathType) index;
            var command = new EditHoldJudgeNotePathTypeCommand(EditingData.CurrentEditingJudgeNote, type,
                type =>
                {
                    UpdateModeView(type);
                });
            command.Execute();
        });
        // 贝塞尔曲线ui
        angleSlider.minValue = ConstData.AngleRange.min;
        angleSlider.maxValue = ConstData.AngleRange.max;
        angleSlider.onValueChanged.AddListener(value =>
        {
            if (EditingData.CurrentEditStep != EditPanelView.EditStep.HoldPath || EditingData.CurrentEditingJudgeNote.judges.Count < 2)
            {
                return;
            }

            var index = EditingData.CurrentEditingHoldJudgeIndex;
            var tempParam = lastParam;
            tempParam.angle = value;
            EditingData.CurrentEditingJudgeNote.pathParameters[index] = tempParam;
            HoldPathEditManager.Instance.RefreshNoteHoldPath();
        });
        angleSliderHandleTrigger.onPointerDown.AddListener(eventData =>
        {
            var index = EditingData.CurrentEditingHoldJudgeIndex;
            lastParam = EditingData.CurrentEditingJudgeNote.pathParameters[index];
        });
        angleSliderHandleTrigger.onPointerUp.AddListener(eventData =>
        {
            var newParam = (angleSlider.value, lastParam.value);
            var command = new EditHoldJudgeParameterCommand(EditingData.CurrentEditingJudgeNote, lastParam, newParam, EditingData.CurrentEditingHoldJudgeIndex,
                param =>
                {
                    lastParam.angle = param.angle;
                    angleSlider.value = param.angle;
                });
            command.Execute();
        });
        valueSlider.minValue = ConstData.ValueRange.min;
        valueSlider.maxValue = ConstData.ValueRange.max;
        valueSlider.onValueChanged.AddListener(value =>
        {
            if (EditingData.CurrentEditStep != EditPanelView.EditStep.HoldPath || EditingData.CurrentEditingJudgeNote.judges.Count < 2)
            {
                return;
            }

            var index = EditingData.CurrentEditingHoldJudgeIndex;
            var tempParam = lastParam;
            tempParam.value = value;
            EditingData.CurrentEditingJudgeNote.pathParameters[index] = tempParam;
            HoldPathEditManager.Instance.RefreshNoteHoldPath();
        });
        valueSliderHandleTrigger.onPointerDown.AddListener(eventData =>
        {
            var index = EditingData.CurrentEditingHoldJudgeIndex;
            lastParam = EditingData.CurrentEditingJudgeNote.pathParameters[index];
        });
        valueSliderHandleTrigger.onPointerUp.AddListener(eventData =>
        {
            var newParam = (lastParam.angle, valueSlider.value);
            var command = new EditHoldJudgeParameterCommand(EditingData.CurrentEditingJudgeNote, lastParam, newParam, EditingData.CurrentEditingHoldJudgeIndex,
                param =>
                {
                    lastParam.value = param.value;
                    valueSlider.value = param.value;
                });
            command.Execute();
        });
        // 自定义曲线
        xCurveEditButton.onClick.AddListener(() =>
        {
            var index = EditingData.CurrentEditingHoldJudgeIndex;
            if (index >= EditingData.CurrentEditingJudgeNote.HoldCurves.Count)
            {
                return;
            }

            var xCurve = EditingData.CurrentEditingJudgeNote.HoldCurves[index].xCurve;
            RTAnimationCurveEditManager.Instance.ShowEditorView(xCurve);
        });
        yCurveEditButton.onClick.AddListener(() =>
        {
            var index = EditingData.CurrentEditingHoldJudgeIndex;
            if (index >= EditingData.CurrentEditingJudgeNote.HoldCurves.Count)
            {
                return;
            }

            var yCurve = EditingData.CurrentEditingJudgeNote.HoldCurves[index].yCurve;
            RTAnimationCurveEditManager.Instance.ShowEditorView(yCurve);
        });
    }

    public override void UpdateView()
    {
        if (EditingData.CurrentEditingJudgeNote == null)
        {
            Popup.ShowMessage("没有正在编辑的判定", Color.red);
        }
        else if (EditingData.CurrentEditingJudgeNote.judgeType == JudgeType.Hold)
        {
            RefreshHoldPathList();
            // 有在编辑状态的hold判定点
            var index = EditingData.CurrentEditingHoldJudgeIndex;
            // 说明不是这个hold的索引，或者没初始化索引
            if (index < 0 || index >= EditingData.CurrentEditingJudgeNote.judges.Count)
            {
                EditingData.CurrentEditingHoldJudgeIndex = index = 0;
            }
            // 判定点信息
            editingJudgePair = EditingData.CurrentEditingJudgeNote.judges[index];
            editingView = holdPathViews[index];
            SetJudgeDataToInput(editingJudgePair);
            // 判定点的路径参数信息
            modeSelection.value = (int) EditingData.CurrentEditingJudgeNote.pathType;
            var param = EditingData.CurrentEditingJudgeNote.pathParameters[index];
            lastParam = param;
            angleSlider.value = param.angle;
            valueSlider.value = param.value;

            // 初始化显示的UI
            UpdateModeView(EditingData.CurrentEditingJudgeNote.pathType);

            if (EditingData.CurrentEditingJudgeNote.judges.Count >= 2)
            {
                HoldPathEditManager.Instance.RefreshNoteHoldPath();
            }
        }
        else
        {
            Popup.ShowMessage("正在编辑的不是Hold", Color.red);
        }
    }

    #endregion

    #region Private Method

    /// <summary>
    /// 添加Hold路径按钮
    /// </summary>
    private void OnAddHoldPathButtonClick()
    {
        var judges = EditingData.CurrentEditingJudgeNote.judges;
        var judgeData = GetJudgeDataFromInput();
        if ((!float.IsNaN(judgeData.judgePosition.x)) && (!float.IsNaN(judgeData.judgePosition.y)) && judgeData.judgeTime > 0)
        {
            if (judges.Any(judge => judge.Equals(judgeData)))
            {
                Popup.ShowMessage("已存在一个同样的判定点", Color.red);
            }
            else
            {
                // 自动排序
                var command = new AddHoldJudgeCommand(EditingData.CurrentEditingJudgeNote, judgeData, RefreshHoldPathList);
                command.Execute();
            }
        }
    }

    private void OnEditJudgePointButton()
    {
        var judge = GetJudgeDataFromInput();
        var command = new EditHoldJudgeCommand(editingJudgeNote, editingIndex, judge, editingView.Initialize);
        command.Execute();

        addHoldPathButton.gameObject.SetActive(true);
        editJudgePointButton.gameObject.SetActive(false);
    }

    /// <summary>
    /// 刷新Hold路径表
    /// </summary>
    private void RefreshHoldPathList()
    {
        var judges = EditingData.CurrentEditingJudgeNote.judges;
        var deltaCount = judges.Count - holdPathViews.Count;
        if (deltaCount > 0)
        {
            for (int i = 0; i < deltaCount; i++)
            {
                var view = Instantiate(holdPropertyPrefab, holdPathShowingArea)
                    .GetComponent<HoldPreviewInfoView>();
                view.AddEditButtonListener(OnEditJudgePoint);
                view.AddDeleteButtonListener(OnDeleteJudgePoint);
                holdPathViews.Add(view);
            }
        }
        else if (deltaCount < 0)
        {
            holdPathViews.GetRange(0, -deltaCount).ForEach(hold => Destroy(hold.gameObject));
            holdPathViews.RemoveRange(0, -deltaCount);
        }

        for (int i = 0; i < judges.Count; i++)
        {
            var view = holdPathViews[i];
            var judgePair = judges[i];
            view.Initialize(judgePair);
            if (!view.gameObject.activeSelf)
            {
                view.gameObject.SetActive(true);
            }
        }

        GamePreviewManager.Instance.UpdatePreview();
    }

    private void OnEditJudgePoint(HoldPreviewInfoView view, JudgePair judgePair)
    {
        SetJudgeDataToInput(judgePair);
        editingView = view;
        editingJudgePair = judgePair;
        editingJudgeNote = EditingData.CurrentEditingJudgeNote;
        editingIndex = editingJudgeNote.judges.FindIndex(judge => judge.Equals(judgePair));
        EditingData.CurrentEditingHoldJudgeIndex = editingIndex;
        var param = editingJudgeNote.pathParameters[editingIndex];
        lastParam = param;
        // 曲线参数
        angleSlider.value = param.angle;
        valueSlider.value = param.value;
    }

    private void OnDeleteJudgePoint(HoldPreviewInfoView view, JudgePair judgePair)
    {
        view.gameObject.SetActive(false);
        var command = new DeleteHoldJudgePointCommand(EditingData.CurrentEditingJudgeNote, judgePair,
            () =>
            {
                RefreshHoldJudgeIndex();
                RefreshHoldPathList();
            });
        Debug.Log(command.Description);
        command.Execute();
    }

    private JudgePair GetJudgeDataFromInput()
    {
        var time = Tools.TransStringToInt(holdJudgeTimeInput.text, 0);
        var x = Tools.TransStringToFloat(holdPosXInput.text, float.NaN);
        var y = Tools.TransStringToFloat(holdPosYInput.text, float.NaN);

        return new JudgePair(new Vector2(x, y), time);
    }

    private void SetJudgeDataToInput(JudgePair judgePair)
    {
        addHoldPathButton.gameObject.SetActive(false);
        editJudgePointButton.gameObject.SetActive(true);

        holdJudgeTimeInput.text = judgePair.judgeTime.ToString();
        holdPosXInput.text = judgePair.judgePosition.x.ToString();
        holdPosYInput.text = judgePair.judgePosition.y.ToString();
    }

    /// <summary>
    /// 刷新正在编辑中的hold判定索引
    /// </summary>
    private void RefreshHoldJudgeIndex()
    {
        EditingData.CurrentEditingHoldJudgeIndex = Mathf.Min(EditingData.CurrentEditingHoldJudgeIndex, EditingData.CurrentEditingJudgeNote.judges.Count - 1);
    }

    /// <summary>
    /// 更新不同模式的编辑UI的显示状态
    /// </summary>
    /// <param name="holdPathType"></param>
    private void UpdateModeView(HoldPathType holdPathType)
    {
        switch (holdPathType)
        {
            case HoldPathType.DirectLine:
            case HoldPathType.BezierLine:
                angleLabel.SetActive(true);
                angleSlider.gameObject.SetActive(true);
                // angleSliderHandleTrigger.gameObject.SetActive(true);
                valueLabel.SetActive(true);
                valueSlider.gameObject.SetActive(true);
                // valueSliderHandleTrigger.gameObject.SetActive(true);
                xCurveEditButton.gameObject.SetActive(false);
                yCurveEditButton.gameObject.SetActive(false);
                break;
            case HoldPathType.Curve:
                angleLabel.SetActive(false);
                angleSlider.gameObject.SetActive(false);
                // angleSliderHandleTrigger.gameObject.SetActive(false);
                valueLabel.SetActive(false);
                valueSlider.gameObject.SetActive(false);
                // valueSliderHandleTrigger.gameObject.SetActive(false);
                xCurveEditButton.gameObject.SetActive(true);
                yCurveEditButton.gameObject.SetActive(true);
                break;
        }
    }

    #endregion
}
