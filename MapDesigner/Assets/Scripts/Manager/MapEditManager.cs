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

    /// <summary>��ϷԤ������</summary>
    [SerializeField] private GamePreviewManager previewManager;
    [SerializeField] private Camera gameCamera;

    [SerializeField] private int scaleSize = 10;
    private readonly (int min, int max) sizeRange = (540, 1080);

    private Action<InGameJudgeNoteInfo> onEditJudgeNote;

    private float mouseDownTime = 0;

    /// <summary>��λ�ã����ڱ༭�ж����noteλ�õĳ���</summary>
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

        // json��ʼ��
        JsonSerializerSettings settings = new JsonSerializerSettings();
        JsonConvert.DefaultSettings = () =>
        {
            //��������Ĭ�ϸ�ʽ������
            settings.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;
            settings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
            // ����ѭ������
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

            // ���ת����
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
        // ���ӡ����ٵ�ϸ�ֽ�������
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
        // ��Ϊ�ϱߵĸ��»���һ�����Բ����±ߵ�ʱ����·�ʽ
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
    /// ��ȡ���ֵ�ʱ�䲽��
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    private float GetDeltaTime(int count)
    {
        var partialBy1 = 1.0f / EditingData.PartialCountForEdit;
        var currentBeat = Tools.TransTimeToBeat(EditingData.CurrentTime, MapDesignerSettings.BpmList, out var bpm);
        // ȡ����ǰbeat��С�ڣ���λ��С��λ����Ӧ�ľ��ǵ�ǰ���롱�е�λ��
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
    /// ǰ����������С����
    /// </summary>
    /// <param name="count">������ֵ</param>
    /// <returns></returns>
    private float GetNextTime(int count)
    {
        var currentBeat = Tools.TransTimeToBeat(EditingData.CurrentTime, MapDesignerSettings.BpmList, out var bpm);
        var curBeatInt = (int) currentBeat;
        var beatPos = currentBeat - curBeatInt;
        // beatPos = Mathf.Max(beatPos, 0);
        var partialBy1 = 1.0f / EditingData.PartialCountForEdit;
        // +0.1�Ƿ�ֹ��ֵ��С���¿�ס
        var newBeat = new Beat(curBeatInt, (int)((beatPos + 0.01f) / partialBy1) + count, EditingData.PartialCountForEdit);
        var newTime = Tools.TransBeatToTime(newBeat, MapDesignerSettings.BpmList) + MapDesignerSettings.Delay / 1000.0f;
        // Debug.Log($"current beat: {currentBeat}, up: {(int)((beatPos + 0.01f) / partialBy1)}, by1: {partialBy1}, count: {count}, newTime: {newTime}");
        // �ȶԺ�����������
        return (newTime + 0.5f) / 1000;
    }

    private void OnMouseClickDown()
    {
        mouseDownTime = 0;
        if (!EditingData.CanEditing || !EditingData.IsInGameWindow || !EnableEdit)
        {
            return;
        }
        Debug.Log($"��ǰ�༭���裺{EditingData.CurrentEditStep} ����Click Down");

        // ��Ȼ���������ţ�����ԭ����Ȼ��ԭ�������½�
        var realPos = EditingData.MousePositionInGameAreaWithScale;
        var halfNoteClickRadius = EditingData.HalfNoteEditClickRadius;
        var judgeClickRadius = EditingData.JudgePointEditClickRadius;
        switch (EditingData.CurrentEditStep)
        {
            case EditPanelView.EditStep.Judge:
                // �ж����������Ϸ�����ˮ������
                if (Tools.IsInRange(realPos.x, 0, 1920) && Tools.IsInRange(realPos.y, 0, ConstData.WaterFaceHeight))
                {
                    // �ȿ��Ǳ༭�ж���������ж�
                    var judgeNotesInRange = EditingData.GetValidJudgeNotes();
                    if (judgeNotesInRange == null || judgeNotesInRange.Length == 0)
                    {
                        // ʱ�䷶Χ��û���ж������Ҫ�����ж�
                        CreateJudgeNote(realPos);
                        SceneImageRenderManager.Instance.RefreshJudgeNotePreview();
                        return;
                    }
                    else
                    {
                        // ʱ�䷶Χ�����ж������һ���Ƿ�㵽��ĳ���ж���
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
                            // û�е㵽�ж�����
                            CreateJudgeNote(realPos);
                            SceneImageRenderManager.Instance.RefreshJudgeNotePreview();
                        }
                        else
                        {
                            // �㵽���ж���
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
                // ���������ֻ��Ϊ�˸����������������
                {
                    // ��noteҲ�����ж�Ϊ��׼���Ա���ѡ���note�༭��ʱ������л�����Ӧ���ж�
                    var judgeNotesInRange = EditingData.GetValidJudgeNotes();
                    if ((judgeNotesInRange == null || judgeNotesInRange.Length == 0) && EditingData.CurrentEditingJudgeNote == null)
                    {
                        Popup.ShowMessage("��ǰʱ��ǰ��û�б༭�е��ж����ȴ����ж���", Color.red);
                    }
                    else
                    {
                        // ����Ƿ�㵽��ĳ����note��
                        InGameJudgeNoteInfo judgeForCheck = null;
                        EditNoteInfo halfNoteForCheck = null;
                        if (judgeNotesInRange != null)
                        {
                            for (int i = 0; i < judgeNotesInRange.Length; i++)
                            {
                                var judgeNote = judgeNotesInRange[i];
                                var upNote = judgeNote.movingNotes.up;
                                var downNote = judgeNote.movingNotes.down;

                                // 10msһ��λ��
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
                            // û�е㵽��note
                            // Ԫ�鲻����������
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
                                Popup.ShowMessage("��ǰ�༭���ж������°�note���Ѵ���", Color.red);
                            }
                        }
                        else
                        {
                            // �㵽�˰�note
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
                            Popup.ShowMessage("��ǰʱ��ǰ��û�б༭�е��ж����ȴ����ж���", Color.red);
                        }
                    }
                    else
                    {
                        var holdJudgeNotes = judgeNotesInRange.Where(judgeNote => judgeNote.judgeType == JudgeType.Hold).ToArray();
                        if (holdJudgeNotes.Length == 0)
                        {
                            if (EditingData.CurrentEditingJudgeNote.judgeType != JudgeType.Hold)
                            {
                                Popup.ShowMessage("��ǰʱ��ǰ��û�б༭�е�Hold�ж����ȴ����ж���", Color.red);
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
                        Popup.ShowMessage("��ǰʱ��ǰ��û�б༭�е��ж����ȴ����ж���", Color.red);
                    }
                    else
                    {
                        var holdJudgeNotes = judgeNotesInRange.Where(judgeNote => judgeNote.judgeType == JudgeType.Hold).ToArray();
                        if (holdJudgeNotes.Length == 0)
                        {
                            Popup.ShowMessage("��ǰʱ��ǰ��û�б༭�е�Hold�ж����ȴ����ж���", Color.red);
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
                            // �༭·������
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

        // ��Ȼ���������ţ�����ԭ����Ȼ��ԭ�������½�
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
        Debug.Log($"��ǰ�༭���裺{EditingData.CurrentEditStep} ����Click Up");

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
