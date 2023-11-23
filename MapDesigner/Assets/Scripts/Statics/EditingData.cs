using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using UnityEngine;

public static class EditingData
{
    #region Properties

    /// <summary>�����ڳߴ�</summary>
    public static (float width, float height) MainCanvasSize;
    /// <summary>��Ϸ���ڵ�����ֵ</summary>
    public static float GameSceneScale;
    /// <summary>��Ļ��С</summary>
    public static (float width, float height) ScreenSize;
    /// <summary>��Ϸ���ڣ������½�ԭ�㣩λ��</summary>
    public static (float x, float y) GameWindowPosition;
    /// <summary>��Ϸ���ڴ�С</summary>
    public static (float width, float height) GameWindowSize;
    /// <summary>��Ϸ������ʵ�ʵ�ԭ���ڱ༭��������ϵ�е�λ��</summary>
    public static Vector2 RealGameZeroPos => ConstData.GameCenter * (GameSceneScale - 1);
    /// <summary>�༭ʱ�ɹ���ѡ��note�İ뾶</summary>
    public static float HalfNoteEditClickRadius => ConstData.HalfNoteClickRadius * GameSceneScale * (ScreenSize.width / GameWindowSize.width) * 0.5f;
    /// <summary>�༭ʱ�ɹ���ѡ�ж���İ뾶</summary>
    public static float JudgePointEditClickRadius => ConstData.JudgePointClickRadius * GameSceneScale * (ScreenSize.width / GameWindowSize.width) * 0.5f;

    /// <summary>�Ƿ������</summary>
    public static bool IsOpenMusic;
    /// <summary>�Ƿ������</summary>
    public static bool IsOpenMap;
    /// <summary>�Ƿ�����Ϸ������</summary>
    public static bool IsInGameWindow;
    /// <summary>�Ƿ���Ա༭</summary>
    public static bool CanEditing => IsOpenMap && IsOpenMusic;
    /// <summary>��ǰ�����棩ʱ��(ms)</summary>
    public static int CurrentTime;
    public static int CurrentTimeWithoutDelay => CurrentTime - MapDesignerSettings.Delay;
    /// <summary>ѡ�����棩ʱ������</summary>
    public static float SectionTime1;
    public static float SectionTime2;
    public static bool SectionSelected1;
    public static bool SectionSelected2;
    /// <summary>�������Ŵ�С</summary>
    public static int screenSize;
    /// <summary>�Եڼ������༭</summary>
    public static int PartialIndexForEdit;

    private static int partialCountForEdit;

    /// <summary>
    /// �Լ������༭
    /// ����ϸ�֣����ڳ������ڵ�ֵ��
    /// </summary>
    public static int PartialCountForEdit
    {
        get => partialCountForEdit;
        set
        {
            if (partialCountForEdit == value)
            {
                return;
            }

            partialCountForEdit = value;
            OnPartialCountChanged?.Invoke(partialCountForEdit);
        }
    }

    private static Action<int> OnPartialCountChanged;
    /// <summary>BPM������</summary>
    public static float BpmScale;

    #region �༭������

    /// <summary>�ж���ĵ�ǰ�༭����</summary>
    public static EditPanelView.EditStep CurrentEditStep;

    private static Action<Difficulty> OnDifficultyChanged;
    private static Difficulty currentMapDifficulty;
    /// <summary>��ǰ�����Ѷ�</summary>
    public static Difficulty CurrentMapDifficulty
    {
        get => currentMapDifficulty;
        set
        {
            currentMapDifficulty = value;
            OnDifficultyChanged?.Invoke(value);
        }
    }
    /// <summary>��ǰ�༭����</summary>
    public static InGameMusicMap CurrentEditingMap;

    private static InGameJudgeNoteInfo currentEditingJudgeNote;

    /// <summary>��ǰ�༭�ж�</summary>
    public static InGameJudgeNoteInfo CurrentEditingJudgeNote
    {
        get => currentEditingJudgeNote;
        set
        {
            if (currentEditingJudgeNote == value)
            {
                return;
            }

            currentEditingJudgeNote = value;
            OnEditingJudgeChanged?.Invoke(currentEditingJudgeNote);
        }
    }

