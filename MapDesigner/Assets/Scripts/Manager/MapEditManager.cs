using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public class MapEditManager : MonoBehaviour
{
    public static MapEditManager Instance;

    #region properties

    /// <summary>游戏预览管理</summary>
    [SerializeField] private GamePreviewManager previewManager;
    [SerializeField] private Camera gameCamera;

    [SerializeField] private int scaleSize = 10;
    private readonly (int min, int max) sizeRange = (540, 1080);

    private Action<InGameJudgeNoteInfo> onEditJudgeNote;

    private float mouseDownTime = 0;

    /// <summary>旧位置，用于编辑判定或半note位置的撤回</summary>
    private Vector2 lastPos;

    public bool EnableEdit;

    #endregion

    void Awake()
    {
        Instance = this;
        GlobalSettings.LoadSettings();
        CoroutineUtils.SetCoroutineInstance(this);
    }

    void Start()
    {
        Initialize();
    }

    private void Update()
    {
        if (EditingData.IsOpenMap)
        {
            EditingData.CurrentTime = (int)(MusicPlayManager.Instance.MusicTime * 1000 + 0.5f) + MapDesignerSettings.Delay;
        }
    }

    #region Public Method

    public void Initialize()
    {
        var input = PlayerInputManager.Instance;
        input.OnScrollAction += OnMouseScroll;
        input.OnLeftMouseDownAction += OnMouseClickDown;
        input.OnLeftMouseKeepDownAction += OnMouseKeepDown;
        input.OnLeftMouseUpAction += OnMouseClickUp;

        EditingData.AddEditingJudgeNoteChangeListener(judgeNote =>
        {
            SceneImageRenderManager.Instance.RefreshJudgeNotePreview();
            SceneImageRenderManager.Instance.DrawEditingJudgeNoteGuideLine();
        });

        previewManager.Initialize();
        ImageRenderManager.Instance.Initialize();

        EnableEdit = true;

        // json初始化
        JsonSerializerSettings settings = new JsonSerializerSettings();
        JsonConvert.DefaultSettings = () =>
        {
            //日期类型默认格式化处理
            settings.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;
            settings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
            // 忽略循环引用
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

            // 添加转换器
            settings.Converters.Add(new AnimationCurveConvertor());
            settings.Converters.Add(new Vector2Converter());

            return settings;
        };
    }

    public void AddEditJudgeNoteListener(Action<InGameJudgeNoteInfo> onEditJudgeNote)
    {
        this.onEditJudgeNote += onEditJudgeNote;
    }

    #endregion

    #region Private Method

    private void OnMouseScroll(float deltaValue)
    {
        if (!EditingData.CanEditing || !EnableEdit)
        {
            return;
        }

        if (!EditingData.IsInGameWindow)
        {
            OnChangeCurrentBeat(deltaValue);
        }
        else
        {
            OnChangeCameraSize(deltaValue);
        }
    }

    private void OnChangeCurrentBeat(float deltaValue)
    {
        // 增加、减少的细分节拍数量
        int deltaBeatCount = 0;
        if (deltaValue > 0)
        {
            deltaBeatCount = (int) (deltaValue + 0.5f);
        }
        else
        {
            deltaBeatCount = (int)(deltaValue - 0.5f);
        }

        // var deltaTime = GetDeltaTime(deltaBeatCount);
        // 
        // var newTime = MusicPlayManager.Instance.MusicTime + deltaTime;
        // Debug.Log($"scroll value: {deltaValue}, convert count: {deltaBeatCount}, musicTime: {MusicPlayManager.Instance.MusicTime}, deltaTime: {deltaTime}");
        var musicLength = MusicPlayManager.Instance.MusicLength;
        // var nextTime = Mathf.Clamp(newTime, 0, musicLength);
        var nextTime = GetNextTime(deltaBeatCount);
        var scaleTime = Mathf.Clamp(nextTime, 0, musicLength);
        MusicPlayManager.Instance.MusicTime = scaleTime;
        // 因为上边的更新会慢一下所以采用下边的时间更新方式
        // MusicPlayManager.Instance.OnMusicProgressChanged(scaleTime / musicLength);
    }

    private void OnChangeCameraSize(float deltaValue)
    {
        var currentSize = (int)(gameCamera.orthographicSize - deltaValue * scaleSize);
        gameCamera.orthographicSize = Mathf.Clamp(currentSize, sizeRange.min, sizeRange.max);
        EditingData.GameSceneScale = gameCamera.orthographicSize / sizeRange.min;

        SceneImageRenderManager.Instance.DrawPreviewLines();
    }

    /// <summary>
    /// 获取滚轮的时间步进
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    private float GetDeltaTime(int count)
    {
        var partialBy1 = 1.0f / EditingData.PartialCountForEdit;
        var currentBeat = Tools.TransTimeToBeat(EditingData.CurrentTime, MapDesignerSettings.BpmList, out var bpm);
        // 取出当前beat（小节）的位置小数位，对应的就是当前“秒”中的位置
        var singleBeatSecond = 60 / bpm;
        var beatPos = currentBeat - (int)currentBeat;
        while (beatPos > 0 && Mathf.Abs(beatPos - partialBy1) > 1e-4)
        {
            beatPos -= partialBy1;
        }
        // Debug.Log($"beat pos: {beatPos}, 1 / partial : {partialBy1}");
        float deltaTime;
        if (beatPos > 1e-4)
        {
            deltaTime = singleBeatSecond * (partialBy1 - beatPos) + (count - 1) * singleBeatSecond * partialBy1;
        }
        else if (beatPos < 0)
        {
            deltaTime = count * singleBeatSecond * partialBy1 - singleBeatSecond * beatPos;
        }
        else
        {
            deltaTime = count * singleBeatSecond * partialBy1;
        }

        return deltaTime;
    }

    /// <summary>
    /// 前进滚轮数的小节线
    /// </summary>
    /// <param name="count">滚轮数值</param>
    /// <returns></returns>
    private float GetNextTime(int count)
    {
        var currentBeat = Tools.TransTimeToBeat(EditingData.CurrentTime, MapDesignerSettings.BpmList, out var bpm);
        var curBeatInt = (int) currentBeat;
        var beatPos = currentBeat - curBeatInt;
        // beatPos = Mathf.Max(beatPos, 0);
        var partialBy1 = 1.0f / EditingData.PartialCountForEdit;
        // +0.1是防止数值略小导致卡住
        var newBeat = new Beat(curBeatInt, (int)((beatPos + 0.01f) / partialBy1) + count, EditingData.PartialCountForEdit);
        var newTime = Tools.TransBeatToTime(newBeat, MapDesignerSettings.BpmList) + MapDesignerSettings.Delay / 1000.0f;
        // Debug.Log($"current beat: {currentBeat}, up: {(int)((beatPos + 0.01f) / partialBy1)}, by1: {partialBy1}, count: {count}, newTime: {newTime}");
        // 先对毫秒四舍五入
        return (newTime + 0.5f) / 1000;
    }

    private void OnMouseClickDown()
    {
        mouseDownTime = 0;
        if (!EditingData.CanEditing || !EditingData.IsInGameWindow || !EnableEdit)
        {
            return;
        }
        Debug.Log($"当前编辑步骤：{EditingData.CurrentEditStep} ——Click Down");

        // 虽然是中心缩放，但是原点仍然是原来的左下角
        var realPos = EditingData.MousePositionInGameAreaWithScale;
        var halfNoteClickRadius = EditingData.HalfNoteEditClickRadius;
        var judgeClickRadius = EditingData.JudgePointEditClickRadius;
        switch (EditingData.CurrentEditStep)
        {
            case EditPanelView.EditStep.Judge:
                // 判定点必须在游戏区域的水面以下
                if (Tools.IsInRange(realPos.x, 0, 1920) && Tools.IsInRange(realPos.y, 0, ConstData.WaterFaceHeight))
                {
                    // 先看是编辑判定还是添加判定
                    var judgeNotesInRange = EditingData.GetValidJudgeNotes();
                    if (judgeNotesInRange == null || judgeNotesInRange.Length == 0)
                    {
                        // 时间范围内没有判定，因此要创建判定
                        CreateJudgeNote(realPos);
                        SceneImageRenderManager.Instance.RefreshJudgeNotePreview();
                        return;
                    }
                    else
                    {
                        // 时间范围内有判定，检查一下是否点到了某个判定上
                        InGameJudgeNoteInfo judgeForCheck = null;
                        for (int i = 0; i < judgeNotesInRange.Length; i++)
                        {
                            var judgeNote = judgeNotesInRange[i];
                            if (Tools.IsInCenterRange(realPos, judgeNote.judge.judgePosition, judgeClickRadius))
                            {
                                judgeForCheck = judgeNote;
                                lastPos = judgeNote.judge.judgePosition;
                                break;
                            }
                        }

                        if (judgeForCheck == null)
                        {
                            // 没有点到判定点上
                            CreateJudgeNote(realPos);
                            SceneImageRenderManager.Instance.RefreshJudgeNotePreview();
                        }
                        else
                        {
                            // 点到了判定点
                            EditingData.CurrentEditingJudgeNote = judgeForCheck;
                            MusicPlayManager.Instance.MusicTime = judgeForCheck.judge.judgeTime / 1000.0f;
                            onEditJudgeNote?.Invoke(judgeForCheck);
                        }
                        EditPanelView.Instance.ShowView();
                        EditPanelView.Instance.OnEditJudgeButton();
                    }
                }
                break;
            case EditPanelView.EditStep.HalfNote:
                // 这个大括号只是为了隔离变量的生命周期
                {
                    // 半note也是以判定为基准，以便在选择半note编辑的时候可以切换到对应的判定
                    var judgeNotesInRange = EditingData.GetValidJudgeNotes();
                    if ((judgeNotesInRange == null || judgeNotesInRange.Length == 0) && EditingData.CurrentEditingJudgeNote == null)
                    {
                        Popup.ShowMessage("当前时间前后没有编辑中的判定，先创建判定吧", Color.red);
                    }
                    else
                    {
                        // 检查是否点到了某个半note上
                        InGameJudgeNoteInfo judgeForCheck = null;
                        EditNoteInfo halfNoteForCheck = null;
                        if (judgeNotesInRange != null)
                        {
                            for (int i = 0; i < judgeNotesInRange.Length; i++)
                            {
                                var judgeNote = judgeNotesInRange[i];
                                var upNote = judgeNote.movingNotes.up;
                                var downNote = judgeNote.movingNotes.down;

                                // 10ms一个位置
                                if (upNote != null)
                                {
                                    var upIndex = (EditingData.CurrentTime - upNote.noteMoveInfo.startTime) / 10;
                                    upIndex = Mathf.Clamp(upIndex, 0, upNote.totalPath.Count - 1);
                                    if (Tools.IsInCenterRange(realPos, upNote.totalPath[upIndex], halfNoteClickRadius))
                                    {
                                        judgeForCheck = judgeNote;
                                        halfNoteForCheck = upNote;
                                        break;
                                    }
                                }

                                if (downNote != null)
                                {
                                    var downIndex = (EditingData.CurrentTime - downNote.noteMoveInfo.startTime) / 10;
                                    downIndex = Mathf.Clamp(downIndex, 0, downNote.totalPath.Count - 1);
                                    if (Tools.IsInCenterRange(realPos, downNote.totalPath[downIndex], halfNoteClickRadius))
                                    {
                                        judgeForCheck = judgeNote;
                                        halfNoteForCheck = downNote;
                                        break;
                                    }
                                }
                            }
                        }

                        if (judgeForCheck == null)
                        {
                            // 没有点到半note
                            // 元组不是引用类型
                            var moveNotes = EditingData.CurrentEditingJudgeNote.movingNotes;
                            if (moveNotes.up == null)
                            {
                                var command = new AddHalfNoteCommand(HalfType.UpHalf, realPos, EditingData.CurrentTime);
                                command.Execute();
                            }
                            else if (moveNotes.down == null)
                            {
                                var command = new AddHalfNoteCommand(HalfType.DownHalf, realPos, EditingData.CurrentTime);
                                command.Execute();
                            }
                            else
                            {
                                Popup.ShowMessage("当前编辑中判定的上下半note均已存在", Color.red);
                            }
                        }
                        else
                        {
                            // 点到了半note
                            EditingData.CurrentEditingJudgeNote = judgeForCheck;
                            EditingData.CurrentEditingHalfNote = halfNoteForCheck;
                            lastPos = halfNoteForCheck.noteMoveInfo.p0;
                        }
                        EditPanelView.Instance.ShowView();
                        EditPanelView.Instance.OnEditHalfNoteButton();
                    }
                }
                break;
            case EditPanelView.EditStep.HoldJudge:
                if (Tools.IsInRange(realPos.x, 0, 1920) && Tools.IsInRange(realPos.y, 0, ConstData.WaterFaceHeight))
                {
                    void addJudgeToEditingJudge()
                    {
                        var command = new AddHoldJudgeCommand(EditingData.CurrentEditingJudgeNote, realPos, EditingData.CurrentTime);
                        command.Execute();
                    }

                    var judgeNotesInRange = EditingData.GetValidJudgeNotes();
                    if (judgeNotesInRange == null || judgeNotesInRange.Length == 0)
                    {
                        if (EditingData.CurrentEditingJudgeNote.judgeType == JudgeType.Hold)
                        {
                            addJudgeToEditingJudge();
                        }
                        else
                        {
                            Popup.ShowMessage("当前时间前后没有编辑中的判定，先创建判定吧", Color.red);
                        }
                    }
                    else
                    {
                        var holdJudgeNotes = judgeNotesInRange.Where(judgeNote => judgeNote.judgeType == JudgeType.Hold).ToArray();
                        if (holdJudgeNotes.Length == 0)
                        {
                            if (EditingData.CurrentEditingJudgeNote.judgeType != JudgeType.Hold)
                            {
                                Popup.ShowMessage("当前时间前后没有编辑中的Hold判定，先创建判定吧", Color.red);
                            }
                            else
                            {
                                addJudgeToEditingJudge();
                            }
                        }
                        else
                        {
                            InGameJudgeNoteInfo judgeForCheck = null;
                            int judgeIndex = 0;
                            for (int i = 0; i < holdJudgeNotes.Length; i++)
                            {
                                var judgeNote = holdJudgeNotes[i];
                                for (int j = 0; j < judgeNote.judges.Count; j++)
                                {
                                    var judge = judgeNote.judges[j];
                                    if (Tools.IsInCenterRange(realPos, judge.judgePosition, judgeClickRadius))
                                    {
                                        judgeForCheck = judgeNote;
                                        judgeIndex = j;
                                        lastPos = judge.judgePosition;
                                        break;
                                    }
                                }

                                if (judgeForCheck != null)
                                {
                                    break;
                                }
                            }

                            if (judgeForCheck == null)
                            {
                                if (EditingData.CurrentEditingJudgeNote.judgeType != JudgeType.Hold)
                                {
                                    EditingData.CurrentEditingJudgeNote = holdJudgeNotes.First();
                                }

                                addJudgeToEditingJudge();
                            }
                            else
                            {
                                EditingData.CurrentEditingJudgeNote = judgeForCheck;
                                EditingData.CurrentEditingHoldJudgeIndex = judgeIndex;
                            }
                        }
                    }

                    EditPanelView.Instance.ShowView();
                    EditPanelView.Instance.OnEditHoldPathButton();
                }
                break;
            case EditPanelView.EditStep.HoldPath:
                if (Tools.IsInRange(realPos.x, 0, 1920) && Tools.IsInRange(realPos.y, 0, ConstData.WaterFaceHeight))
                {
                    var judgeNotesInRange = EditingData.GetValidJudgeNotes();
                    if (judgeNotesInRange == null || judgeNotesInRange.Length == 0)
                    {
                        Popup.ShowMessage("当前时间前后没有编辑中的判定，先创建判定吧", Color.red);
                    }
                    else
                    {
                        var holdJudgeNotes = judgeNotesInRange.Where(judgeNote => judgeNote.judgeType == JudgeType.Hold).ToArray();
                        if (holdJudgeNotes.Length == 0)
                        {
                            Popup.ShowMessage("当前时间前后没有编辑中的Hold判定，先创建判定吧", Color.red);
                        }
                        else
                        {
                            InGameJudgeNoteInfo judgeForCheck = null;
                            int judgeIndex = 0;
                            for (int i = 0; i < holdJudgeNotes.Length; i++)
                            {
                                var judgeNote = holdJudgeNotes[i];
                                for (int j = 0; j < judgeNote.judges.Count; j++)
                                {
                                    var judge = judgeNote.judges[j];
                                    if (Tools.IsInCenterRange(realPos, judge.judgePosition, judgeClickRadius))
                                    {
                                        judgeForCheck = judgeNote;
                                        judgeIndex = j;
                                        lastPos = judge.judgePosition;
                                        break;
                                    }
                                }

                                if (judgeForCheck != null)
                                {
                                    break;
                                }
                            }

                            if (judgeForCheck == null)
                            {
                                return;
                            }

                            EditingData.CurrentEditingJudgeNote = judgeForCheck;
                            EditingData.CurrentEditingHoldJudgeIndex = judgeIndex;
                            // 编辑路径参数
                            EditPanelView.Instance.ShowView();
                            EditPanelView.Instance.OnEditHoldPathButton();
                        }
                    }
                }
                break;
        }
    }

    private void OnMouseKeepDown()
    {
        mouseDownTime += Time.deltaTime;
        if (mouseDownTime < ConstData.dragDelayTime)
        {
            return;
        }
        if (!EditingData.CanEditing || !EditingData.IsInGameWindow || EditingData.CurrentEditingJudgeNote == null || !EnableEdit)
        {
            return;
        }

        // 虽然是中心缩放，但是原点仍然是原来的左下角
        var realPos = EditingData.MousePositionInGameAreaWithScale;
        switch (EditingData.CurrentEditStep)
        {
            case EditPanelView.EditStep.Judge:
                if (Tools.IsInRange(realPos.x, 0, 1920) && Tools.IsInRange(realPos.y, 0, ConstData.WaterFaceHeight))
                {
                    EditingData.CurrentEditingJudgeNote.judge.judgePosition = realPos;
                    SceneImageRenderManager.Instance.RefreshJudgeNotePreview();
                }
                break;
            case EditPanelView.EditStep.HalfNote:
                {
                    if (EditingData.CurrentEditingHalfNote == null)
                    {
                        break;
                    }

                    // EditingData.CurrentEditingHalfNote.SetInitPos(realPos);
                    EditingData.CurrentEditingHalfNote.noteMoveInfo.p0 = realPos;
                    GamePreviewManager.Instance.UpdatePreview();
                }
                break;
            case EditPanelView.EditStep.HoldJudge:
                if (EditingData.CurrentEditingJudgeNote.judgeType != JudgeType.Hold
                    || EditingData.CurrentEditingJudgeNote.judges.Count <= EditingData.CurrentEditingHoldJudgeIndex)
                {
                    break;
                }

                var judge = EditingData.CurrentEditingJudgeNote.judges[EditingData.CurrentEditingHoldJudgeIndex];
                if (Tools.IsInRange(realPos.x, 0, 1920) && Tools.IsInRange(realPos.y, 0, ConstData.WaterFaceHeight))
                {
                    judge.judgePosition = realPos;
                    SceneImageRenderManager.Instance.RefreshJudgeNotePreview();
                }
                break;
            case EditPanelView.EditStep.HoldPath:
                break;
            default:
                break;
        }
    }

    private void OnMouseClickUp()
    {
        mouseDownTime += Time.deltaTime;
        if (mouseDownTime < ConstData.dragDelayTime)
        {
            return;
        }
        if (!EditingData.CanEditing || !EditingData.IsInGameWindow || EditingData.CurrentEditingJudgeNote == null || !EnableEdit)
        {
            return;
        }
        Debug.Log($"当前编辑步骤：{EditingData.CurrentEditStep} ——Click Up");

        var realPos = EditingData.MousePositionInGameAreaWithScale;
        var currentEditingJudgeNote = EditingData.CurrentEditingJudgeNote;
        switch (EditingData.CurrentEditStep)
        {
            case EditPanelView.EditStep.Judge:
                {
                    var command = new EditJudgeNotePositionCommand(currentEditingJudgeNote, lastPos, currentEditingJudgeNote.judge.judgePosition);
                    command.Execute();
                    EditPanelView.Instance.ShowView();
                    EditPanelView.Instance.OnEditJudgeButton();
                }
                break;
            case EditPanelView.EditStep.HalfNote:
                {
                    if (EditingData.CurrentEditingHalfNote == null)
                    {
                        break;
                    }

                    var command = new EditHalfNotePositionCommand(EditingData.CurrentEditingHalfNote, lastPos, realPos, currentEditingJudgeNote.judge.judgeTime);
                    command.Execute();
                }
                break;
            case EditPanelView.EditStep.HoldJudge:
                break;
            case EditPanelView.EditStep.HoldPath:
                break;
            default:
                break;
        }

        mouseDownTime = 0;

        SceneImageRenderManager.Instance.DrawEditingJudgeNoteGuideLine();
    }

    private void CreateJudgeNote(Vector2 judgePos)
    {
        var command = new AddJudgeNoteCommand(judgePos, EditingData.CurrentTime, onEditJudgeNote);
        command.Execute();
    }

    #endregion
}
