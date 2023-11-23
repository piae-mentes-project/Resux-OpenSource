using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolsPanelView : BaseView
{
    #region Properties

    [SerializeField] private InputField globalDelayInput;
    [SerializeField] private ButtonRadio difficultyButtonRadio;
    [SerializeField] private ButtonRadio measureButtonRadio;
    [SerializeField] private InputField difficultyLevelInput;

    [SerializeField] private Button openPresetButton;

    #endregion

    #region Public Method

    public override void Initialize()
    {
        globalDelayInput.text = MapDesignerSettings.Delay.ToString();
        globalDelayInput.onEndEdit.AddListener(OnDelayInputEnd);

        EditingData.AddDifficultyChangeListener(OnDifficultyChanged);

        difficultyButtonRadio.Initialize();
        difficultyButtonRadio.AddAllButtonClickListener(index =>
        {
            EditingData.CurrentMapDifficulty = (Difficulty)index;
            difficultyLevelInput.text = EditingData.CurrentEditingMap.diffLevel.ToString();
            Debug.Log($"current difficulty: {EditingData.CurrentMapDifficulty}");
        });
        difficultyButtonRadio.ResetRadio();

        difficultyLevelInput.onValueChanged.AddListener(text =>
        {
            var level = int.Parse(text);
            level = Mathf.Clamp(level, ConstData.MinLevel, ConstData.MaxLevel);
            EditingData.CurrentEditingMap.diffLevel = level;
        });

        measureButtonRadio.Initialize();
        measureButtonRadio.AddAllButtonClickListener(index =>
        {
            EditingData.PartialIndexForEdit = index;
            EditingData.PartialCountForEdit = ConstData.BeatList[index];
        });
        measureButtonRadio.ResetRadio();

        openPresetButton.onClick.AddListener(OnOpenPresetView);
    }

    /// <summary>
    /// ÷ÿ÷√√Ê∞Â
    /// </summary>
    public override void ResetView()
    {
        // difficultyButtonRadio.ResetRadio();
        difficultyButtonRadio.SelectClickButton((int) EditingData.CurrentMapDifficulty);
        difficultyLevelInput.text = EditingData.CurrentEditingMap.diffLevel.ToString();
        globalDelayInput.text = MapDesignerSettings.Delay.ToString();
        measureButtonRadio.SelectClickButton(EditingData.PartialIndexForEdit);
    }

    #endregion

    #region Private Method

    private void OnDelayInputEnd(string input)
    {
        if (EditingData.IsOpenMusic)
        {
            MapDesignerSettings.Delay = Tools.TransStringToInt(input, 0);
        }
    }

    private void OnDifficultyChanged(Difficulty difficulty)
    {
        EditingData.CurrentEditingMap = EditingData.EditingMapDic[difficulty];
    }

    private void OnOpenPresetView()
    {
        Popup.ShowSpecialWindow<PresetHalfNotePopupView>("PresetHalfNotePopupView");
    }

    #endregion
}
