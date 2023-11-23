using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BpmPanelView : MonoBehaviour
{
    #region Properties

    /// <summary>节拍输入</summary>
    [SerializeField] private InputField beatInput;
    /// <summary>小节输入</summary>
    [SerializeField] private InputField measureInput;
    /// <summary>分音</summary>
    [SerializeField] private InputField partialInput;
    /// <summary>BPM输入</summary>
    [SerializeField] private InputField bpmInput;
    [SerializeField] private Button addBpmButton;
    [SerializeField] private Button editBpmButton;
    [SerializeField] private RectTransform bpmShowingArea;

    /// <summary>bpm属性预制体</summary>
    private GameObject bpmPropertyPrefab;
    /// <summary>bpm数据列表</summary>
    private List<BPMInfoView> bpmInfos;
    private List<(Beat beat, float bpm)> bpmInfoList => MapDesignerSettings.BpmList;

    private BPMInfoView editingBpmInfoView;
    private int editingBpmIndex;

    #endregion

    #region Public Method

    public void Initialize()
    {
        bpmPropertyPrefab = Resources.Load<GameObject>("BPMInfoPrefab");
        bpmInfos = new List<BPMInfoView>();
        addBpmButton.onClick.AddListener(OnAddBPMButton);
        editBpmButton.onClick.AddListener(OnEditBpmButton);
    }

    /// <summary>
    /// 重置面板
    /// </summary>
    public void ResetView()
    {
        ResetBpmInput();
        RefreshBpmViews();
    }

    #endregion

    #region Private Method

    private void OnAddBPMButton()
    {
        if (EditingData.IsOpenMusic)
        {
            // 首先要查找是否已存在此beat，已存在的话就是修改了
            var info = GetBpmInfoFromInput();
            for (int i = 0; i < bpmInfos.Count; i++)
            {
                BPMInfoView bpmInfo = bpmInfos[i];
                if (bpmInfo.info.beat == info.beat)
                {
                    Popup.ShowMessage($"已存在一个{info.beat}的数据，是否要覆盖？", Color.yellow, true,
                        onOk: () =>
                        {
                            var command = new EditBPMCommand(
                                bpmInfoList[i], info,
                                bpmInfo.time,
                                bpmInfo, i);
                            command.Execute();
                            // bpmInfoList[i] = info;
                            // bpmInfo.Initialize(info, Tools.TransBeatToTime(info.beat, bpmInfoList));
                        });
                    return;
                }
            }

            var command = new AddBPMCommand(AddBpm, info, DeleteBpm);
            command.Execute();
        }
        else
        {
            Popup.ShowMessage("请先添加音乐", new Color(0.6f, 0, 0));
        }
    }

    /// <summary>
    /// 重置BPM输入
    /// </summary>
    /// <param name="beat">节拍</param>
    /// <param name="measure">小节</param>
    /// <param name="partial">分音</param>
    /// <param name="currentBpm">当前bpm</param>
    private void ResetBpmInput(string beat = "", string measure = "", string partial = "", string currentBpm = "")
    {
        beatInput.text = beat;
        measureInput.text = measure;
        partialInput.text = partial;
        bpmInput.text = currentBpm;
    }

    private void DeleteBpm(BPMInfoView infoView, (Beat beat, float bpm) bpmInfo)
    {
        var index = bpmInfoList.FindIndex(info => info.beat == bpmInfo.beat);
        if (index >= 0 && index < bpmInfoList.Count)
        {
            bpmInfoList.RemoveAt(index);
            RefreshBpmViews();
        }
        else
        {
            Popup.ShowMessage("没找到这个BPM信息", Color.red);
        }
    }

    private void AddBpm((Beat beat, float bpm) bpmInfo)
    {
        bpmInfoList.Add(bpmInfo);

        SortBPM();
        RefreshBpmViews();
    }

    private void EditBpm(BPMInfoView infoView, (Beat beat, float bpm) bpmInfo)
    {
        addBpmButton.gameObject.SetActive(false);
        editBpmButton.gameObject.SetActive(true);
        editingBpmInfoView = infoView;
        editingBpmIndex = bpmInfoList.FindIndex(info => info.beat == bpmInfo.beat);
        ResetBpmInput(bpmInfo.beat.IntPart.ToString(), bpmInfo.beat.UpPart.ToString(), bpmInfo.beat.DownPart.ToString(), bpmInfo.bpm.ToString());
    }

    private void OnEditBpmButton()
    {
        var info = GetBpmInfoFromInput();
        var command = new EditBPMCommand(
            bpmInfoList[editingBpmIndex], info,
            editingBpmInfoView.time,
            editingBpmInfoView, editingBpmIndex);
        command.Execute();
        // bpmInfoList[editingBpmIndex] = info;
        // editingBpmInfoView.Initialize(info, Tools.TransBeatToTime(info.beat, bpmInfoList));
        addBpmButton.gameObject.SetActive(true);
        editBpmButton.gameObject.SetActive(false);
    }

    private (Beat beat, float bpm) GetBpmInfoFromInput()
    {
        var beatValue = Tools.TransStringToInt(beatInput.text, 0);
        var measureValue = Tools.TransStringToInt(measureInput.text, 0);
        var partialValue = Tools.TransStringToInt(partialInput.text, 2);
        Beat beat = new Beat(beatValue, measureValue, partialValue);
        return (beat, Tools.TransStringToFloat(bpmInput.text, 120));
    }

    private void RefreshBpmViews()
    {
        var deltaCount = bpmInfoList.Count - bpmInfos.Count;
        if (deltaCount > 0)
        {
            for (int i = 0; i < deltaCount; i++)
            {
                var bpmInfoView = Instantiate(bpmPropertyPrefab, bpmShowingArea).GetComponent<BPMInfoView>();
                bpmInfos.Add(bpmInfoView);
            }
        }
        else if (deltaCount < 0)
        {
            bpmInfos.GetRange(0, -deltaCount).ForEach(view => Destroy(view.gameObject));
            bpmInfos.RemoveRange(0, -deltaCount);
        }

        for (int i = 0; i < bpmInfoList.Count; i++)
        {
            var info = bpmInfoList[i];
            var bpmInfoView = bpmInfos[i];
            bpmInfoView.Reset();
            var time = Tools.TransBeatToTime(info.beat, bpmInfoList);
            bpmInfoView.Initialize(info, time);
            bpmInfoView.AddEditButtonClickListener(() =>
            {
                EditBpm(bpmInfoView, info);
            });
            bpmInfoView.AddDeleteButtonClickListener(() =>
            {
                var command = new DeleteBPMCommand(info, time, bpmInfoView, DeleteBpm, AddBpm);
                command.Execute();
            });
        }
    }

    private void SortBPM()
    {
        // 都排序是因为这俩的索引是一一对应的
        MapDesignerSettings.SortBPM();
        bpmInfos.Sort((left, right) =>
        {
            return left.info.beat.CompareTo(right.info.beat);
        });
    }

    #endregion
}
