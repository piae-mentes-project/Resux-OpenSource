using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BPMInfoView : MonoBehaviour
{
    public (Beat beat, float bpm) info { get; private set; }
    public float time { get; private set; }

    /// <summary>节拍文本</summary>
    [SerializeField] private Text BeatText;
    /// <summary>BPM文本</summary>
    [SerializeField] private Text BPMText;
    /// <summary>时间文本</summary>
    [SerializeField] private Text TimeText;
    /// <summary>编辑按钮</summary>
    [SerializeField] private Button EditButton;
    /// <summary>删除按钮</summary>
    [SerializeField] private Button DeleteButton;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="beat">节拍</param>
    /// <param name="BPM"></param>
    /// <param name="time">时间</param>
    public void Initialize((Beat beat, float bpm) info, float time)
    {
        this.info = info;
        this.time = time;
        BeatText.text = info.beat.ToString();
        BPMText.text = $"{info.bpm}";
        TimeText.text = Tools.TimeFormat(time / 1000f);
    }

    public void AddDeleteButtonClickListener(UnityAction onDeleteClick)
    {
        DeleteButton.onClick.AddListener(onDeleteClick);
    }

    public void AddEditButtonClickListener(UnityAction onEditClick)
    {
        EditButton.onClick.AddListener(onEditClick);
    }

    public void Reset()
    {
        info = default;
        BeatText.text = BPMText.text = TimeText.text = "";
        DeleteButton.onClick.RemoveAllListeners();
        EditButton.onClick.RemoveAllListeners();
    }
}
