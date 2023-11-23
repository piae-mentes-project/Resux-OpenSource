using System;
using System.Collections.Generic;

/// <summary>
/// 添加预置半note命令
/// </summary>
public class AddPresetHalfNoteCommand : BaseCommand
{
    #region properties

    public override string Description => $"添加预置半note：索引 {index}";

    private int index;
    private HalfNotePresetInfo oldPresetInfo;
    private HalfNotePresetInfo newPresetInfo;

    #endregion

    public AddPresetHalfNoteCommand(int index, HalfNotePresetInfo presetInfo)
    {
        this.index = index;
        this.newPresetInfo = presetInfo;
        this.oldPresetInfo = HalfNotePresetManager.Instance.HalfNotePresets[index];
    }

    public override void Execute()
    {
        base.Execute();

        HalfNotePresetManager.Instance.AddHalfNotePreset(newPresetInfo, index);
    }

    public override void Undo()
    {
        base.Undo();

        HalfNotePresetManager.Instance.AddHalfNotePreset(oldPresetInfo, index);
    }
}
