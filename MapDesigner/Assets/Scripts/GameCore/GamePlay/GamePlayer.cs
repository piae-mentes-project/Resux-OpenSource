using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using Resux.UI;
using Resux.Component;

namespace Resux.GamePlay
{
    public class GamePlayer : MonoBehaviour
    {
        #region properties

        /// <summary>单例实例</summary>
        public static GamePlayer Instance;

        [SerializeField][Tooltip("半note的透明度曲线")] private AnimationCurve alphaCurve;

        /// <summary>装饰性note</summary>
        private List<HalfNote> decorativeNotes;
        private Queue<HalfNote> allDecorativeNoteQueue;
        /// <summary>tap判定</summary>
        private List<NormalJudgeNote> tapNotes;
        /// <summary>flick判定</summary>
        private List<NormalJudgeNote> flickNotes;
        /// <summary>hold判定</summary>
        private List<HoldJudgeNote> holdNotes;

        private List<IJudgeNote> sortedJudgeNotes;
        private Queue<IJudgeNote> judgeQueue;
        /// <summary>队列中提前入队的ms数</summary>
        private int preShowTime = 100;

        private ObjectPool<GameObject> perfectEffectPool;
        private ObjectPool<UIFrameAnimation> holdEffectPool;
        private ObjectPool<UIFrameAnimation> scaleEffectPool;

        #region MusicProperties

        /// <summary>是否在游玩中</summary>
        private bool isPlaying;

        /// <summary>
        /// 当前时间（指从歌开始）
        /// </summary>
        public int MusicTime => (int)(musicPlayer.MusicTime * 1000 + 0.5f);

        /// <summary>
        /// 加上延迟的“实际时间”
        /// </summary>
        public int timeWithOffset => MusicTime;

        /// <summary>谱面难度</summary>
        public Difficulty Diffculty => EditingData.CurrentMapDifficulty;

        #endregion

        #region 场景实例

        /// <summary>
        /// 半note的父物体
        /// </summary>
        public Transform SingleNoteParent;

        /// <summary>
        /// 特效区父物体
        /// </summary>
        public Transform JudgeEffectParent;

        /// <summary>
        /// Hold路径父物体
        /// </summary>
        public Transform HoldPathParent;

        /// <summary>
        /// 歌曲进度
        /// </summary>
        [SerializeField] private PlaySlider progresSlider;

        /// <summary>
        /// 预览内容
        /// </summary>
        [SerializeField] private GameObject[] gamePreviewObjs;

        /// <summary>
        /// 音乐播放器
        /// </summary>
        public MusicPlayManager musicPlayer
        {
            get => MusicPlayManager.Instance;
        }

        #endregion

        #endregion

        #region UnityEngine

        void Awake()
        {
            JudgeEffectSetting.GetEffectPrefab(JudgeResult.PERFECT);
        }

        void OnEnable()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        void Start()
        {
            musicPlayer.AddMusicPlayStatusChangedListener(OnSpaceButton);

            isPlaying = false;
            progresSlider.Value = 0;

            Initialize();
            ShowPreview(false);
        }

        void Update()
        {
            if (!isPlaying)
            {
                return;
            }

            UpdateNotes();

            var cachedCurrentTime = timeWithOffset;
            var holdNotesLength = holdNotes.Count;
            for (int i = 0, j = 0; i < holdNotesLength - j; i++)
            {
                // 自动移除已被判定的note，以缩短后面的for时间，同时减轻内存负担
                var note = holdNotes[i];
                if (note.IsReleased)
                {
                    note.OnDestroy();
                    holdNotes.Remove(note);
                    j++;
                    i--;
                }
                else
                {
                    note.OnUpdate(cachedCurrentTime);
                }
            }

            var flickNotesLength = flickNotes.Count;
            for (int i = 0, j = 0; i < flickNotesLength - j; i++)
            {
                // 自动移除已被判定的note，以缩短后面的for时间，同时减轻内存负担
                var note = flickNotes[i];
                if (note.IsReleased)
                {
                    note.OnDestroy();
                    flickNotes.Remove(note);
                    j++;
                    i--;
                }
                else
                {
                    note.OnUpdate(cachedCurrentTime);
                }
            }

            var tapNotesLength = tapNotes.Count;
            for (int i = 0, j = 0; i < tapNotesLength - j; i++)
            {
                // 自动移除已被判定的note，以缩短后面的for时间，同时减轻内存负担
                var note = tapNotes[i];
                if (note.IsReleased)
                {
                    note.OnDestroy();
                    tapNotes.Remove(note);
                    j++;
                    i--;
                }
                else
                {
                    note.OnUpdate(cachedCurrentTime);
                }
            }

            // 装饰性半note的运动
            if (decorativeNotes.Any())
            {
                decorativeNotes.ForEach(note => note.Update(cachedCurrentTime));
                decorativeNotes.RemoveAll(halfNote => halfNote.collisionTime < cachedCurrentTime);
            }

            // 音频默认长度是s，转换成ms
            var progress = MusicTime / (musicPlayer.MusicLength * 1000);
            progresSlider.Value = progress;
        }

