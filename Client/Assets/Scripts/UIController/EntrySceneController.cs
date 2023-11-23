using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using IngameDebugConsole;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif
using Resux.Data;
using Resux.Configuration;
using Resux.Manager;

namespace Resux.UI.Manager
{
    public class EntrySceneController : MonoBehaviour
    {
        #region properties

        public static GameScene ThisScene => GameScene.EntryScene;

        private bool isFirstPlay;

        #region SceneObject

        [SerializeField] private Button entryButton;
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private GameObject accountBar;
        [SerializeField] private Image avatar;
        [SerializeField] private Text loginMessage;
        [SerializeField] private Text touchToStartText;
        [SerializeField] private GameObject topRightBtns;
        [SerializeField] private Text version;
        [SerializeField] private RawImage videoImage;
        [SerializeField] private Image splashImage;

        #region Debug

        [SerializeField] private GameObject debugArea;
        [SerializeField] private Button jumpToturialBtn;

        #endregion

        #endregion

        #region ShowContent

        /// <summary>开场的视频过场列表</summary>
        [SerializeField] private List<VideoClip> videoClips = new List<VideoClip>();
        /// <summary>开场的图片过场列表</summary>
        [SerializeField] private List<Sprite> splashImages = new List<Sprite>();

        /// <summary>当前视频索引</summary>
        private int currentVideoIndex;

        #endregion

        #endregion

        #region UnityEngine

        void Start()
        {
            AssetBundle.SetAssetBundleDecryptKey(ConstConfigs.GetKey());
            // 申请权限
            RequestPermission();

            // 初始化debugUI
            InitDebugUI();

            // 设置帧率限制144（应该也是可以让60的设备达到60的）
            Application.targetFrameRate = 144;
            isFirstPlay = UserLocalSettings.GetInt(ConstConfigs.LocalKey.IsFirstPlay, 0) == 0;

            AudioPlayManager.Init();

            currentVideoIndex = 0;
            // 一个视频播完后自动触发点击效果，播放下一个或消失
            videoPlayer.loopPointReached += player => OnVideoEnd();
            touchToStartText.gameObject.SetActive(false);
            splashImage.gameObject.SetActive(true);
            videoImage.gameObject.SetActive(true);
            topRightBtns.SetActive(false);
            StartCoroutine(SplashImageAnimation(() =>
            {
                OnVideoEnd();
                Init();
            }));
            GameConfigs.AdaptationJudgeDistance(Mathf.Min(ScreenAdaptation.WidthScale, ScreenAdaptation.HeightScale));

            // 本地化
            touchToStartText.text = $"- {"TOUCH_START".Localize()} -";
        }

        #endregion

        #region Private Method

        private void InitDebugUI()
        {
            debugArea.SetActive(GameConfigs.GameBaseSetting.debugMode);
            jumpToturialBtn.onClick.AddListener(TutorialManager.JumpAllTutorialPhase);
            DebugLogConsole.AddCommand("throw_exception", "抛出异常", () =>
            {
                throw new Exception("test exception");
            });
            DebugLogConsole.AddCommand("return_scene", "返回上一个场景", () =>
            {
                Navigator.SceneReturn();
            });
            DebugLogConsole.AddCommand("print_scenestack", "打印当前场景栈", () =>
            {
                StringBuilder scenes = new StringBuilder();
                int index = 0;
                foreach (var scene in Navigator.GetSceneStack())
                {
                    scenes.Append(index);
                    scenes.Append(": ");
                    scenes.AppendLine(scene.ToString());
                    index++;
                }
                Logger.Log(scenes.ToString());
            });
        }

        /// <summary>
        /// 动态请求权限
        /// </summary>
        public void RequestPermission()
        {
#if PLATFORM_ANDROID
            var callbacks = new PermissionCallbacks();
            callbacks.PermissionGranted += str =>
            {
                Logger.Log($"permission granted: {str}", Color.yellow);
            };

            var permissions = new[] {
                 Permission.ExternalStorageRead, Permission.ExternalStorageWrite,
            };

            Logger.Log($"动态请求权限: \n{string.Join("\n", permissions)}");
            Permission.RequestUserPermissions(permissions, callbacks);
#endif
        }

        private void Init()
        {
            entryButton.onClick.AddListener(OnEntryClicked);
            if (version)
            {
                version.text = $"Version {Application.version}";
            }
        }

