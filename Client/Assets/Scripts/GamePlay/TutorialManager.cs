using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Resux.Data;
using Resux.Manager;
using Resux.Assets;
using Resux.Configuration;
using Resux.LevelData;

namespace Resux.UI.Manager
{
    /// <summary>
    /// 新手教程管理，是挂载在界面上的mono，无教程的场景下请勿调用
    /// 需要对教程本身的一些数据做操作请使用静态方法
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance;

        #region inner class/enum

        [System.Serializable]
        class TutorialInfo
        {
            [Tooltip("阶段")] public TutorialPhase phase;
            [Tooltip("起始时间，毫秒数")] public int startTime;
            [Header("本地化教程内容")] [Tooltip("教程文字")] public TutorialGroup[] tutorialMessages;
        }

        [System.Serializable]
        class TutorialGroup
        {
            [Tooltip("教程的持续显示时间，秒数")] public float messageContinueTime;
            public string message;
            public RectTransform ui;
        }

        /// <summary>教程阶段</summary>
        enum TutorialPhase
        {
            None,
            Tap,
            Hold,
            /// <summary>hold的进一步介绍</summary>
            HoldInfoFlick,
            /// <summary>双押</summary>
            DoubleTouch,
            PlayingUIPause,
            /// <summary>暂停的介绍</summary>
            /// <summary>最后的整合</summary>
            PauseInfoCombine,
            End
        }

        #endregion

        #region properties

        #region static

        public static bool IsForceEnableTutorial { get; private set; }

        #endregion

        [SerializeField] private GameObject tutorialArea;
        [SerializeField] private RectTransform highLightAreaRect;
        [SerializeField] private Text tutorialText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private GameObject safeArea;
        [Space]
        [Header("教程信息")]
        [SerializeField] private TutorialInfo[] tutorialInfos;

        private int currentTime => GamePlay.GamePlayer.Instance.MusicTime;
        private bool isPlaying => GamePlay.GamePlayer.Instance.IsPlaying;
        private int totalLength => (int) (GamePlay.MusicPlayer.Instance.GetMusicLength() * 1000 + 0.5f);

        private TutorialPhase currentPhase;
        private int nextTutirialIndex;
        private int firstPhaseTime;
        private bool isCutingIn;

        private Color leftColor;
        private Color rightColor;
        private bool isEnableTutorial => AccountManager.IsEnableTutorial && AccountManager.TutorialPhase <= Data.TutorialPhase.Playing;

        #endregion

        #region UnityEngine

        private void Awake()
        {
            Instance = this;
            Logger.Log($"isEnableTutorial: {isEnableTutorial}， IsForceEnableTutorial : {IsForceEnableTutorial}", Color.yellow);
            tutorialArea.SetActive(false);
            highLightAreaRect.gameObject.SetActive(false);

            if (isEnableTutorial || IsForceEnableTutorial)
            {
                this.enabled = true;
            }
            else
            {
                this.enabled = false;
            }
        }

        void Start()
        {
            // 无法输入换行，所以曲线救国一下
            for (int i = 0; i < tutorialInfos.Length; i++)
            {
                var tutorialInfo = tutorialInfos[i];
                for (int j = 0; j < tutorialInfo.tutorialMessages.Length; j++)
                {
                    var messageInfo = tutorialInfo.tutorialMessages[j];
                    messageInfo.message = messageInfo.message.Replace("\\n", "\n");
                }
            }

            firstPhaseTime = tutorialInfos[0].startTime;
            rightColor = backgroundImage.color;
            leftColor = new Color(1, 1, 1, 0);
        }

