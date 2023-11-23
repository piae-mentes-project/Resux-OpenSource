using System;
using System.Collections.Generic;

/// <summary>
/// 编辑预置半note整体配置命令
/// </summary>
public class EditPresetHalfNoteConfigCommand : BaseCommand
{
    #region properties

    public override string Description => $"编辑预置配置";

    private string[] oldPresetHalfInfoNames;
    private HalfNotePresetGroupInfo[] oldPresetGroupInfos;
    private string[] newPresetHalfInfoNames;
    private HalfNotePresetGroupInfo[] newPresetGroupInfos;

    #endregion

    public EditPresetHalfNoteConfigCommand(string[] presetHalfInfoNames, HalfNotePresetGroupInfo[] presetTempGroupInfos)
    {
        var presetHalfInfos = HalfNotePresetManager.Instance.HalfNotePresets;
        var presetGroupInfos = HalfNotePresetManager.Instance.HalfNoteGroupPresets;

        this.newPresetHalfInfoNames = presetHalfInfoNames;
        this.newPresetGroupInfos = presetTempGroupInfos;

        this.oldPresetGroupInfos = presetGroupInfos;
        this.oldPresetHalfInfoNames = new string[presetHalfInfoNames.Length];
        for (int i = 0; i < oldPresetHalfInfoNames.Length; i++)
        {
            var halfInfo = presetHalfInfos[i];
            if (halfInfo == null)
            {
                continue;
            }
            oldPresetHalfInfoNames[i] = halfInfo.presetName;
        }
    }

    public override void Execute()
    {
        base.Execute();

        var presetHalfInfos = HalfNotePresetManager.Instance.HalfNotePresets;
        var presetGroupInfos = HalfNotePresetManager.Instance.HalfNoteGroupPresets;

        for (int i = 0; i < newPresetHalfInfoNames.Length; i++)
        {
            var halfInfo = presetHalfInfos[i];
            if (halfInfo == null)
            {
                continue;
            }
            halfInfo.presetName = newPresetHalfInfoNames[i];
        }

        for (int i = 0; i < newPresetGroupInfos.Length; i++)
        {
            var groupInfo = newPresetGroupInfos[i];
            if (presetGroupInfos[i] == null || groupInfo == null)
            {
                presetGroupInfos[i] = groupInfo;
            }
            else
            {
                presetGroupInfos[i].upHalfIndex = groupInfo.upHalfIndex;
                presetGroupInfos[i].downHalfIndex = groupInfo.downHalfIndex;
            }
        }

        HalfNotePresetManager.Instance.SavePresetData();
    }

    public override void Undo()
    {
        base.Undo();

        var presetHalfInfos = HalfNotePresetManager.Instance.HalfNotePresets;
        var presetGroupInfos = HalfNotePresetManager.Instance.HalfNoteGroupPresets;

        for (int i = 0; i < oldPresetHalfInfoNames.Length; i++)
        {
            var halfInfo = presetHalfInfos[i];
            if (halfInfo == null)
            {
                continue;
            }
            halfInfo.presetName = oldPresetHalfInfoNames[i];
        }

        for (int i = 0; i < oldPresetGroupInfos.Length; i++)
        {
            var groupInfo = oldPresetGroupInfos[i];
            if (presetGroupInfos[i] == null || groupInfo == null)
            {
                presetGroupInfos[i] = groupInfo;
            }
            else
            {
                presetGroupInfos[i].upHalfIndex = groupInfo.upHalfIndex;
                presetGroupInfos[i].downHalfIndex = groupInfo.downHalfIndex;
            }
        }

        HalfNotePresetManager.Instance.SavePresetData();
    }
}
