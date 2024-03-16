using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Resux.Manager;
using Resux.Data;
using UnityEngine.UI;
using Resux.UI;
using Resux.UI.Manager;
using Resux.Component;
using Resux.Configuration;
using Resux.GamePlay.Judge;
using Resux.LevelData;

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

        private Queue<IJudgeNote> judgeQueue;
        /// <summary>队列中提前入队的ms数</summary>
        private int preShowTime = 100;

        /// <summary>optimized - 需要判定的Note</summary>
        private FixedUnorderedList<BaseJudgeNote> inTimeRangeNotes = new FixedUnorderedList<BaseJudgeNote>(10);

        /// <summary>optimized - 需要持续判定的HoldNote</summary>
        private FixedUnorderedList<HoldJudgeNote> inTimeRangeHoldNotes = new FixedUnorderedList<HoldJudgeNote>(10);

        private ObjectPool<GameObject> earlyEffectPool;
        private ObjectPool<GameObject> lateEffectPool;
        private ObjectPool<GameObject> perfectEffectPool;
        private ObjectPool<GameObject> goodEffectPool;
        private ObjectPool<GameObject> badEffectPool;
        private ObjectPool<UIFrameAnimation> holdEffectPool;
        private ObjectPool<UIFrameAnimation> scaleEffectPool;

        #region MusicProperties

        /// <summary>是否在游玩中</summary>
        private bool isPlaying;
        public bool IsPlaying => isPlaying;

        /// <summary>
        /// 当前时间（指从歌开始）
        /// </summary>
        public int MusicTime => musicPlayer.GetTrackTime();

        /// <summary>
        /// 加上延迟的“实际时间”
        /// </summary>
        public int timeWithOffset => MusicTime - PlayerGameSettings.Setting.Offset;

        // 暂停开始回溯动画时获取到的时间与实际时间有着一定偏差，以暂停时最后一个time为准
        private int pauseTime = 0;

        /// <summary>歌曲id</summary>
        public int Id
        {
            get => MusicScoreSelection.MusicId;
        }

        public LevelData.LevelDetail LevelDetail => MusicScoreSelection.LevelDetail;

        /// <summary>谱面难度</summary>
        public Difficulty Diffculty
        {
            get => MusicScoreSelection.Difficulty;
        }

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
        /// 歌曲进度
        /// </summary>
        [SerializeField] private PlaySlider progresSlider;
        [SerializeField] private Button pauseButton;

        /// <summary>
        /// 音乐播放器
        /// </summary>
        public MusicPlayer musicPlayer
        {
            get => MusicPlayer.Instance;
        }

        #endregion

        #endregion

        #region UnityEngine

        void Awake()
        {
            JudgeEffectSetting.GetEffectPrefab(JudgeResult.PERFECT);

            Logger.Log("<color=red>Player Awake</color>");
        }

        void OnEnable()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            Logger.Log("<color=red>Player OnEnable</color>");
        }

        void Start()
        {
            AudioPlayManager.Instance.StopBGM();
            pauseButton.onClick.AddListener(Pause);
            isPlaying = false;
            progresSlider.Value = 0;
            PlayingSceneController.Instance.InitPausePopupView(Continue);
            Play();
            // MovingNotes = movingNotes.ToArray();
            Logger.Log("<color=red>Player Start</color>");
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
                    ProcessTimeState(note, note.OnUpdate(cachedCurrentTime));
                }

                if (note.IsInJudge)
                {
                    inTimeRangeHoldNotes.Insert(note);
                }
                else if (note.IsJudged)
                {
                    inTimeRangeHoldNotes.Remove(note);
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
                    ProcessTimeState(note, note.OnUpdate(cachedCurrentTime));
                }
            }

            // 装饰性半note的运动
            if (decorativeNotes.Any())
            {
                decorativeNotes.ForEach(note => note.Update(cachedCurrentTime));
                decorativeNotes.RemoveAll(halfNote => halfNote.collisionTime < cachedCurrentTime);
            }

            // 音频默认长度是s，转换成ms
            var progress = MusicTime / (musicPlayer.GetMusicLength() * 1000);
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

        void OnApplicationFocus(bool focus)
        {
            if (!focus && isPlaying)
            {
                Pause();
            }
        }

        #endregion

        #region Private Method

        /// <summary>
        /// 初始化
        /// </summary>
        private void Initialize()
        {
            HalfNote.alphaCurve = alphaCurve;

            if (decorativeNotes == null)
            {
                decorativeNotes = new List<HalfNote>();
            }
            decorativeNotes.Clear();

            earlyEffectPool = new ObjectPool<GameObject>(5, CreateEarlyEffect, AfterReturnPool);
            lateEffectPool = new ObjectPool<GameObject>(5, CreateLateEffect, AfterReturnPool);
            perfectEffectPool = new ObjectPool<GameObject>(5, CreatePerfectEffect, AfterReturnPool);
            goodEffectPool = new ObjectPool<GameObject>(5, CreateGoodEffect, AfterReturnPool);
            badEffectPool = new ObjectPool<GameObject>(5, CreateBadEffect, AfterReturnPool);
            BaseJudgeNote.effectPool = scaleEffectPool = new ObjectPool<UIFrameAnimation>(5, CreateScaleAnimationObject, AfterMonoComponentReturnPool);
            BaseJudgeNote.holdEffectPool = holdEffectPool = new ObjectPool<UIFrameAnimation>(5, CreateHoldEffect, AfterMonoComponentReturnPool);

            // 这里只是加载谱面，开始游戏在别的地方
            LoadMap();
        }

        /// <summary>
        /// 开始游玩时调用
        /// </summary>
        private void Play()
        {
            Initialize();
            PlayingSceneController.Instance.MusicCutIn(MusicScoreSelection.LevelDetail, MusicScoreSelection.Difficulty, () =>
            {
                SetInputListener();
                // 开启自动时禁用输入，否则启用
                TouchInputManager.Instance.SetEnable(!MusicScoreSelection.isAutoPlay);
                isPlaying = true;
                SystemTimeCounter.Instance.StartCount();
                musicPlayer.AddMusicEndListener(OnPlayEnd);
                PlayMusic();
            });
        }

        private BaseJudgeNote GetNearestNote(Vector2 pos, int time)
        {
            BaseJudgeNote result = null;
            float distance = -1;
            foreach (BaseJudgeNote judge in inTimeRangeNotes)
            {
                if (judge == null) continue;
                int spaceDistance = -1;
                var timeDistance = -1;
                if (judge.IsJudged)
                {
                    continue;
                }
                var judgePos = judge.FirstJudgePos;
                spaceDistance = Convert.ToInt32(Vector2.Distance(pos, judgePos));
                timeDistance = Math.Abs(judge.FirstJudgeTime - time);

                // 加权平均
                var newDistance = (spaceDistance + 0.4f * timeDistance * 1.6f) / 2f;
                if (newDistance < distance || distance == -1)
                {
                    distance = newDistance;
                    result = judge;
                }
            }
            return result;
        }

        private HoldJudgeNote GetNearestHoldNote(Vector2 pos)
        {
            HoldJudgeNote result = null;
            float distance = -1;
            foreach (HoldJudgeNote judge in inTimeRangeHoldNotes)
            {
                if (judge == null) continue;
                int spaceDistance = -1;
                if (judge.IsJudged)
                {
                    continue;
                }
                var judgePos = judge.FirstJudgePos;
                spaceDistance = Convert.ToInt32(Vector2.Distance(pos, judgePos));

                // 加权平均
                var newDistance = spaceDistance;
                if (newDistance < distance || distance == -1)
                {
                    distance = newDistance;
                    result = judge;
                }
            }

            return result;
        }

        private void ProcessTimeState(BaseJudgeNote note, NoteTimeState state)
        {
            if (state == NoteTimeState.Enter)
                inTimeRangeNotes.Insert(note);
            else if (state == NoteTimeState.Leave)
                inTimeRangeNotes.Remove(note);
        }

        /// <summary>
        /// 设置输入监听
        /// </summary>
        private void SetInputListener()
        {
            // 添加判定事件监听
            var input = TouchInputManager.Instance;
            input.AddBeginTouchListener(touch =>
            {
                // 这里不再需要持续向HoldNote发送触摸点信息进行判定了，只需要发送一次开始判定即可
                // 传入HoldNote后保存这个触摸点信息，因为内存引用的原因，在里面是可以持续获得触摸信息更新的
                if (!touch.IsUsed)
                {
                    // 先用于未判定Note的判定
                    var judge = GetNearestNote(touch.Pos, timeWithOffset);
                    bool judgeSucceeded = false;
                    if (judge is NormalJudgeNote judgeNote)
                    {
                        judgeSucceeded = judgeNote.Judge(touch);
                    }
                    else if (judge is HoldJudgeNote holdJudge)
                    {
                        judgeSucceeded = holdJudge.Judge(touch);
                    }
                    touch.IsUsed = true;
                    if (judgeSucceeded)
                    {
                        inTimeRangeNotes.Remove(judge);
                        return;
                    }
                }
            });
            input.AddTouchMovingListener(touch =>
            {
                foreach (var flickNote in flickNotes)
                {
                    if (flickNote.Judge(touch)) break;
                }
            });
            input.AddCoutinueTouchListener(touch =>
            {
                var holdJudge = GetNearestHoldNote(touch.Pos);
                if (holdJudge!= null && holdJudge.Judge(touch))
                {
                    inTimeRangeHoldNotes.Remove(holdJudge);
                }
            });
        }

        /// <summary>
        /// 加载谱面
        /// </summary>
        private void LoadMap()
        {
            // 设置音乐
            // 空也没关系，所以写在前面便于debug
            musicPlayer.SetMusic(LevelDetail);
            // 加载谱面
            var map = Assets.MapLoader.GetMusicMap(LevelDetail, Diffculty);
            if (map == null)
            {
                Logger.LogError($"map note exist: id-{Id}, difficulty-{Diffculty}");
                return;
            }
            Logger.Log($"map: diff-{map.difficulty} level-{map.diffLevel}");

            // 按照实际的数量来初始化池子大小，目的是尽量避免运行中的实例化行为
            var judgeCount = map.judgeNotes.Sum(judgeNote => judgeNote.judges.Count);
            var halfNoteCount = map.decorativeNotes.Count + map.judgeNotes.Count * 2;
            HalfNote.notePool = new ObjectPool<SpriteRenderer>((int)(halfNoteCount / musicPlayer.GetMusicLength() * 4), CreateHalfNote, halfNote => halfNote.gameObject.SetActive(false));
            var holdCount = map.judgeNotes.Count(judgeNote => judgeNote.judgeType == JudgeType.Hold);
            HalfNote.holdLinePool = new ObjectPool<HoldLineRenderer>(Mathf.Max(holdCount / 4, 5), CreateHoldPath, AfterHoldPathReturnPool);
            var judgePointCount = map.judgeNotes.Where(judgeNote => judgeNote.judgeType == JudgeType.Hold).Sum(judgeNote => judgeNote.judges.Count);
            BaseJudgeNote.holdJudgePointPool = new ObjectPool<GameObject>(Mathf.Max(judgePointCount / 4, 10), () => NoteFactory.CreateJudgePoint(SingleNoteParent), AfterReturnPool);

            judgeQueue = new Queue<IJudgeNote>();
            allDecorativeNoteQueue = new Queue<HalfNote>();
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
            var allJudgeNotes = map.judgeNotes.GroupBy(judgeNoteInfo => judgeNoteInfo.judges[0].judgeTime / 2 / 2)
                .SelectMany(groupByTime =>
                {
                    // Logger.Log($"group judges: {Utils.ConvertToStrBySplit(groupByTime.Select(judge => judge.judges[0].judgeTime))}", Color.yellow);
                    bool isMultiPress = groupByTime.Count() >= 2;
                    return groupByTime.Select(judgeInfo => CreateNote(judgeInfo, isMultiPress));
                }).ToList();

            // 排序，用于队列
            allJudgeNotes.Sort((left, right) => left.FirstHalfNoteStartTime.CompareTo(right.FirstHalfNoteStartTime));
            foreach (var judgeNote in allJudgeNotes)
            {
                judgeQueue.Enqueue(judgeNote);
            }

            // 初始化空间为20个，一般来讲应该够用吧？
            tapNotes = new List<NormalJudgeNote>(20);
            holdNotes = new List<HoldJudgeNote>(20);
            flickNotes = new List<NormalJudgeNote>(20);
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

        /// <summary>
        /// 播放音乐
        /// </summary>
        private void PlayMusic()
        {
            musicPlayer.PlayMusic();
        }

        /// <summary>
        /// 歌打完的时候调用
        /// </summary>
        private async void OnPlayEnd()
        {
            Logger.Log("Play End");
            TouchInputManager.Instance.Disable();
            TouchInputManager.Instance.RemoveAllListeners();
            musicPlayer.Stop();
            isPlaying = false;
            SystemTimeCounter.Instance.Pause();
            MusicScoreRecorder.Instance.SetBannedState(SystemTimeCounter.Instance.IsBanned);
            MusicScoreRecorder.Instance.RemoveListeners();

            var res = MusicScoreRecorder.Instance.GetScoreResult();

            // 如果是教程那就取消教程，不是教程且没被ban就推送成绩
            if (AccountManager.IsEnableTutorial || TutorialManager.IsForceEnableTutorial)
            {
                AccountManager.TutorialPhase = TutorialPhase.End;
                TutorialManager.CancelForcedTutorial();
            } else if (!MusicScoreRecorder.Instance.IsBanned)
            {
                var record = MusicScoreRecorder.Instance.GetResultRecord();
                (var isNewBest, var diff) = PlayerRecordManager.SetRecord(Id, Diffculty, record);
                ResultSceneController.Data.ResultRecord = record;
                ResultSceneController.Data.IsNewBest = isNewBest;
                ResultSceneController.Data.ScoreDifference = diff;
            }

            // 显示特效
            PlayingSceneController.Instance.ShowResultEffect(res);

            CoroutineUtils.RunDelay(GotoNextScene, 1f);
        }

        private void GotoNextScene()
        {
            Logger.Log("Go To ResultScene");
            pauseButton.gameObject.SetActive(false);
            PlayingSceneController.Instance.OnPlayEnd();
            if (TutorialManager.Instance.enabled)
            {
                if (AccountManager.IsEnableTutorial)
                {
                    AccountManager.TutorialPhase = TutorialPhase.End;
                }

                // Assets.AudioLoader.UnloadMusicBundle();
                // Assets.MapLoader.UnloadMapBundle();
                // Assets.ImageLoader.UnloadMusicCoverBundle();
                Logger.Log("Jump to GameScene Directly");

                Navigator.JumpScene(GameScene.MainMenuScene);
            }
            else
            {
                Navigator.JumpScene(GameScene.ResultScene, true);
            }
        }

        // private void OnPushScoreResultSuccess(PushScoreResultResponse response, ResultRecordParameter record)
        // {
        //     if (response.IsNewBest)
        //     {
        //         PlayerRecordManager.SetRecord(Id, Diffculty, record);
        //     }
        // }

        #endregion

        #region private pool method

        private GameObject CreatePerfectEffect()
        {
            var effect = JudgeEffectSetting.GetEffectPrefab(JudgeResult.Perfect);
            effect = Instantiate(effect, JudgeEffectParent);
            effect.transform.localScale *= PlayerGameSettings.Setting.EffectSize;
            var anime = effect.GetComponent<UIFrameAnimationGroup>();
            anime.Initialize();
            anime.AddAnimationCallback(() => perfectEffectPool.ReturnToPool(effect));
            effect.SetActive(false);

            return effect;
        }

        private GameObject CreateEarlyEffect()
        {
            var effect = JudgeEffectSetting.GetELPrefab(PerfectType.Early);
            effect = Instantiate(effect, JudgeEffectParent);
            effect.transform.localScale *= PlayerGameSettings.Setting.EffectSize;
            var anime = effect.GetComponent<UIFrameAnimation>();
            anime.Initialize();
            anime.AddAnimationCallback(() => earlyEffectPool.ReturnToPool(effect));
            effect.SetActive(false);

            return effect;
        }

        private GameObject CreateLateEffect()
        {
            var effect = JudgeEffectSetting.GetELPrefab(PerfectType.Late);
            effect = Instantiate(effect, JudgeEffectParent);
            effect.transform.localScale *= PlayerGameSettings.Setting.EffectSize;
            var anime = effect.GetComponent<UIFrameAnimation>();
            anime.Initialize();
            anime.AddAnimationCallback(() => lateEffectPool.ReturnToPool(effect));
            effect.SetActive(false);

            return effect;
        }

        private GameObject CreateGoodEffect()
        {
            var effect = JudgeEffectSetting.GetEffectPrefab(JudgeResult.Good);
            effect = Instantiate(effect, JudgeEffectParent);
            effect.transform.localScale *= PlayerGameSettings.Setting.EffectSize;
            var anime = effect.GetComponent<UIFrameAnimationGroup>();
            anime.Initialize();
            anime.AddAnimationCallback(() => goodEffectPool.ReturnToPool(effect));
            effect.SetActive(false);

            return effect;
        }

        private GameObject CreateBadEffect()
        {
            var effect = JudgeEffectSetting.GetEffectPrefab(JudgeResult.Bad);
            effect = Instantiate(effect, JudgeEffectParent);
            effect.transform.localScale *= PlayerGameSettings.Setting.EffectSize;
            var anime = effect.GetComponent<UIFrameAnimationGroup>();
            anime.Initialize();
            anime.AddAnimationCallback(() => badEffectPool.ReturnToPool(effect));
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
            scaleAnimationGO.transform.localScale *= PlayerGameSettings.Setting.EffectSize;
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
            // Debugger.Log($"hold effect scale:{scale}", Color.yellow);
            effect.transform.localScale = scale * PlayerGameSettings.Setting.EffectSize;
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
            var holdPath = NoteFactory.CreateHoldPath(SingleNoteParent);
            holdPath.gameObject.SetActive(false);
            return holdPath;
        }

        private void AfterHoldPathReturnPool(HoldLineRenderer holdPath)
        {
            holdPath.Initialize();
            holdPath.gameObject.SetActive(false);
        }

        private SpriteRenderer CreateHalfNote()
        {
            var halfNote = NoteFactory.CreareHalfNote(SingleNoteParent);
            halfNote.transform.localScale *= PlayerGameSettings.Setting.NoteSize;
            halfNote.gameObject.SetActive(false);
            return halfNote;
        }

        #endregion

        #region Public Method

        public GameObject GetEffect(JudgeResult result)
        {
            switch (result)
            {
                case JudgeResult.Bad:
                    return badEffectPool.GetObject();
                case JudgeResult.Good:
                    return goodEffectPool.GetObject();
                case JudgeResult.Perfect:
                case JudgeResult.PERFECT:
                    return perfectEffectPool.GetObject();
                case JudgeResult.None:
                case JudgeResult.Miss:
                default:
                    return null;
            }
        }

        public GameObject GetELEffect(PerfectType perfectType)
        {
            switch (perfectType)
            {
                case PerfectType.Early:
                    return earlyEffectPool.GetObject();
                case PerfectType.Late:
                    return lateEffectPool.GetObject();
                case PerfectType.Just:
                case PerfectType.None:
                default:
                    return null;
            }
        }

        /// <summary>
        /// 暂停
        /// </summary>
        public void Pause()
        {
            if (!isPlaying)
            {
                return;
            }

            isPlaying = false;
            pauseTime = MusicTime;
            SystemTimeCounter.Instance.Pause();
            musicPlayer.PauseMusic();
            StopAllCoroutines();
            PlayingSceneController.Instance.ShowPausePopupView();
        }

        /// <summary>
        /// 继续游玩
        /// </summary>
        public void Continue()
        {
            PlayingSceneController.Instance.HidePausePopupView();
            // 启用协程来回溯
            StartCoroutine(Reverse(3000));
        }

        #endregion

        #region Coroutine（协程）

        /// <summary>
        /// （未判定的）谱面回溯
        /// </summary>
        /// <param name="ms">时长（ms）</param>
        IEnumerator Reverse(int ms)
        {
            // 实际的回溯时长
            var realReverseTime = ms;
            // 当前的音乐时间
            var currentMusicTime = pauseTime;
            // 如果当前歌曲进度小于3秒
            if (currentMusicTime < ms)
            {
                realReverseTime = currentMusicTime;
            }
            // 开始回溯
            var deltaTime = 10;
            var waitSecond = new WaitForSeconds(0.01f);
            yield return null;
            
            // var lengthOfTap = tapNotes.Count;
            // var lengthOfFlick = flickNotes.Count;
            // var lengthOfHold = holdNotes.Count;
            for (int i = 0; i < realReverseTime; i += deltaTime)
            {
                currentMusicTime -= deltaTime;

                // 使用for代替itor
                var currTime = currentMusicTime - PlayerGameSettings.Setting.Offset;
                foreach (var note in tapNotes)
                {
                    note.OnUpdate(currTime);
                }

                foreach (var note in flickNotes)
                {
                    note.OnUpdate(currTime);
                }

                foreach (var note in holdNotes)
                {
                    note.OnUpdate(currTime);
                }

                // 处理进度条
                var progress = currentMusicTime / (musicPlayer.GetMusicLength() * 1000);
                progresSlider.Value = progress;
                yield return waitSecond;
            }

            musicPlayer.ContinueMusic(realReverseTime / 1000f);
            
            var endCurrTime = currentMusicTime - PlayerGameSettings.Setting.Offset;
            
            SystemTimeCounter.Instance.StartCount(endCurrTime);

            yield return new WaitForEndOfFrame();

            isPlaying = true;
        }

        #endregion
    }
}
