using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ButtonRadio : MonoBehaviour
{
    /// <summary>按钮组</summary>
    public List<Button> ButtonGroup = new List<Button>();

    public void Initialize()
    {
        for (int i = 0; i < ButtonGroup.Count; i++)
        {
            var j = i;
            ButtonGroup[i].onClick.AddListener(() =>
            {
                SelectClickButton(j);
            });
        }
    }

    /// <summary>
    /// 点击按钮高亮
    /// </summary>
    /// <param name="index"></param>
    public void SelectClickButton(int index)
    {
        for (int i = 0; i < ButtonGroup.Count; ++i)
        {
            ButtonGroup[i].interactable = (i != index);
        }
    }

    /// <summary>
    /// 获取当前选择项
    /// </summary>
    /// <returns></returns>
    public int GetCurrentSelection()
    {
        for (int i = 0; i < ButtonGroup.Count; ++i)
        {
            if (!ButtonGroup[i].interactable)
            {
                return i;
            }
        }
        return -1;
    }

    public void AddAllButtonClickListener(UnityAction<int> onClick)
    {
        for (var i = 0; i < ButtonGroup.Count; i++)
        {
            var j = i;
            ButtonGroup[i].onClick.AddListener((() => onClick(j)));
        }
    }

    public void ResetRadio()
    {
        SelectClickButton(0);
    }
}