        void FixedUpdate()
        {
            if (!isPlaying)
            {
                return;
            }

            var cachedCurrentTime = timeWithOffset;

            var holdNotesLength = holdNotes.Count;
            for (int i = 0, j = 0; i < holdNotesLength - j; i++)
            {
                var note = holdNotes[i];
                // 刷新hold路径
                note.FixedUpdate(cachedCurrentTime);
            }
        }

        #endregion

        #region Private Method

        /// <summary>
        /// 初始化
        /// </summary>
        private void Initialize()
        {
            HalfNote.alphaCurve ??= alphaCurve;

            decorativeNotes ??= new List<HalfNote>();
            decorativeNotes.Clear();

            perfectEffectPool = new ObjectPool<GameObject>(5, CreatePerfectEffect, AfterReturnPool);
            BaseJudgeNote.effectPool = scaleEffectPool = new ObjectPool<UIFrameAnimation>(5, CreateScaleAnimationObject, AfterMonoComponentReturnPool);
            BaseJudgeNote.holdEffectPool = holdEffectPool = new ObjectPool<UIFrameAnimation>(5, CreateHoldEffect, AfterMonoComponentReturnPool);
        }

        /// <summary>
        /// 加载谱面
        /// </summary>
        private void LoadMap()
        {
            // 加载谱面
            MusicMap map = null;
            try
            {
                map = new MusicMap(EditingData.CurrentEditingMap);
            }
            catch (Exception)
            {
                throw;
            }

            // 按照实际的数量来初始化池子大小，目的是尽量避免运行中的实例化行为
            var judgeCount = map.judgeNotes.Sum(judgeNote => judgeNote.judges.Count);
            var halfNoteCount = map.decorativeNotes.Count + map.judgeNotes.Count * 2;
            HalfNote.notePool ??= new ObjectPool<Image>((int)(halfNoteCount / musicPlayer.MusicLength * 4), CreateHalfNote, halfNote => halfNote.gameObject.SetActive(false));
            var holdCount = map.judgeNotes.Count(judgeNote => judgeNote.judgeType == JudgeType.Hold);
            HalfNote.holdLinePool ??= new ObjectPool<HoldLineRenderer>(Mathf.Max(holdCount / 4, 5), CreateHoldPath, AfterHoldPathReturnPool);
            var judgePointCount = map.judgeNotes.Where(judgeNote => judgeNote.judgeType == JudgeType.Hold).Sum(judgeNote => judgeNote.judges.Count);
            BaseJudgeNote.holdJudgePointPool ??= new ObjectPool<GameObject>(Mathf.Max(judgePointCount / 4, 10), () => NoteFactory.CreateJudgePoint(HoldPathParent), AfterReturnPool);

            judgeQueue ??= new Queue<IJudgeNote>();
            judgeQueue.Clear();

            allDecorativeNoteQueue ??= new Queue<HalfNote>();
            allDecorativeNoteQueue.Clear();

            var decorativeNotes = new List<HalfNote>();

            // 装饰性半note
            foreach (var note in map.decorativeNotes)
            {
                var moveNote = NoteFactory.CreateDecorativeNote(note, SingleNoteParent);
                decorativeNotes.Add(moveNote);
            }

            decorativeNotes.Sort((left, right) => left.appearTime.CompareTo(right.appearTime));
            foreach (var decorative in decorativeNotes)
            {
                allDecorativeNoteQueue.Enqueue(decorative);
            }

            // 需要添加多押提示
            sortedJudgeNotes = map.judgeNotes.GroupBy(judgeNoteInfo => judgeNoteInfo.judges[0].judgeTime / 2 / 2)
                .SelectMany(groupByTime =>
                {
                    bool isMultiPress = groupByTime.Count() >= 2;
                    return groupByTime.Select(judgeInfo => CreateNote(judgeInfo, isMultiPress));
                }).ToList();

            // 排序，用于队列
            sortedJudgeNotes.Sort((left, right) => left.FirstHalfNoteStartTime.CompareTo(right.FirstHalfNoteStartTime));
            foreach (var judgeNote in sortedJudgeNotes)
            {
                judgeQueue.Enqueue(judgeNote);
            }

            // 初始化空间为20个，一般来讲应该够用吧？
            tapNotes ??= new List<NormalJudgeNote>(20);
            tapNotes.Clear();
            holdNotes ??= new List<HoldJudgeNote>(20);
            holdNotes.Clear();
            flickNotes ??= new List<NormalJudgeNote>(20);
            flickNotes.Clear();
            UpdateNotes();

            MusicScoreRecorder.Instance.SetScoreSetting(judgeCount);
        }