        private void OnEntryClicked()
        {
            Logger.Log("OnEntryClicked");
            if (videoPlayer.clip != null)
            {
                videoPlayer.playbackSpeed = 2.5f;
                videoPlayer.SetDirectAudioMute(0, true);
                return;
            }

            // 所有过程视频播放完毕
            videoPlayer.Stop();
            videoPlayer.gameObject.SetActive(false);

            Login();
        }

        /// <summary>
        /// 登录
        /// </summary>
        private void Login()
        {
            Logger.Log("Account Login");

            try
            {
                OnLoginSuccess();
            }
            catch (Exception e)
            {
                // 获取玩家信息失败
                PopupView.Instance.ShowErrorWindow($"登录异常：{e}！");
                Logger.LogError(e);
            }
        }

        // 登录成功后执行（如果之前已经登录过也执行）
        private void OnLoginSuccess()
        {
            Logger.Log("OnLoginSuccess");
            // CoroutineUtils.RunWithCondition(
            //     condition: () => AccountManager.User.Avatar != null,
            //     action: async () =>
            //     {
            //         ShowMessageBar();
            //         await Task.Delay(1500);
            //         accountBar.SetActive(false);
            //         JumpToNextScene();
            //     });
            InitDataBySavedArchive();
            JumpToNextScene();
        }

        /// <summary>
        /// 通过本地存档初始化数据
        /// </summary>
        private void InitDataBySavedArchive()
        {
            var savedArchive = FileUtils.LoadSavedArchive();
            PlayerRecordManager.InitRecords(savedArchive.ScoreRecords);
        }

        /// <summary>
        /// 视频播放完后的回调
        /// </summary>
        private void OnVideoEnd()
        {
            videoPlayer.Stop();
            videoPlayer.playbackSpeed = 1;
            videoPlayer.SetDirectAudioMute(0, false);
            if (currentVideoIndex >= videoClips.Count)
            {
                videoPlayer.clip = null;
                videoImage?.gameObject.SetActive(false);
                touchToStartText.gameObject.SetActive(true);
                topRightBtns.SetActive(true);
                SaveFirstPlaySetting();
                GetComponent<Animator>().SetTrigger("cutin");
                AudioPlayManager.Instance.PlayBGM(Sounds.Bgm.EntryScene);
                return;
            }
            else
            {
                videoPlayer.clip = videoClips[currentVideoIndex];
            }
            currentVideoIndex++;
            videoPlayer.Play();
        }

        /// <summary>
        /// 储存本设备的“初次游玩”参数
        /// </summary>
        private void SaveFirstPlaySetting()
        {
            if (isFirstPlay)
            {
                UserLocalSettings.SetInt(ConstConfigs.LocalKey.IsFirstPlay, 1);
            }
        }

        /// <summary>
        /// 显示玩家信息条
        /// </summary>
        private void ShowMessageBar()
        {
            avatar.sprite = AccountManager.User.Avatar;
            loginMessage.text = $"{AccountManager.User.NickName}，欢迎回来！";
            accountBar.SetActive(true);
        }

        private void JumpToNextScene()
        {
            switch (AccountManager.TutorialPhase)
            {
                case TutorialPhase.Start:
                case TutorialPhase.Playing:
                    TutorialManager.EnterTutorialPlay();
                    break;
                case TutorialPhase.End:
                default:
                    Navigator.JumpScene(GameScene.MainMenuScene);
                    break;
            }
        }

        private void QuitGame()
        {
            Application.Quit(0);
        }

        #endregion

        #region Coroutine

        IEnumerator SplashImageAnimation(Action callback = null)
        {
            splashImage.gameObject.SetActive(true);
            var leftColor = splashImage.color;
            var rightColor = Color.black;

            var waitSecond = new WaitForSeconds(0.5f);
            var deltaSecond = new WaitForSeconds(0.01f);
            foreach (var image in splashImages)
            {
                splashImage.sprite = image;
                for (int i = 0; i < 50; i++)
                {
                    splashImage.color = Color.Lerp(rightColor, leftColor, i / 50f);
                    yield return deltaSecond;
                }
                yield return waitSecond;
                for (int i = 0; i < 50; i++)
                {
                    splashImage.color = Color.Lerp(leftColor, rightColor, i / 50f);
                    yield return deltaSecond;
                }
            }

            splashImage.gameObject.SetActive(false);
            callback?.Invoke();
        }

        #endregion
    }
}