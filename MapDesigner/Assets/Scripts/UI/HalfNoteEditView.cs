using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 半note编辑界面
/// </summary>
public class HalfNoteEditView : BaseView
{
    #region properties

    [Space]
    [SerializeField] private Text statusHintText;
    /// <summary>Note重量</summary>
    [SerializeField] private ButtonRadio noteWeightButtonRadio;
    /// <summary>是否反向</summary>
    [SerializeField] private Toggle isReverseToggle;
    [SerializeField] private InputField moveTimeInput;
    [SerializeField] private InputField initPosXInput;
    [SerializeField] private InputField initPosYInput;
    [SerializeField] private Text speedText;
    [SerializeField] private Slider xSpeedSlider;
    [SerializeField] private UIEventTrigger xSpeedSliderHandler;
    [SerializeField] private Slider ySpeedSlider;
    [SerializeField] private UIEventTrigger ySpeedSliderHandler;
    [SerializeField] private DirectionArrow arrow;
    [SerializeField] private InputField xSpeedInput;
    [SerializeField] private InputField ySpeedInput;
    [SerializeField] private GameObject speedSliderAreaObj;
    [SerializeField] private GameObject speedInputAreaObj;

    private EditNoteInfo noteInfo;
    private Vector2 lastSpeedValue;

    #endregion

    #region Public Method

    public override void Initialize()
    {
        xSpeedSlider.minValue = ConstData.HorizontalSpeedRange.min;
        xSpeedSlider.maxValue = ConstData.HorizontalSpeedRange.max;
        xSpeedSlider.onValueChanged.AddListener(value => RefreshSpeed(RectTransform.Axis.Horizontal, value));
        xSpeedSliderHandler.onPointerDown.AddListener(eventData =>
        {
            lastSpeedValue.x = xSpeedSlider.value;
        });
        xSpeedSliderHandler.onPointerUp.AddListener(eventData =>
        {
            var newV0 = lastSpeedValue;
            newV0.x = xSpeedSlider.value;
            var command = new EditHalfNoteInitSpeedCommand(lastSpeedValue, newV0, noteInfo, v0 =>
            {
                arrow.RefreshDirection(v0);
                lastSpeedValue.x = v0.x;
                xSpeedSlider.value = v0.x;
            });
            command.Execute();
        });
        xSpeedInput.onEndEdit.AddListener(str =>
        {
            var value = Tools.TransStringToFloat(str);
            if (!Tools.IsInRange(value, ConstData.HorizontalSpeedRange.min, ConstData.HorizontalSpeedRange.max))
            {
                xSpeedInput.text =
                    Mathf.Clamp(value, ConstData.HorizontalSpeedRange.min, ConstData.HorizontalSpeedRange.max)
                        .ToString();
            }
            else
            {
                RefreshSpeed(RectTransform.Axis.Horizontal, value);
            }
        });
        ySpeedSlider.minValue = ConstData.VerticalSpeedRange.min;
        ySpeedSlider.maxValue = ConstData.VerticalSpeedRange.max;
        ySpeedSlider.onValueChanged.AddListener(value => RefreshSpeed(RectTransform.Axis.Vertical, value));
        ySpeedSliderHandler.onPointerDown.AddListener(eventData =>
        {
            lastSpeedValue.y = ySpeedSlider.value;
        });
        ySpeedSliderHandler.onPointerUp.AddListener(eventData =>
        {
            var newV0 = lastSpeedValue;
            newV0.y = ySpeedSlider.value;
            var command = new EditHalfNoteInitSpeedCommand(lastSpeedValue, newV0, noteInfo, v0 =>
            {
                arrow.RefreshDirection(v0);
                lastSpeedValue.y = v0.y;
                ySpeedSlider.value = v0.y;
            });
            command.Execute();
        });
        ySpeedInput.onEndEdit.AddListener(str =>
        {
            var value = Tools.TransStringToFloat(str);
            if (!Tools.IsInRange(value, ConstData.VerticalSpeedRange.min, ConstData.VerticalSpeedRange.max))
            {
                xSpeedInput.text =
                    Mathf.Clamp(value, ConstData.VerticalSpeedRange.min, ConstData.VerticalSpeedRange.max)
                        .ToString();
            }
            else
            {
                RefreshSpeed(RectTransform.Axis.Vertical, value);
            }
        });

        initPosXInput.onEndEdit.AddListener(value => RefreshPosition(RectTransform.Axis.Horizontal, Tools.TransStringToFloat(value)));
        initPosYInput.onEndEdit.AddListener(value => RefreshPosition(RectTransform.Axis.Vertical, Tools.TransStringToFloat(value)));
        isReverseToggle.onValueChanged.AddListener(isOn =>
        {
            if (noteInfo == null) return;
            if (noteInfo.noteMoveInfo.isReverse == isOn)
            {
                return;
            }

            var command = new EditHalfNoteReverseCommand(noteInfo);
            command.Execute();
        });
        noteWeightButtonRadio.Initialize();
        noteWeightButtonRadio.AddAllButtonClickListener(value =>
        {
            if (noteInfo == null) return;
            var weight = (WeightType)value;
            var command = new EditHalfNoteWeightTypeCommand(noteInfo, weight);
            command.Execute();
        });
        moveTimeInput.onEndEdit.AddListener(value =>
        {
            if (noteInfo == null) return;
            var defaultValue = noteInfo.noteMoveInfo.endTime - noteInfo.noteMoveInfo.startTime;
            var time = Tools.TransStringToInt(value, defaultValue);
            var command = new EditHalfNoteMoveTimeCommand(noteInfo, time);
            command.Execute();
        });
    }