        private IJudgeNote CreateNote(JudgeNoteInfo judgeNoteInfo, bool isMultiPress = false)
        {
            IJudgeNote judgeNote = null;
            switch (judgeNoteInfo.judgeType)
            {
                case JudgeType.Tap:
                case JudgeType.Flick:
                    judgeNote = new NormalJudgeNote(judgeNoteInfo, isMultiPress);
                    break;
                case JudgeType.Hold:
                    judgeNote = new HoldJudgeNote(judgeNoteInfo, isMultiPress);
                    break;
            }

            return judgeNote;
        }

        /// <summary>
        /// 更新生效中的判定
        /// </summary>
        private void UpdateNotes()
        {
            var currentTime = MusicTime;
            while (judgeQueue.Count > 0)
            {
                var currentJudgeNote = judgeQueue.Peek();

                // 队列按时间入队，所以一旦队头超出时间就退出
                if (currentJudgeNote.FirstHalfNoteStartTime > currentTime + preShowTime)
                {
                    break;
                }

                judgeQueue.Dequeue();
                switch (currentJudgeNote.JudgeType)
                {
                    case JudgeType.Tap:
                        tapNotes.Add(currentJudgeNote as NormalJudgeNote);
                        break;
                    case JudgeType.Hold:
                        holdNotes.Add(currentJudgeNote as HoldJudgeNote);
                        break;
                    case JudgeType.Flick:
                        flickNotes.Add(currentJudgeNote as NormalJudgeNote);
                        break;
                }
            }

            while (allDecorativeNoteQueue.Count > 0)
            {
                var currentDecorativeNote = allDecorativeNoteQueue.Peek();
                if (currentDecorativeNote.appearTime > currentTime + preShowTime)
                {
                    break;
                }

                allDecorativeNoteQueue.Dequeue();
                decorativeNotes.Add(currentDecorativeNote);
            }
        }

        private void ShowPreview(bool show)
        {
            for (int i = 0; i < gamePreviewObjs.Length; i++)
            {
                gamePreviewObjs[i].SetActive(show);
            }
        }

        #endregion

        #region private pool method

        private GameObject CreatePerfectEffect()
        {
            var effect = JudgeEffectSetting.GetEffectPrefab(JudgeResult.Perfect);
            effect = Instantiate(effect, JudgeEffectParent);
            effect.transform.localScale = effect.transform.localScale*GlobalSettings.EffectSize*2;
            var anime = effect.GetComponent<UIFrameAnimationGroup>();
            anime.Initialize();
            anime.AddAnimationCallback(() => perfectEffectPool.ReturnToPool(effect));
            effect.SetActive(false);

            return effect;
        }

        private void AfterReturnPool(GameObject effect)
        {
            effect.SetActive(false);
        }

