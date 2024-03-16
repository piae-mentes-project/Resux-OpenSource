using System;
using System.Collections;
using System.Threading.Tasks;
using Resux.Configuration;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Resux.UI;
using Resux.Data;
using Resux.GamePlay;
using Resux.LevelData;
using Resux.UI.Component.Effect;
using Resux.Manager;

namespace Resux.UI.Manager
{
    public class PlayingSceneController : MonoBehaviour
    {
        #region properties

        public static GameScene ThisScene => GameScene.PlayScene;

        #region Singleton

        public static PlayingSceneController Instance;

        #endregion

        #region 歌信息切入

        [SerializeField] private GameObject musicCutInUI;
        [SerializeField] private RawImage musicCover;
        [SerializeField] private Text musicName;
        [SerializeField] private Text musicComposer;
        [SerializeField] private Text levelDesigner;
        [SerializeField] private Text illustration;
        [SerializeField] private Image maskUI;
        /// <summary>曲绘背景</summary>
        [SerializeField] private RawImage musicCoverBg;
        /// <summary>底部的曲名</summary>
        [SerializeField] private Text musicNameBottom;
        /// <summary>底部的难度名</summary>
        [SerializeField] private Text difficultyName;

        #endregion

        #region Play Message

        [SerializeField] private Text scoreText;
        [SerializeField] private Text comboText;
        [SerializeField] private Image fcMark;
        [SerializeField] private Image apMark;
        /// <summary>最后结算特效（fc、ap）</summary>
        [SerializeField] private Transform resultEffectParent;
        [SerializeField] private GameObject comboLabel;

        #endregion

        [SerializeField] private Camera gameCamera;
        [SerializeField] private RawImage gameRawImage;
        [SerializeField] private RectTransform safeAreaTransform;
        [SerializeField] private Color leftColor;
        [SerializeField] private Color rightColor;
        [SerializeField] private Color scoreFrontColor = new Color(1, 1, 1, 0.25f);
        [SerializeField] private Color scoreBackColor = Color.white;

        /// <summary>
        /// 暂停弹窗
        /// </summary>
        private PausePopupView pausePopupView;

        #endregion

        #region UnityEngine

        void Awake()
        {
            Instance = this;
            MusicScoreRecorder.Instance.Reset();
            // 添加对成绩变动的监听
            MusicScoreRecorder.Instance.AddScoreChangedListener(
                onScoreChanged: (combo, totalScore, isFc, isAp) =>
                {
                    scoreText.text = Utils.GetScoreText(totalScore, scoreFrontColor, scoreBackColor);
                    comboText.text = combo.ToString();
                    fcMark.color = new Color(1, 1, 1, isFc ? 1 : 0f);
                    apMark.color = new Color(1, 1, 1, isAp ? 1 : 0f);
                });
            musicCoverBg.color = Color.white * PlayerGameSettings.Setting.CoverBrightness;
        }

        void Start()
        {
            Initialize();
        }

        void OnDestroy()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        #endregion

        #region Public Method

        public void MusicCutIn(LevelData.LevelDetail config, Difficulty difficulty, Action callback)
        {
            var cover = Assets.ImageLoader.GetMusicCover(config?._songName);
            musicCover.texture = cover;
            musicCoverBg.texture = musicCover.texture;
            var songName = config?._songName;
            musicName.text = songName;
            musicNameBottom.text = $" {songName} ";
            musicComposer.text = $"{"Music".Localize()}: {config?._artistName}";
            illustration.text = $"{"Illustration".Localize()}: {config?._illustrationName}";
            levelDesigner.text = $"{"Level Designer".Localize()}: {config?.GetLevelDesigner(difficulty)}";

            musicCoverBg.GetComponent<UIImageAutoScale>().AutoScale();

            difficultyName.text = $" [[{difficulty.GetName()}]] {config.GetLevelStr(difficulty)} ";
            difficultyName.gameObject.SetActive(!TutorialManager.Instance.enabled);

            StartCoroutine(MusicInfoCutIn(callback));
        }

        public void InitPausePopupView(UnityAction @continue)
        {
            // 如果有旧的 先销毁，1是摧毁垃圾，2是防止闭包
            DestroyPausePopupView();
            pausePopupView = PopupView.Instance.ShowSpecialWindow<PausePopupView>("PlayPausePopupPanel");
            pausePopupView.Initialize(Exit, @continue, ReLoad);
            pausePopupView.gameObject.SetActive(false);
        }

        public void ShowPausePopupView()
        {
            StartCoroutine(WaitUntilFrameEnd(() =>
            {
                var bg = ScreenCapture.CaptureScreenshotAsTexture();
                pausePopupView.gameObject.SetActive(true);
                pausePopupView.ShowBlurBackground(bg);
            }));
        }

        public void HidePausePopupView()
        {
            pausePopupView.gameObject.SetActive(false);
        }

        public void DestroyPausePopupView()
        {
            if (pausePopupView)
            {
                Destroy(pausePopupView.gameObject);
            }
        }

