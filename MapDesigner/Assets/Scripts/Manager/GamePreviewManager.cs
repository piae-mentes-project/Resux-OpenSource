using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePreviewManager : MonoBehaviour
{
    public static GamePreviewManager Instance;

    #region properties

    /// <summary>note显示区域</summary>
    public RectTransform NoteArea;

    /// <summary>Note预制体</summary>
    private GameObject NotePrefab;
    private Dictionary<HalfType, Dictionary<JudgeType, Sprite>> SpriteDic => Resux.GamePlay.HalfNote.SpriteDic;
    /// <summary>
    /// 半note对象池
    /// </summary>
    /// <remarks>用image是因为image一样携带transform，且可以避免重复的GetComponent方法消耗性能</remarks>
    private ObjectPool<Image> halfNotePool;

    private Dictionary<EditNoteInfo, Image> previewingNoteDic;
    private Dictionary<EditNoteInfo, Image> tempNoteDic;

    /// <summary>预览时的判定队列</summary>
    private Queue<(JudgePair judge, JudgeType type)> judgeNoteQueue;

    public bool IsGamePreviewOpen { get; private set; }

    #endregion

    #region Unity

    private void Awake()
    {
        Instance = this;
    }

    #endregion

    #region Public Method

    /// <summary>
    /// 初始化
    /// </summary>
    public void Initialize()
    {
        NotePrefab = Resources.Load<GameObject>("Prefabs/Note");

        previewingNoteDic = new Dictionary<EditNoteInfo, Image>();
        tempNoteDic = new Dictionary<EditNoteInfo, Image>();

        judgeNoteQueue = new Queue<(JudgePair judge, JudgeType type)>();

        halfNotePool = new ObjectPool<Image>(10, CreateNote, OnReturnNote);

        MusicPlayManager.Instance.AddMusicPlayingListener(time =>
        {
            if (!IsGamePreviewOpen)
            {
                UpdatePreview(time);
                PreviewAudioEffect(time);
            }
        });

        PlayerInputManager.Instance.OnCtrlPAction += () =>
        {
            IsGamePreviewOpen = !IsGamePreviewOpen;
            OnPreviewChange(IsGamePreviewOpen);
        };
    }

    /// <summary>
    /// 更新预览
    /// </summary>
    public void UpdatePreview(float time = -1)
    {
        int currentPlayTime = 0;
        if (time > 0)
        {
            currentPlayTime = (int)(time * 1000 + 0.5f) + MapDesignerSettings.Delay;
        }
        else
        {
            currentPlayTime = EditingData.CurrentTime;
        }
        
        var judgeNotesInRange = EditingData.GetValidJudgeNotes();

        if (judgeNotesInRange != null && judgeNotesInRange.Length != 0)
        {
            // 获取并更新半note的显示，临时存储在tempDic内
            foreach (var judgeNote in judgeNotesInRange)
            {
                var upNote = judgeNote.movingNotes.up;
                var downNote = judgeNote.movingNotes.down;
                if (upNote != null)
                {
                    if (previewingNoteDic.ContainsKey(upNote))
                    {
                        tempNoteDic.Add(upNote, previewingNoteDic[upNote]);
                        previewingNoteDic.Remove(upNote);
                    }
                    else
                    {
                        tempNoteDic.Add(upNote, halfNotePool.GetObject());
                    }

                    NoteOnUpdate(upNote, tempNoteDic[upNote], currentPlayTime, judgeNote.judgeType);
                }

                if (downNote != null)
                {
                    if (previewingNoteDic.ContainsKey(downNote))
                    {
                        tempNoteDic.Add(downNote, previewingNoteDic[downNote]);
                        previewingNoteDic.Remove(downNote);
                    }
                    else
                    {
                        tempNoteDic.Add(downNote, halfNotePool.GetObject());
                    }

                    NoteOnUpdate(downNote, tempNoteDic[downNote], currentPlayTime, judgeNote.judgeType);
                }
            }
        }

        if (previewingNoteDic.Count > 0)
        {
            foreach (var note in previewingNoteDic)
            {
                halfNotePool.ReturnToPool(note.Value);
            }
        }

        var temp = previewingNoteDic;
        temp.Clear();
        previewingNoteDic = tempNoteDic;
        tempNoteDic = temp;

        // 预览绘制更新
        SceneImageRenderManager.Instance.DrawPreviewLines();
    }

    /// <summary>
    /// 初始化判定队列
    /// </summary>
    public void InitJudgeNoteQueue()
    {
        var map = EditingData.CurrentEditingMap;
        if (map == null)
        {
            return;
        }

        var previewJudgeNotes = new List<(JudgePair judge, JudgeType type)>();
        foreach (var judgeNote in map.judgeNotes)
        {
            foreach (var judge in judgeNote.judges)
            {
                previewJudgeNotes.Add((judge, judgeNote.judgeType));
            }
        }

        previewJudgeNotes.Sort((left, right) => left.judge.judgeTime.CompareTo(right.judge.judgeTime));

        judgeNoteQueue.Clear();
        foreach (var @group in previewJudgeNotes)
        {
            judgeNoteQueue.Enqueue(@group);
        }
    }

    #endregion

    #region Private Method

    /// <summary>
    /// 预览判定音效
    /// </summary>
    private void PreviewAudioEffect(float time = -1)
    {
        int currentPlayTime = 0;
        if (time > 0)
        {
            currentPlayTime = (int)(time * 1000 + 0.5f) + MapDesignerSettings.Delay;
        }
        else
        {
            currentPlayTime = EditingData.CurrentTime;
        }
        for (int i = 0; i < judgeNoteQueue.Count; i++)
        {
            var judgeInfo = judgeNoteQueue.Peek();
            if (judgeInfo.judge.judgeTime <= currentPlayTime)
            {
                MusicPlayManager.Instance.PlayEffect(judgeInfo.type);
                judgeNoteQueue.Dequeue();
            }
            else
            {
                break;
            }
        }
    }

    private void NoteOnUpdate(EditNoteInfo editNoteInfo, Image note, int time, JudgeType judgeType)
    {
        note.sprite = SpriteDic[editNoteInfo.halfType][judgeType];
        if (EditingData.CurrentEditingJudgeNote != null && (EditingData.CurrentEditingJudgeNote.movingNotes.up == editNoteInfo || EditingData.CurrentEditingJudgeNote.movingNotes.down == editNoteInfo))
        {
            note.color = ConstData.EditingHalfNoteColor;
        }
        else
        {
            note.color = ConstData.NotEditingHalfNoteColor;
        }

        var index = (time - editNoteInfo.noteMoveInfo.startTime) / 10;
        index = Mathf.Clamp(index, 0, editNoteInfo.totalPath.Count - 1);
        if (index == 0 && !editNoteInfo.noteMoveInfo.isReverse)
        {
            note.transform.localPosition = editNoteInfo.noteMoveInfo.p0;
        }
        else
        {
            note.transform.localPosition = editNoteInfo.totalPath[index];
        }

        if (!note.gameObject.activeSelf)
        {
            note.gameObject.SetActive(true);
        }
    }

    private Image CreateNote()
    {
        var note = Instantiate(NotePrefab);
        note.transform.SetParent(NoteArea);
        note.SetActive(false);
        return note.GetComponent<Image>();
    }

    private void OnReturnNote(Image image)
    {
        image.gameObject.SetActive(false);
    }

    private void OnPreviewChange(bool isOpenGamePreview)
    {
        if (isOpenGamePreview)
        {
            if (EditingData.CheckCurrentMap())
            {
                SceneImageRenderManager.Instance.ClearAllRender();
                Resux.GamePlay.GamePlayer.Instance.Play();
            }
        }
        else
        {
            Resux.GamePlay.GamePlayer.Instance.Stop();
            UpdatePreview();
        }
    }

    #endregion
}
