using System.Collections;
using System.Collections.Generic;
using Resux.Configuration;
using UnityEngine;
using UnityEngine.UI;
using Resux.UI;
using Resux.Manager;

namespace Resux.UI.Manager
{
    /// <summary>
    /// 根场景
    /// </summary>
    public class MainMenuSceneController : MonoBehaviour
    {
        #region properties

        private GameScene thisScene = GameScene.MainMenuScene;

        #region Scene Object

        /// <summary>
        /// 选歌场景入口（自由模式）
        /// </summary>
        [SerializeField] private Button mainSceneEntry;
        [SerializeField] private Text mainSceneText;

        /// <summary>
        /// 剧情模式场景入口
        /// </summary>
        [SerializeField] private Button storySceneEntry;
        [SerializeField] private Text storySceneText;

        /// <summary>
        /// 设置场景入口
        /// </summary>
        [SerializeField] private Button settingSceneEntry;
        [SerializeField] private Text settingSceneText;

        /// <summary>
        /// 切换CG
        /// </summary>
        [SerializeField] private Button cgSwitchButton;
        [SerializeField] private Text cgSwitchText;

        /// <summary>
        /// 商店场景入口
        /// </summary>
        [SerializeField] private Button shopSceneEntry;

        /// <summary>
        /// 个人信息场景入口
        /// </summary>
        [SerializeField] private Button playerInfoSceneEntry;

        /// <summary>
        /// 成就场景入口
        /// </summary>
        [SerializeField] private Button achievementSceneEntry;

        [Space]
        [SerializeField] private PlayerInfoView playerInfoView;
        [SerializeField] private Animator cameraAnimator;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private AudioListener audioListener;
        [SerializeField] private CameraGyroscope cameraGyroscope;

        #endregion

        #endregion

        void Start()
        {
            Initialize();
        }

        #region Private Method

        private void Initialize()
        {
            InitListener();
            playerInfoView.Initialize(Data.AccountManager.User);
            AudioPlayManager.Instance.PlayBGM(Data.Sounds.Bgm.RootScene);
        }

        private void InitListener()
        {
            mainSceneEntry.onClick.AddListener(OnMusicGroupSceneClick);
            storySceneEntry.onClick.AddListener(OnStorySceneClick);
            settingSceneEntry.onClick.AddListener(OnSettingClick);
            shopSceneEntry.onClick.AddListener(OnShopClick);
            playerInfoSceneEntry.onClick.AddListener(OnPlayerInfoClick);
            achievementSceneEntry.onClick.AddListener(OnAchievementClick);
            cgSwitchButton.onClick.AddListener(OnSwitchCG);
        }

        private void OnMusicGroupSceneClick()
        {
            StartCoroutine(OnSceneLoad(tex2D =>
            {
                ChapterListSceneController.Data.BackgroundTex = tex2D;
                Navigator.LoadNewScene(GameScene.ChapterListScene);
            }));
        }

        private void OnStorySceneClick()
        {
            PopupView.Instance.ShowUniversalWindow("此次连山归藏测试不开放此功能，敬请期待！", PopupType.Warn);
            // SceneSwitchManager.LoadScene(GameScene.StoryScene, ThisScene);
        }

        private void OnSettingClick()
        {
            Navigator.LoadNewScene(GameScene.SettingScene);
        }

        private void OnMapSceneClick()
        {
            PopupView.Instance.ShowUniversalWindow("此次连山归藏测试不开放此功能，敬请期待！", PopupType.Warn);
        }

        private void OnShopClick()
        {
            // TODO: 商店界面
            PopupView.Instance.ShowUniversalWindow("此次连山归藏测试不开放此功能，敬请期待！", PopupType.Warn);
        }

        private void OnPlayerInfoClick()
        {
            PopupView.Instance.ShowUniversalWindow("此次连山归藏测试不开放此功能，敬请期待！", PopupType.Warn);
            // SceneSwitchManager.LoadScene(GameScene.PlayerInfoScene, ThisScene);
        }

        private void OnAchievementClick()
        {
            // TODO: 成就和收藏界面
            PopupView.Instance.ShowUniversalWindow("此次连山归藏测试不开放此功能，敬请期待！", PopupType.Warn);
        }

        private void OnSwitchCG()
        {
            // TODO: 切换CG和对应的透视
            PopupView.Instance.ShowUniversalWindow("此次连山归藏测试不开放此功能，敬请期待！", PopupType.Warn);
        }

        #endregion

        #region Coroutine

        /// <summary>
        /// 叠加加载其他场景时调用
        /// </summary>
        IEnumerator OnSceneLoad(System.Action<Texture2D> onLoad)
        {
            // audioListener.enabled = false;
            var wait = new WaitForSeconds(0.02f);
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            for (int i = 1; i <= 10; i++)
            {
                canvasGroup.alpha = 1 - i / 10f;
                yield return wait;
            }

            yield return new WaitForEndOfFrame();

            onLoad?.Invoke(ScreenCapture.CaptureScreenshotAsTexture());
        }

        /// <summary>
        /// 叠加加载其他场景时调用
        /// </summary>
        IEnumerator OnSceneUnLoad()
        {
            // audioListener.enabled = true;
            var wait = new WaitForSeconds(0.02f);
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            for (int i = 1; i <= 10; i++)
            {
                canvasGroup.alpha = i / 10f;
                yield return wait;
            }
        }

        #endregion
    }
}
