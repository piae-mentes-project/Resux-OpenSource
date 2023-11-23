using System.Collections;
using Resux.Assets;
using Resux.Data;
using UnityEngine;
using Resux.Manager;

namespace Resux.UI.Manager
{
    public class GlobalStaticUIManager : MonoBehaviour
    {
        public static GlobalStaticUIManager Instance => instance;

        private static GlobalStaticUIManager instance;

        #region properties

        private GameObject debugPanel;
        private GameObject exitGamePanel;

        #endregion

        void Awake()
        {
            instance = this;
            CoroutineUtils.SetCoroutineInstance(Instance);
        }

        void Start()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                if (instance != this)
                {
                    Destroy(gameObject);
                }
            }
            debugPanel = GameObject.Find("IngameDebugConsole");
            var debugMode = GameConfigs.GameBaseSetting.debugMode;
            debugPanel?.SetActive(debugMode);
            Navigator.Init();
            Init();
        }

        void Update()
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                // 禁用系统返回的场景不允许使用系统返回做任何事情
                if (Navigator.IsCurrentSceneCanUseEscape())
                {
                    // 先关弹窗，弹窗关完了再返回场景
                    if (!PopupView.Instance.OnEscape())
                    {
                        BackToPrevScene();
                    }
                }
            }
        }

        private void OnApplicationQuit()
        {
            // 当快速退出时会导致调用协程等待卸载ab包时实例已经被摧毁的情况
            // 因此在这里通知以直接进行卸载
            AssetBundleDM.ForceUnloadAllBundles();
        }

        #region Public Method

        public void BackToPrevScene()
        {
            var result = Navigator.SceneReturn();
            if (!result)
            {
                // 是否要退出游戏
                if (!exitGamePanel)
                {
                    exitGamePanel = UI.PopupView.Instance.ShowUniversalWindow("是否要退出游戏？",
                        onCancel: () => { },
                        onOk: () =>
                        {
                            QuitGame();
                        });
                }
                else
                {
                    exitGamePanel.SetActive(!exitGamePanel.activeSelf);
                }
            }
        }

        #endregion

        #region Private Method

        /// <summary>
        /// 初始化各种常驻面板
        /// </summary>
        private void Init()
        {
            var loadingPanel = Resources.Load<GameObject>("Prefabs/UI/LoadingPanel");
            loadingPanel = Instantiate(loadingPanel, transform);
            loadingPanel.SetActive(false);
            // LoadingUIManager.Instance.Hide();
        }

        private void QuitGame()
        {
            Application.Quit(0);
        }

        #endregion
    }
}