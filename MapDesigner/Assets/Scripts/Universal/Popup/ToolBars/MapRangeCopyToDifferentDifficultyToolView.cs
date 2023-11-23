using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapRangeCopyToDifferentDifficultyToolView : BaseToolBarView
{
    #region 跨难度谱面复制

    [Header("跨难度谱面复制")]
    [SerializeField] private InputField copyRangeStartInput;
    [SerializeField] private InputField copyRangeEndInput;
    [SerializeField] private Dropdown difficultySelection;

    #endregion

    public override void Initialize(Action onOk = null)
    {
        base.Initialize(onOk);

        difficultySelection.options.Clear();
        var difficultyNames = new List<string>();
        foreach (Difficulty difficulty in Enum.GetValues(typeof(Difficulty)))
        {
            difficultyNames.Add(difficulty.ToString());
        }

        difficultySelection.AddOptions(difficultyNames);

        if(EditingData.SectionSelected1 && EditingData.SectionSelected2){
            copyRangeStartInput.text = ((int)(EditingData.SectionTime1+0.5f)).ToString();
            copyRangeEndInput.text = ((int)(EditingData.SectionTime2+0.5f)).ToString();
        }
    }

    public override void OnOk()
    {
        var start = Tools.TransStringToInt(copyRangeStartInput.text);
        var end = Tools.TransStringToInt(copyRangeEndInput.text);
        var targetDifficulty = (Difficulty)difficultySelection.value;

        var command = new CopyToDifferentDifficultyCommand(start, end, targetDifficulty);
        command.Execute();
    }
}
