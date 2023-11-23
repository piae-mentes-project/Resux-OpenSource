using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PresetInfoPopupView : BasePopupView
{
    #region properties

    [SerializeField] private Button okButton;

    [SerializeField] private InputField nameInputField;

    [SerializeField] private Dropdown positionSelection;

    [SerializeField, Tooltip("当该位置已有预置时显示警告")] private Sprite warnIcon;

    private EditNoteInfo editNoteInfo;

    #endregion

    #region public Method

    public override void Initialize()
    {
        base.Initialize();
        okButton.onClick.AddListener(OnOkButton);

        positionSelection.ClearOptions();
        var presets = HalfNotePresetManager.Instance.HalfNotePresets;
        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
        for (int i = 0; i < presets.Length; i++)
        {
            Dropdown.OptionData option;
            var preset = presets[i];
            if (preset != null)
            {
                option = new Dropdown.OptionData($"{i}-{preset.presetName}", warnIcon);
            }
            else
            {
                option = new Dropdown.OptionData(i.ToString());
            }
            options.Add(option);
        }

        positionSelection.AddOptions(options);
    }

    public void Initialize(EditNoteInfo editNoteInfo)
    {
        this.editNoteInfo = editNoteInfo;
    }

    #endregion

    #region private Method

    private void OnOkButton()
    {
        var presetName = nameInputField.text;
        var presetIndex = positionSelection.value;

        HalfNotePresetInfo halfNotePresetInfo = new HalfNotePresetInfo(editNoteInfo)
        {
            presetName = presetName
        };

        var command = new AddPresetHalfNoteCommand(presetIndex, halfNotePresetInfo);
        command.Execute();

        Close();
    }

    #endregion
}
