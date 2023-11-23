using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PresetHalfNotePopupView : BasePopupView
{
    #region inner class

    [System.Serializable]
    public class PresetGroupSettingInfo
    {
        public InputField upHalf;
        public InputField downHalf;
    }

    #endregion

    #region properties

    [SerializeField] private Button okButton;
    [SerializeField] private PresetGroupSettingInfo[] presetGroups;
    [SerializeField] private InputField[] presetHalfs;

    #endregion

    #region Public Method

    public override void Initialize()
    {
        base.Initialize();

        okButton.onClick.AddListener(OnOkButton);

        var presetHalfInfos = HalfNotePresetManager.Instance.HalfNotePresets;
        var presetGroupInfos = HalfNotePresetManager.Instance.HalfNoteGroupPresets;

        for (int i = 0; i < presetHalfs.Length; i++)
        {
            var halfInfo = presetHalfInfos[i];
            presetHalfs[i].text = halfInfo == null ? "-" : halfInfo.presetName;
        }

        for (int i = 0; i < presetGroups.Length; i++)
        {
            var groupInfo = presetGroupInfos[i];
            if (groupInfo == null)
            {
                continue;
            }
            var groupUI = presetGroups[i];
            groupUI.upHalf.text = groupInfo.upHalfIndex.ToString();
            groupUI.downHalf.text = groupInfo.downHalfIndex.ToString();
        }
    }

    #endregion

    #region Private Method

    private void OnOkButton()
    {
        var presetHalfInfoNames = new string[presetHalfs.Length];
        var presetTempGroupInfos = new HalfNotePresetGroupInfo[presetGroups.Length];

        for (int i = 0; i < presetHalfs.Length; i++)
        {
            presetHalfInfoNames[i] = presetHalfs[i].text;
        }

        for (int i = 0; i < presetGroups.Length; i++)
        {
            var groupUI = presetGroups[i];
            if (int.TryParse(groupUI.upHalf.text, out var upRes) && int.TryParse(groupUI.downHalf.text, out var downRes))
            {
                presetTempGroupInfos[i] = new HalfNotePresetGroupInfo(upRes, downRes);
            }
            else
            {
                presetTempGroupInfos[i] = null;
            }
        }

        var command = new EditPresetHalfNoteConfigCommand(presetHalfInfoNames, presetTempGroupInfos);
        command.Execute();

        Close();
    }

    #endregion
}