    public void SetHalfNote(EditNoteInfo editNoteInfo)
    {
        noteInfo = editNoteInfo;
    }

    public override void UpdateView()
    {
        if (noteInfo != null)
        {
            statusHintText.text = "状态：<color=#ff0000>编辑</color>";
            // 把上半的信息填进去
            initPosXInput.text = noteInfo.noteMoveInfo.p0.x.ToString();
            initPosYInput.text = noteInfo.noteMoveInfo.p0.y.ToString();
            xSpeedSlider.value = noteInfo.noteMoveInfo.v0.x;
            ySpeedSlider.value = noteInfo.noteMoveInfo.v0.y;
            moveTimeInput.text = (noteInfo.noteMoveInfo.endTime - noteInfo.noteMoveInfo.startTime).ToString();
            noteWeightButtonRadio.SelectClickButton((int)noteInfo.weightType);
            isReverseToggle.isOn = noteInfo.noteMoveInfo.isReverse;
            lastSpeedValue = noteInfo.noteMoveInfo.v0;
        }
        else
        {
            statusHintText.text = "状态：<color=#ff0000>创建</color>";
            // 置空
            initPosXInput.text = "";
            initPosYInput.text = "";
            xSpeedSlider.value = 0;
            ySpeedSlider.value = 0;
            moveTimeInput.text = "";
        }
    }

    public void ChangeSpeedEditMode(bool isInputMode)
    {
        speedSliderAreaObj.SetActive(!isInputMode);
        speedInputAreaObj.SetActive(isInputMode);

        if (isInputMode)
        {
            xSpeedInput.text = xSpeedSlider.value.ToString();
            ySpeedInput.text = ySpeedSlider.value.ToString();
        }
        else
        {
            xSpeedSlider.value = Tools.TransStringToFloat(xSpeedInput.text);
            ySpeedSlider.value = Tools.TransStringToFloat(ySpeedInput.text);
        }
    }

    #endregion

    #region Private Method

    private void RefreshSpeed(RectTransform.Axis axis, float value)
    {
        if (EditingData.CurrentEditingJudgeNote == null)
        {
            return;
        }

        var newV0 = new Vector2();
        if (noteInfo == null)
        {
            return;
        }

        if (RectTransform.Axis.Horizontal == axis)
        {
            var verticalValue = noteInfo.noteMoveInfo.v0.y;
            speedText.text = $"初速度({value:.00},{verticalValue:.00})";
            newV0.x = value;
            newV0.y = verticalValue;
        }
        else
        {
            var horizontalValue = noteInfo.noteMoveInfo.v0.x;
            speedText.text = $"初速度({horizontalValue:.00},{value:.00})";
            newV0.x = horizontalValue;
            newV0.y = value;
        }

        noteInfo.SetInitV0(newV0);
        arrow.RefreshDirection(newV0);
    }

    private void RefreshPosition(RectTransform.Axis axis, float value)
    {
        if (EditingData.CurrentEditingJudgeNote == null)
        {
            return;
        }

        // 坐标需要限制在范围内
        var realValue = GetPosition(axis, value);
        var newP0 = new Vector2();
        if (noteInfo == null)
        {
            return;
        }

        if (RectTransform.Axis.Horizontal == axis)
        {
            var verticalValue = noteInfo.noteMoveInfo.p0.y;
            newP0.x = realValue;
            newP0.y = verticalValue;
        }
        else
        {
            var horizontalValue = noteInfo.noteMoveInfo.p0.x;
            newP0.x = horizontalValue;
            newP0.y = realValue;
        }

        var command = new EditHalfNotePositionCommand(noteInfo, newP0, EditingData.CurrentEditingJudgeNote.judge.judgeTime);
        command.Execute();
    }

    private float GetPosition(RectTransform.Axis axis, float value)
    {
        if (RectTransform.Axis.Horizontal == axis)
        {
            return Mathf.Clamp(value, ConstData.PositionXRange.min, ConstData.PositionXRange.max);
        }
        else
        {
            return Mathf.Clamp(value, ConstData.PositionYRange.min, ConstData.PositionYRange.max);
        }
    }

    #endregion
}
