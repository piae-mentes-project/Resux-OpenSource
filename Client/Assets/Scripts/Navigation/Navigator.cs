using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using System.Linq.Expressions;
using Resux.Configuration;
using Resux.UI.Manager;

namespace Resux.Manager
{
    /// <summary>
    /// 场景切换管理
    /// </summary>
    public static class Navigator
    {
        class LoadSceneData
        {
            public LoadSceneMode Mode;
            public Action OnUnload;
        }

        #region properties

        private static Stack<(GameScene scene, LoadSceneData data)> sceneStack;
        private static GameScene currentScene;
        private static Action onUnloadAction;

        /// <summary>禁用返回的场景</summary>
        private static List<GameScene> banBackScenes = new List<GameScene>()
        {
            GameScene.PlayScene,
            GameScene.ResultScene
        };

        #endregion

        static Navigator()
        {
            sceneStack = new Stack<(GameScene scene, LoadSceneData data)>();
            currentScene = GameScene.None;
        }

        #region Public Method

        public static void Init()
        {
            SceneManager.sceneLoaded += OnSceneLoadEnd;
            SceneManager.sceneUnloaded += OnSceneUnLoadEnd;
        }

        #region Old Code

        /// <summary>
        /// 加载场景<c>（异步）</c>，<paramref name="lastScene"/>是None时不会加入到返回栈内
        /// <paramref name="onUnload"/>是加载的目标场景被卸载后的回调
        /// </summary>
        /// <param name="scene">目标场景</param>
        /// <param name="lastScene">当前场景（上一场景）</param>
        /// <param name="isSceneAdditive">是否以叠加形式加载</param>
        /// <param name="onUnload">加载的目标场景被卸载后的回调</param>
        [Obsolete]
        public static void LoadScene(GameScene scene, GameScene lastScene, bool isSceneAdditive = false, Action onUnload = null)
        {
            Logger.Log($"scene: {scene}, lastScene: {lastScene}, isAdditive: {isSceneAdditive}, onUnload is null: {onUnload is null}", Color.yellow);
            var loadMode = isSceneAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single;
            onUnloadAction = onUnload;
            if (lastScene != GameScene.None)
            {
                var data = new LoadSceneData()
                {
                    Mode = loadMode,
                    OnUnload = onUnload,
                };
                sceneStack.Push((lastScene, data));
            }

            if (scene != currentScene)
            {
                currentScene = scene;
            }

            LoadSceneAction(scene, loadMode);
        }

        /// <summary>
        /// 场景退回（当无场景可退时返回 false）
        /// </summary>
        /// <returns>场景退回是否成功</returns>
        [Obsolete]
        public static bool SceneEscape()
        {
            if (sceneStack.Count > 0)
            {
                // 场景数据为：上个场景，当前场景的加载方式，当前场景卸载后的回调
                var sceneData = sceneStack.Pop();
                switch (sceneData.data.Mode)
                {
                    case LoadSceneMode.Additive:
                        SceneManager.UnloadSceneAsync(currentScene.ToString());
                        currentScene = sceneData.scene;
                        onUnloadAction = sceneData.data.OnUnload;
                        break;
                    case LoadSceneMode.Single:
                    default:
                        LoadScene(sceneData.scene, GameScene.None, sceneData.data.Mode == LoadSceneMode.Additive, sceneData.data.OnUnload);
                        break;
                }
                return true;
            }

            return false;
        }


        #endregion

        public static void ClearEscapeStack()
        {
            sceneStack.Clear();
        }

        public static bool IsCurrentSceneCanUseEscape()
        {
            foreach (var scene in banBackScenes)
            {
                if (scene == currentScene)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 返回上一个场景
        /// </summary>
        public static bool SceneReturn()
        {
            if (sceneStack.Count <= 1)
                return false;

            var curr = sceneStack.Pop();
            var last = sceneStack.Peek();
            if (curr.data.Mode == LoadSceneMode.Additive)
            {
                SceneManager.UnloadSceneAsync(currentScene.ToString());
                onUnloadAction = last.data.OnUnload; // 真的有scene用到onunload了吗？
            }
            else
            {
                LoadSceneAction(last.scene, last.data.Mode);
            }

            currentScene = last.scene;

            return true;
        }

        /// <summary>
        /// 载入一个新的Scene，可以返回到上一个Scene
        /// </summary>
        public static void LoadNewScene(GameScene scene, bool additive = false)
        {
            var loadMode = additive ? LoadSceneMode.Additive : LoadSceneMode.Single;
            sceneStack.Push((scene, new LoadSceneData
            {
                Mode = loadMode
            }));
            currentScene = scene;
            LoadSceneAction(scene, loadMode);
        }

        /// <summary>
        /// 跳转到一个Scene，并替换掉当前的Scene，无法返回到被替换的Scene
        /// </summary>
        public static void JumpScene(GameScene scene, bool additive = false)
        {
            var loadMode = additive ? LoadSceneMode.Additive : LoadSceneMode.Single;

            if (sceneStack.Count != 0)
            {
                var lastScene = sceneStack.Pop();
                lastScene.scene = scene;
                lastScene.data.Mode = loadMode;
                sceneStack.Push(lastScene);
            }
            else
            {
                sceneStack.Push((scene, new LoadSceneData
                {
                    Mode = LoadSceneMode.Single
                }));
            }

            currentScene = scene;

            LoadSceneAction(scene, loadMode);
        }

        #endregion



        #region Private Method
        
        internal static GameScene[] GetSceneStack()
        {
            return sceneStack.Select(x => x.scene).ToArray();
        }

        private static void LoadSceneAction(GameScene scene, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
        {
            LoadingSceneController.Instance.SetCallBack(
                () =>
                {
                    SceneManager.LoadSceneAsync(scene.ToString(), loadSceneMode);
                });
            LoadingSceneController.Instance.Show();
        }

        private static void OnSceneLoadEnd(Scene scene, LoadSceneMode loadSceneMode)
        {
            Logger.Log($"Scene Load Complete: {scene.name}");
            LoadingSceneController.Instance.Hide();
        }

        private static void OnSceneUnLoadEnd(Scene scene)
        {
            Logger.Log($"Scene UnLoad Complete: {scene.name}");
            if (onUnloadAction != null)
            {
                onUnloadAction.Invoke();
                onUnloadAction = null;
            }
        }

        #endregion
    }
}