        private UIFrameAnimation CreateScaleAnimationObject()
        {
            var scaleAnimationGO = Instantiate(JudgeEffectSetting.ScaleCirclePrefab, JudgeEffectParent);
            scaleAnimationGO.transform.localScale *= GlobalSettings.EffectSize;
            var scaleAnimation = scaleAnimationGO.GetComponent<UIFrameAnimation>();
            scaleAnimation.AddAnimationCallback(() => scaleEffectPool.ReturnToPool(scaleAnimation));
            scaleAnimation.gameObject.SetActive(false);
            return scaleAnimation;
        }

        private void AfterMonoComponentReturnPool(MonoBehaviour effect)
        {
            effect.gameObject.SetActive(false);
        }

        private UIFrameAnimation CreateHoldEffect()
        {
            var effect = JudgeEffectSetting.HoldLoopEffectPrefab;
            effect = Instantiate(effect, JudgeEffectParent);
            var scale = effect.transform.localScale;
            effect.transform.localScale = scale * GlobalSettings.EffectSize;
            var anime = effect.GetComponent<UIFrameAnimation>();
            anime.Initialize();
            var effectRawScale = effect.transform.localScale;
            anime.AddOnForceLoopOverCallback(() =>
            {
                holdEffectPool.ReturnToPool(anime);
                effect.transform.localScale = effectRawScale;
            });
            effect.SetActive(false);

            return anime;
        }

        private HoldLineRenderer CreateHoldPath()
        {
            var holdPath = NoteFactory.CreateHoldPath(HoldPathParent);
            holdPath.gameObject.SetActive(false);
            return holdPath;
        }

        private void AfterHoldPathReturnPool(HoldLineRenderer holdPath)
        {
            holdPath.Initialize();
            holdPath.gameObject.SetActive(false);
        }

        private Image CreateHalfNote()
        {
            var halfNote = NoteFactory.CreareHalfNote(SingleNoteParent);
            // halfNote.transform.localScale *= PlayerGameSettings.Setting.NoteSize;
            halfNote.gameObject.SetActive(false);
            return halfNote;
        }

        #endregion

        #region Public Method

        /// <summary>
        /// 开始游玩时调用
        /// </summary>
        public void Play()
        {
            try
            {
                ClearNotes();
                ShowPreview(true);
                LoadMap();
                isPlaying = true;
                musicPlayer.PlayMusic();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Stop()
        {
            isPlaying = false;
            musicPlayer.StopMusic();
            ShowPreview(false);
        }

        public GameObject GetEffect(JudgeResult result)
        {
            switch (result)
            {
                case JudgeResult.Bad:
                case JudgeResult.Good:
                case JudgeResult.Perfect:
                case JudgeResult.PERFECT:
                    return perfectEffectPool.GetObject();
                case JudgeResult.None:
                case JudgeResult.Miss:
                default:
                    return null;
            }
        }

        /// <summary>
        /// 暂停
        /// </summary>
        public void OnSpaceButton(bool isPlaying)
        {
            // 因为在音乐播放管理里已经有暂停和播放事件了，所以这里管理一下运行的开关就行
            this.isPlaying = isPlaying && GamePreviewManager.Instance.IsGamePreviewOpen;
        }

        public void RefreshNotes()
        {
            var currentMusicTime = timeWithOffset;

            if (judgeQueue.Count < sortedJudgeNotes.Count)
            {
                judgeQueue.Clear();

                foreach (var judgeNote in sortedJudgeNotes)
                {
                    judgeNote.ResetJudge(currentMusicTime);
                    judgeQueue.Enqueue(judgeNote);
                }
            }
        }

        public void ClearNotes()
        {
            if (tapNotes != null)
            {
                tapNotes.ForEach(note => note.OnDestroy());
                tapNotes.Clear();
            }

            if (holdNotes != null)
            {
                holdNotes.ForEach(note => note.OnDestroy());
                holdNotes.Clear();
            }

            if (flickNotes != null)
            {
                flickNotes.ForEach(note => note.OnDestroy());
                flickNotes.Clear();
            }
        }

        #endregion

        #region Coroutine（协程）



        #endregion
    }
}
