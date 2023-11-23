using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HalfNotePresetManager : MonoBehaviour
{
    public static HalfNotePresetManager Instance;

    #region properties

    public HalfNotePresetInfo[] HalfNotePresets { get; private set; }
    public HalfNotePresetGroupInfo[] HalfNoteGroupPresets { get; private set; }

    /// <summary>Ԥ�õ������ļ��洢·��</summary>
    private string Path => $"{GlobalSettings.MapDataPath}/PresetData.json";

    #endregion

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Initialize();
    }

    #region Public Method

    public void AddHalfNotePreset(HalfNotePresetInfo halfNotePresetInfo, int index)
    {
        HalfNotePresets[index] = halfNotePresetInfo;

        SavePresetData();
    }

    /// <summary>
    /// ��ָ��λ�� ���/���� ָ����һ��Ԥ��
    /// </summary>
    /// <param name="index">Ԥ���������</param>
    /// <param name="index1">Ԥ��1������</param>
    /// <param name="index2">Ԥ��2������</param>
    public void AddPresetGroup(int index, int index1, int index2)
    {
        HalfNoteGroupPresets[index] = new HalfNotePresetGroupInfo(index1, index2);

        SavePresetData();
    }

    public void SavePresetData()
    {
        var presetData = new PresetStructer()
        {
            halfNotePresets = HalfNotePresets,
            halfNoteGroupPresets = HalfNoteGroupPresets
        };

        FileUtils.SaveJsonFile(Path, presetData);
    }

    #endregion

    #region Private Method

    private void Initialize()
    {
        // ��ʼ��Ԥ������
        var presetData = FileUtils.GetDataFromJsonFile<PresetStructer>(Path);
        if (presetData == null)
        {
            HalfNotePresets = new HalfNotePresetInfo[10];
            HalfNoteGroupPresets = new HalfNotePresetGroupInfo[10];
        }
        else
        {
            HalfNotePresets = presetData.halfNotePresets;
            HalfNoteGroupPresets = presetData.halfNoteGroupPresets;
        }

        PlayerInputManager.Instance.OnLeftCtrlAndNumKeyDown += OnPressNumberKey;
    }

    private void OnPressNumberKey(int keyCode)
    {
        Debug.Log($"Left Ctrl + Alpha {keyCode} (Number)");
        HalfNotePresetGroupInfo halfNotePresetGroupInfo;
        if (keyCode >= 0 && keyCode < 10)
        {
            halfNotePresetGroupInfo = HalfNoteGroupPresets[keyCode];
        }
        else
        {
            return;
        }

        if (halfNotePresetGroupInfo == null)
        {
            return;
        }

        HalfNotePresetInfo upHalfPreset = HalfNotePresets[halfNotePresetGroupInfo.upHalfIndex];
        HalfNotePresetInfo downHalfPreset = HalfNotePresets[halfNotePresetGroupInfo.downHalfIndex];

        if (upHalfPreset == null || downHalfPreset == null)
        {
            Popup.ShowMessage($"Ԥ�õ�һ���note����ȫ����index: {keyCode}", Color.red);
            return;
        }

        var editingJudge = EditingData.CurrentEditingJudgeNote;
        var moveEndTime = editingJudge.judge.judgeTime;
        // �ϰ�Ԥ��
        var upHalfNote = new EditNoteInfo(upHalfPreset.weightType, HalfType.UpHalf,
            new NoteMoveInfo(upHalfPreset.v0, upHalfPreset.GetP0(editingJudge.judge.judgePosition), startTime: moveEndTime - upHalfPreset.moveTime, endTime: moveEndTime, upHalfPreset.isReverse),
            new List<Vector2>()
            );
        upHalfNote.CalculatePath();
        editingJudge.movingNotes.up = upHalfNote;
        // �°�Ԥ��
        var downHalfNote = new EditNoteInfo(downHalfPreset.weightType, HalfType.DownHalf,
            new NoteMoveInfo(downHalfPreset.v0, downHalfPreset.GetP0(editingJudge.judge.judgePosition), startTime: moveEndTime - downHalfPreset.moveTime, endTime: moveEndTime, downHalfPreset.isReverse),
            new List<Vector2>()
            );
        downHalfNote.CalculatePath();
        editingJudge.movingNotes.down = downHalfNote;
    }

    #endregion
}