    private static Action<InGameJudgeNoteInfo> OnEditingJudgeChanged;

    /// <summary>��ǰ�༭�е�hold�ж�������</summary>
    public static int CurrentEditingHoldJudgeIndex;
    /// <summary>��ǰ�༭�а�note</summary>
    public static EditNoteInfo CurrentEditingHalfNote;


    /// <summary>����Ѷȵ�����</summary>
    public static Dictionary<Difficulty, InGameMusicMap> EditingMapDic;

    #endregion

    #region ��������

    /// <summary>��Ϸ��������ϵ�����λ��</summary>
    public static (float x, float y) MousePositionInGameArea;
    /// <summary>���ź�����λ�ö�Ӧ��ʵ����Ϸ����</summary>
    public static Vector2 MousePositionInGameAreaWithScale;
    /// <summary>����������ϵ�����λ��</summary>
    public static (float x, float y) MousePositionInMainPanel;
    /// <summary>���򴰿�����ϵ����Ϸ��������λ��</summary>
    public static (float x, float y) GameAreaLeftDownPositionInMainWindow;
    /// <summary>���򴰿�����ϵ����Ϸ��������λ��</summary>
    public static (float x, float y) GameAreaRightUpPositionInMainWindow;
    /// <summary>����������ϵ����Ϸ��������λ��</summary>
    public static (float x, float y) GameAreaLeftDownPositionInMainPanel;
    /// <summary>����������ϵ����Ϸ��������λ��</summary>
    public static (float x, float y) GameAreaRightUpPositionInMainPanel;

    #endregion

    #endregion

    static EditingData()
    {
        CurrentEditStep = EditPanelView.EditStep.Judge;
        CurrentEditingHoldJudgeIndex = -1;
        PartialIndexForEdit = 0;
        PartialCountForEdit = 2;
        BpmScale = 1;
        SectionTime1 = 0f; 
        SectionSelected1 = false;
        SectionTime2 = 0f; 
        SectionSelected2 = false;
        EditingMapDic = new Dictionary<Difficulty, InGameMusicMap>()
        {
            {Difficulty.Tale, new InGameMusicMap(Difficulty.Tale)},
            {Difficulty.Romance, new InGameMusicMap(Difficulty.Romance)},
            {Difficulty.History, new InGameMusicMap(Difficulty.History)},
            {Difficulty.Revival, new InGameMusicMap(Difficulty.Revival)},
            {Difficulty.Story, new InGameMusicMap(Difficulty.Story)}
        };
        CurrentEditingMap = EditingMapDic[Difficulty.Tale];
    }

    #region Public Method

    /// <summary>
    /// ����Ѷȱ���¼�����
    /// </summary>
    /// <param name="onDifficultyChanged"></param>
    public static void AddDifficultyChangeListener(Action<Difficulty> onDifficultyChanged)
    {
        OnDifficultyChanged += onDifficultyChanged;
    }

    public static void AddEditingJudgeNoteChangeListener(Action<InGameJudgeNoteInfo> onEditingJudgeNoteChanged)
    {
        OnEditingJudgeChanged += onEditingJudgeNoteChanged;
    }

    /// <summary>
    /// ��ӽ���ϸ�ֱ�����¼�����
    /// </summary>
    /// <param name="onPartialCountChanged"></param>
    public static void AddPartialCountForEditChangeListener(Action<int> onPartialCountChanged)
    {
        OnPartialCountChanged += onPartialCountChanged;
    }