        void Update()
        {
            if (!isPlaying)
            {
                return;
            }

            if (nextTutirialIndex >= tutorialInfos.Length)
            {
                // 结束
                AccountManager.FinishTutorial();
                return;
            }

            if (!isCutingIn && currentTime > firstPhaseTime * 2 / 3)
            {
                isCutingIn = true;
                if (isEnableTutorial && AccountManager.TutorialPhase < Data.TutorialPhase.Playing)
                {
                    AccountManager.TutorialPhase = Data.TutorialPhase.Playing;
                }
                StartCoroutine(CutIn());
            }

            var tutorialInfo = tutorialInfos[nextTutirialIndex];
            if (currentPhase < tutorialInfo.phase)
            {
                // Debugger.Log($"phase ok: {currentPhase} < {tutorialInfo.phase} = {currentPhase < tutorialInfo.phase}, current time: {currentTime}, next start Time: {tutorialInfo.startTime}");
                if (tutorialInfo.startTime > 0)
                {
                    if (currentTime >= tutorialInfo.startTime)
                    {
                        currentPhase = tutorialInfo.phase;
                        StartCoroutine(ShowTutorial(tutorialInfo));
                    }
                }
                else
                {
                    if (currentTime - tutorialInfo.startTime > totalLength)
                    {
                        currentPhase = tutorialInfo.phase;
                        StartCoroutine(ShowTutorial(tutorialInfo));
                    }
                }
            }
        }

        private void OnDestroy()
        {
            
        }

        #endregion

        #region Public Method

        public static void CancelForcedTutorial()
        {
            IsForceEnableTutorial = false;
        }

        /// <summary>
        /// 跳过所有新手教程
        /// </summary>
        public static void JumpAllTutorialPhase()
        {
            AccountManager.FinishTutorial();
            PopupView.Instance.ShowUniversalWindow("已跳过所有新手教程");
        }

        #endregion

        #region Coroutine

        IEnumerator ShowTutorial(TutorialInfo tutorialInfo)
        {
            Logger.Log($"Current Tutorial Phase: {currentPhase}");
            tutorialArea.SetActive(true);
            for (int i = 0; i < tutorialInfo.tutorialMessages.Length; i++)
            {
                var messageGroup = tutorialInfo.tutorialMessages[i];
                tutorialText.text = messageGroup.message.Localize();
                if (messageGroup.ui != null)
                {
                    highLightAreaRect.pivot = messageGroup.ui.pivot;
                    highLightAreaRect.position = messageGroup.ui.position;
                    highLightAreaRect.sizeDelta = messageGroup.ui.rect.size;
                    highLightAreaRect.gameObject.SetActive(true);
                }else
                {
                    highLightAreaRect.gameObject.SetActive(false);
                }
                yield return new WaitForSeconds(messageGroup.messageContinueTime);
            }

            tutorialArea.SetActive(false);
            highLightAreaRect.gameObject.SetActive(false);
            nextTutirialIndex++;
        }

        IEnumerator CutIn()
        {
            safeArea.SetActive(false);
            highLightAreaRect.gameObject.SetActive(false);
            tutorialArea.SetActive(true);
            var totalContinueTime = firstPhaseTime / 3000f;
            var times = 30;
            var waitDeltaTime = new WaitForSeconds(totalContinueTime / times);
            for (int i = 0; i < times; i++)
            {
                backgroundImage.color = Color.Lerp(leftColor, rightColor, i / (float)times);
                yield return waitDeltaTime;
            }

            safeArea.SetActive(true);
            // highLightAreaRect.gameObject.SetActive(true);
        }

        #endregion

        #region Static

        public static void EnterTutorialPlay()
        {
            // 教程数据
            var groupId = 1000;
            LevelData.LevelDetail tutorialLevelDetail = new LevelData.LevelDetail()
            {
                _artistName = "Lax1u57",
                _songName = "Migration (tutorial)",
                _illustrationName = "megakite",
                _id = 53,
                _musicGroupId = groupId,
                _chartInfos = new ChartDetail[]
                {
                    new ChartDetail(){ _level = 500, _designer = "Kevin w/ J8rever53.exe" },
                }
            };
            GamePlay.MusicScoreSelection.Difficulty = Difficulty.Tale;
            GamePlay.MusicScoreSelection.LevelDetail = tutorialLevelDetail;
            IsForceEnableTutorial = !AccountManager.IsEnableTutorial;

            AudioLoader.LoadMusicBundle(groupId);
            MapLoader.LoadMapBundle(groupId);
            ImageLoader.LoadMusicCoverBundle(groupId);

            Navigator.JumpScene(GameScene.PlayScene);
        }

        #endregion
    }
}
