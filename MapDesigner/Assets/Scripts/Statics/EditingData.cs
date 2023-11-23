using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using UnityEngine;

public static class EditingData
{
    #region Properties

    /// <summary>主窗口尺寸</summary>
    public static (float width, float height) MainCanvasSize;
    /// <summary>游戏窗口的缩放值</summary>
    public static float GameSceneScale;
    /// <summary>屏幕大小</summary>
    public static (float width, float height) ScreenSize;
    /// <summary>游戏窗口（的左下角原点）位置</summary>
    public static (float x, float y) GameWindowPosition;
    /// <summary>游戏窗口大小</summary>
    public static (float width, float height) GameWindowSize;
    /// <summary>游戏场景内实际的原点在编辑窗口坐标系中的位置</summary>
    public static Vector2 RealGameZeroPos => ConstData.GameCenter * (GameSceneScale - 1);
    /// <summary>编辑时可供点选半note的半径</summary>
    public static float HalfNoteEditClickRadius => ConstData.HalfNoteClickRadius * GameSceneScale * (ScreenSize.width / GameWindowSize.width) * 0.5f;
    /// <summary>编辑时可供点选判定点的半径</summary>
    public static float JudgePointEditClickRadius => ConstData.JudgePointClickRadius * GameSceneScale * (ScreenSize.width / GameWindowSize.width) * 0.5f;

    /// <summary>是否打开音乐</summary>
    public static bool IsOpenMusic;
    /// <summary>是否打开谱面</summary>
    public static bool IsOpenMap;
    /// <summary>是否外游戏窗口内</summary>
    public static bool IsInGameWindow;
    /// <summary>是否可以编辑</summary>
    public static bool CanEditing => IsOpenMap && IsOpenMusic;
    /// <summary>当前（谱面）时间(ms)</summary>
    public static int CurrentTime;
    public static int CurrentTimeWithoutDelay => CurrentTime - MapDesignerSettings.Delay;
    /// <summary>选择（谱面）时间区域</summary>
    public static float SectionTime1;
    public static float SectionTime2;
    public static bool SectionSelected1;
    public static bool SectionSelected2;
    /// <summary>画面缩放大小</summary>
    public static int screenSize;
    /// <summary>以第几分音编辑</summary>
    public static int PartialIndexForEdit;

    private static int partialCountForEdit;

    /// <summary>
    /// 以几分音编辑
    /// 节拍细分（属于常量表内的值）
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
    /// <summary>BPM比例尺</summary>
    public static float BpmScale;

    #region 编辑中内容

    /// <summary>判定点的当前编辑步骤</summary>
    public static EditPanelView.EditStep CurrentEditStep;

    private static Action<Difficulty> OnDifficultyChanged;
    private static Difficulty currentMapDifficulty;
    /// <summary>当前谱面难度</summary>
    public static Difficulty CurrentMapDifficulty
    {
        get => currentMapDifficulty;
        set
        {
            currentMapDifficulty = value;
            OnDifficultyChanged?.Invoke(value);
        }
    }
    /// <summary>当前编辑谱面</summary>
    public static InGameMusicMap CurrentEditingMap;

    private static InGameJudgeNoteInfo currentEditingJudgeNote;

    /// <summary>当前编辑判定</summary>
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

    /// <summary>当前编辑中的hold判定点索引</summary>
    public static int CurrentEditingHoldJudgeIndex;
    /// <summary>当前编辑中半note</summary>
    public static EditNoteInfo CurrentEditingHalfNote;


    /// <summary>多个难度的谱面</summary>
    public static Dictionary<Difficulty, InGameMusicMap> EditingMapDic;

    #endregion

    #region 输入数据

    /// <summary>游戏画板坐标系的鼠标位置</summary>
    public static (float x, float y) MousePositionInGameArea;
    /// <summary>缩放后的鼠标位置对应的实际游戏坐标</summary>
    public static Vector2 MousePositionInGameAreaWithScale;
    /// <summary>主画板坐标系的鼠标位置</summary>
    public static (float x, float y) MousePositionInMainPanel;
    /// <summary>程序窗口坐标系的游戏窗口左下位置</summary>
    public static (float x, float y) GameAreaLeftDownPositionInMainWindow;
    /// <summary>程序窗口坐标系的游戏窗口右上位置</summary>
    public static (float x, float y) GameAreaRightUpPositionInMainWindow;
    /// <summary>主画板坐标系的游戏窗口左下位置</summary>
    public static (float x, float y) GameAreaLeftDownPositionInMainPanel;
    /// <summary>主画板坐标系的游戏窗口右上位置</summary>
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
    /// 添加难度变更事件监听
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
    /// 添加节拍细分变更的事件监听
    /// </summary>
    /// <param name="onPartialCountChanged"></param>
    public static void AddPartialCountForEditChangeListener(Action<int> onPartialCountChanged)
    {
        OnPartialCountChanged += onPartialCountChanged;
    }

    /// <summary>
    /// 获取有效的判定信息
    /// <c>有效：指该判定的上半或下半在当前时间处于显示状态</c>
    /// </summary>
    /// <returns>判定列表</returns>
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
            // 上半note
            bool result = false;
            if (upNote != null)
            {
                result = result || Tools.IsInRange(CurrentTime, upNote.noteMoveInfo.startTime, upNote.noteMoveInfo.endTime + upNote.holdPath.Count * 10);
            }
            // 下半note
            if (downNote != null)
            {
                result = result || Tools.IsInRange(CurrentTime, downNote.noteMoveInfo.startTime, downNote.noteMoveInfo.endTime + downNote.holdPath.Count * 10);
            }

            // 判定本身，同时避免浮现式提前显示
            if (judgeNote.judgeType == JudgeType.Hold)
            {
                // 判定本身
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
    /// 获取时间范围内的判定
    /// </summary>
    /// <param name="startTime">开始时间</param>
    /// <param name="endTime">结束时间</param>
    /// <returns></returns>
    public static InGameJudgeNoteInfo[] GetJudgeNotesInTimeRange(int startTime, int endTime)
    {
        if (CurrentEditingMap == null)
        {
            return null;
        }
        // 先刷新一遍排序。。。算了不刷新也行，遍历一遍得了
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
    /// 谱面刷新排序
    /// </summary>
    /// <param name="currentEditingMusicMap">当前编辑谱面</param>
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
                var message = $"位于{judge.judgeTime} ms，";
                if (halfNotes.up == null)
                {
                    message += " 上半为空 ";
                }

                if (halfNotes.down == null)
                {
                    message += " 下半为空 ";
                }

                messages.Add(message);
            }
        }

        if (!result)
        {
            Popup.ShowMessage($"存在空判定：{string.Join("\n", messages)}", Color.red);
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