    /// <summary>
    /// ��ȡ��Ч���ж���Ϣ
    /// <c>��Ч��ָ���ж����ϰ���°��ڵ�ǰʱ�䴦����ʾ״̬</c>
    /// </summary>
    /// <returns>�ж��б�</returns>
    public static InGameJudgeNoteInfo[] GetValidJudgeNotes()
    {
        if (CurrentEditingMap == null)
        {
            return null;
        }

        var judgeNotes = new List<InGameJudgeNoteInfo>();

        for (int i = 0; i < CurrentEditingMap.judgeNotes.Count; i++)
        {
            var judgeNote = CurrentEditingMap.judgeNotes[i];
            var upNote = judgeNote.movingNotes.up;
            var downNote = judgeNote.movingNotes.down;
            // �ϰ�note
            bool result = false;
            if (upNote != null)
            {
                result = result || Tools.IsInRange(CurrentTime, upNote.noteMoveInfo.startTime, upNote.noteMoveInfo.endTime + upNote.holdPath.Count * 10);
            }
            // �°�note
            if (downNote != null)
            {
                result = result || Tools.IsInRange(CurrentTime, downNote.noteMoveInfo.startTime, downNote.noteMoveInfo.endTime + downNote.holdPath.Count * 10);
            }

            // �ж�����ͬʱ���⸡��ʽ��ǰ��ʾ
            if (judgeNote.judgeType == JudgeType.Hold)
            {
                // �ж�����
                result = result || Tools.IsInRange(CurrentTime, judgeNote.judges[0].judgeTime, judgeNote.judges[judgeNote.judges.Count - 1].judgeTime);
            }
            else if (upNote == null || downNote == null)
            {
                result = result || Tools.IsInRadiusRange(judgeNote.judge.judgeTime, CurrentTime, 1000);
            }

            if (result)
            {
                judgeNotes.Add(judgeNote);
            }
        }

        return judgeNotes.ToArray();
    }

    /// <summary>
    /// ��ȡʱ�䷶Χ�ڵ��ж�
    /// </summary>
    /// <param name="startTime">��ʼʱ��</param>
    /// <param name="endTime">����ʱ��</param>
    /// <returns></returns>
    public static InGameJudgeNoteInfo[] GetJudgeNotesInTimeRange(int startTime, int endTime)
    {
        if (CurrentEditingMap == null)
        {
            return null;
        }
        // ��ˢ��һ�����򡣡������˲�ˢ��Ҳ�У�����һ�����
        // RefreshEditingMapSorting();

        var judgeNotes = new List<InGameJudgeNoteInfo>();

        foreach (var judgeNote in CurrentEditingMap.judgeNotes)
        {
            if (Tools.IsInRange(judgeNote.judge.judgeTime, startTime, endTime))
            {
                judgeNotes.Add(judgeNote);
            }
        }

        return judgeNotes.ToArray();
    }

    /// <summary>
    /// ����ˢ������
    /// </summary>
    /// <param name="currentEditingMusicMap">��ǰ�༭����</param>
    public static void RefreshEditingMapSorting()
    {
        RefreshMapSorting(CurrentEditingMap);
    }

    public static void RefreshAllMapSorting()
    {
        var maps = EditingMapDic.Values.ToArray();
        for (int i = 0; i < maps.Length; i++)
        {
            var map = maps[i];
            RefreshMapSorting(map);
        }
    }

    public static bool CheckCurrentMap()
    {
        return CheckMap(CurrentEditingMap);
    }

    public static bool CheckMap(Difficulty difficulty)
    {
        return CheckMap(EditingMapDic[difficulty]);
    }

    public static bool CheckMap(InGameMusicMap musicMap)
    {
        bool result = true;
        var judgeNotes = musicMap.judgeNotes;
        var length = judgeNotes.Count;
        List<string> messages = new List<string>();
        for (int i = 0; i < length; i++)
        {
            var judge = judgeNotes[i].judge;
            var halfNotes = judgeNotes[i].movingNotes;
            if (halfNotes.up != null && halfNotes.down != null)
            {
                continue;
            }
            else
            {
                result = false;
                var message = $"λ��{judge.judgeTime} ms��";
                if (halfNotes.up == null)
                {
                    message += " �ϰ�Ϊ�� ";
                }

                if (halfNotes.down == null)
                {
                    message += " �°�Ϊ�� ";
                }

                messages.Add(message);
            }
        }

        if (!result)
        {
            Popup.ShowMessage($"���ڿ��ж���{string.Join("\n", messages)}", Color.red);
        }

        return result;
    }

    #endregion

    #region Private Method

    private static void RefreshMapSorting(InGameMusicMap map)
    {
        map.judgeNotes.Sort((a, b) => a.judge.judgeTime.CompareTo(b.judge.judgeTime));
    }

    #endregion
}