        public void OnPlayEnd()
        {
            // 虽然应该有动画，但是先不做动画了
            // 游玩信息
            comboText.gameObject.SetActive(false);
            scoreText.gameObject.SetActive(false);
            fcMark.gameObject.SetActive(false);
            apMark.gameObject.SetActive(false);

            // 曲信息
            musicNameBottom.gameObject.SetActive(false);
            difficultyName.gameObject.SetActive(false);

            // 其他
            resultEffectParent.gameObject.SetActive(false);
            comboLabel.SetActive(false);

            // 音频收听
            gameCamera.GetComponent<AudioListener>().enabled = false;
        }

        public void ShowResultEffect(ScoreType score)
        {
            var resEffect = JudgeEffectSetting.GetResultEffectPrefab(score);
            if (resEffect != null)
            {
                resultEffectParent.gameObject.SetActive(true);
                GameObject.Instantiate(resEffect, resultEffectParent);
            }
        }

        #endregion

        #region Private Method

        private void Initialize()
        {
            scoreText.text = Utils.GetScoreText(0, scoreFrontColor, scoreBackColor);
            comboText.text = "0";
            musicCutInUI.SetActive(false);

            // 初始化
            var (width, height) = ScreenAdaptation.ScaledSize;
            // var (width, height) = ScreenAdaptation.StandardSize;

            RenderTexture rt = new RenderTexture(width, height, 1);
            gameCamera.targetTexture = rt;
            gameRawImage.texture = rt;
            // gameRawImage.rectTransform.sizeDelta = new Vector2(width, height);
            // gameRawImage.rectTransform.position = safeAreaTransform.position;

            // 保持不熄屏状态
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        private void Exit()
        {
            DestroyPausePopupView();
            Logger.Log("Exit PlayScene");
            MusicScoreRecorder.Instance.Reset();
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
            if (AccountManager.IsEnableTutorial || TutorialManager.IsForceEnableTutorial)
            {
                if (TutorialManager.IsForceEnableTutorial)
                {
                    TutorialManager.CancelForcedTutorial();
                }

                // Assets.AudioLoader.UnloadMusicBundle();
                // Assets.MapLoader.UnloadMapBundle();
                // Assets.ImageLoader.UnloadMusicCoverBundle();

                Navigator.ClearEscapeStack();
                Navigator.JumpScene(GameScene.MainMenuScene);
            }
            else
            {
                TutorialManager.CancelForcedTutorial();
                Navigator.JumpScene(GameScene.MusicListScene);
            }
        }

        private void ReLoad()
        {
            Logger.Log("ReLoad PlayScene");
            DestroyPausePopupView();
            MusicScoreRecorder.Instance.Reset();
            Navigator.JumpScene(ThisScene);
        }

        #endregion

        #region 协程Coroutine

        IEnumerator MusicInfoCutIn(Action callback)
        {
            var startTime = DateTime.Now;
            var times = 30;
            var waitFixedUpdate = new WaitForFixedUpdate();

            var blur = musicCoverBg.GetComponent<ImageBlurGPU>();
            Logger.Log("Rate " + PlayerGameSettings.Setting.CoverBlurRate.ToString());
            var newCurve = new AnimationCurve();
            
            for (int i = 0; i < blur.sizeWithIterationCurve.keys.Length; i++)
            {
                newCurve.AddKey(new Keyframe(blur.sizeWithIterationCurve.keys[i].time * PlayerGameSettings.Setting.CoverBlurRate, blur.sizeWithIterationCurve.keys[i].value * PlayerGameSettings.Setting.CoverBlurRate,
                    blur.sizeWithIterationCurve.keys[i].inTangent, blur.sizeWithIterationCurve.keys[i].outTangent,
                    blur.sizeWithIterationCurve.keys[i].inWeight, blur.sizeWithIterationCurve.keys[i].outWeight));
            }
            blur.sizeWithIterationCurve = newCurve;
            var enableBlur = newCurve.keys[newCurve.length - 1].value > 1;
            if (musicCoverBg.texture != null && PlayerGameSettings.Setting.CoverBrightness != 0f && enableBlur) { blur.StartBlur(true); }

            for (int i = 0; i < times; i++)
            {
                maskUI.color = Color.Lerp(leftColor, rightColor, i / (float)times);
                yield return waitFixedUpdate;
            }
            musicCutInUI.SetActive(true);
            yield return blur.WaitBlurDone();
            var showTime = TimeSpan.FromSeconds(2.5);
            var endTime = DateTime.Now - startTime;
            if (endTime < showTime) // 至少Loading 3秒（
            {
                yield return new WaitForSeconds(Convert.ToSingle((showTime - endTime).TotalSeconds));
            }
            StartCoroutine(MusicInfoCutOut(callback));
        }

        IEnumerator MusicInfoCutOut(Action callback)
        {
            var times = 30;
            var waitFixedUpdate = new WaitForFixedUpdate();

            musicCutInUI.SetActive(false);
            for (int i = 0; i < times; i++)
            {
                maskUI.color = Color.Lerp(rightColor, leftColor, i / (float)times);
                yield return waitFixedUpdate;
            }
            maskUI.gameObject.SetActive(false);

            callback();
        }

        IEnumerator WaitUntilFrameEnd(Action callback)
        {
            yield return new WaitForEndOfFrame();
            callback();
        }

        #endregion
    }
}