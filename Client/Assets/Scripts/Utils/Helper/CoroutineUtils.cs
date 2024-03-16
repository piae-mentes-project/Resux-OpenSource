using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Resux
{
    /// <summary>
    /// 通用协程方法类
    /// </summary>
    public class CoroutineUtils : MonoBehaviour
    {
        #region properties

        private static WaitForEndOfFrame waitForEndOfFrame;

        private static Func<IEnumerator, Coroutine> StartCoroutine { get; set; }
        
        private static Action<Coroutine> StopCoroutine { get; set; }

        #endregion

        static CoroutineUtils()
        {
            waitForEndOfFrame = new WaitForEndOfFrame();
            var coroutineUtilsGo = new GameObject("coroutineUtils");
            DontDestroyOnLoad(coroutineUtilsGo);
            SetCoroutineInstance(coroutineUtilsGo.AddComponent<CoroutineUtils>());
        }

        /// <summary>
        /// 设置协程用的工具（
        /// </summary>
        /// <param name="mono"></param>
        public static void SetCoroutineInstance(MonoBehaviour mono)
        {
            StartCoroutine = mono.StartCoroutine;
            StopCoroutine = mono.StopCoroutine;
        }

        /// <summary>
        /// 停止指定协程
        /// </summary>
        /// <param name="coroutine">要停止的协程</param>
        public static void Stop(Coroutine coroutine)
        {
            StopCoroutine(coroutine);
        }

        /// <summary>
        /// 延迟执行，<paramref name="delayTime"/>是秒数
        /// </summary>
        /// <param name="action"></param>
        /// <param name="delayTime">延迟时间</param>
        /// <returns></returns>
        public static Coroutine RunDelay(Action action, float delayTime)
        {
            return StartCoroutine(DelayExecute(action, delayTime));
        }

        /// <summary>
        /// 延迟执行，<paramref name="delayTime"/>是毫秒数
        /// </summary>
        /// <param name="action">要执行的方法</param>
        /// <param name="delayTime">延迟时间</param>
        /// <returns>协程实例</returns>
        public static Coroutine RunDelay(Action action, int delayTime)
        {
            return StartCoroutine(DelayExecute(action, delayTime));
        }

        /// <summary>
        /// 等待当前帧结束后执行
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Coroutine RunWhenFrameEnd(Action action)
        {
            return StartCoroutine(ExecuteWhenFrameEnd(action));
        }

        /// <summary>
        /// 每<paramref name="waitSecond"/>秒执行一次，共执行<paramref name="count"/>次
        /// </summary>
        /// <param name="action">要执行的方法</param>
        /// <param name="waitSecond">每次等待的秒数</param>
        /// <param name="count">执行次数</param>
        /// <returns>协程实例</returns>
        public static Coroutine RunPerDelayTime(Action action, float waitSecond, int count)
        {
            return StartCoroutine(ExecutePerDelayTime(action, waitSecond, count));
        }

        /// <summary>
        /// 每<paramref name="waitSecond"/>秒执行一次，共执行<paramref name="count"/>次
        /// </summary>
        /// <param name="action">要执行的以次数为参数的方法</param>
        /// <param name="waitSecond">每次等待的秒数</param>
        /// <param name="count">执行次数</param>
        /// <returns>协程实例</returns>
        public static Coroutine RunPerDelayTime(Action<int> action, float waitSecond, int count)
        {
            return StartCoroutine(ExecutePerDelayTime(action, waitSecond, count));
        }

        /// <summary>
        /// 每帧执行一次，共执行<paramref name="count"/>次
        /// </summary>
        /// <param name="action">要执行的方法</param>
        /// <param name="count">次数</param>
        /// <returns>协程实例</returns>
        public static Coroutine RunPerFrame(Action action, int count)
        {
            return StartCoroutine(ExecutePerFrame(action, count));
        }

        /// <summary>
        /// 每帧执行一次，共执行<paramref name="count"/>次
        /// </summary>
        /// <param name="action">要执行的参数为帧索引的方法</param>
        /// <param name="count">次数</param>
        /// <returns>协程实例</returns>
        public static Coroutine RunPerFrame(Action<int> action, int count)
        {
            return StartCoroutine(ExecutePerFrame(action, count));
        }

        /// <summary>
        /// 等待条件满足后运行
        /// </summary>
        /// <param name="condition">运行条件</param>
        /// <param name="action">运行内容</param>
        /// <returns>协程实例</returns>
        public static Coroutine RunWithCondition(Func<bool> condition, Action action)
        {
            return StartCoroutine(WaitUntil(condition, action));
        }

        /// <summary>
        /// 从指定链接下载图片
        /// </summary>
        /// <param name="url">图片链接</param>
        /// <param name="onSuccess">成功的回调</param>
        /// <returns>协程实例</returns>
        public static Coroutine DownloadImage(string url, Action<Sprite> onSuccess)
        {
            return StartCoroutine(DownloadImageFromUrl(url, onSuccess));
        }

        /// <summary>
        /// 以指定条件循环执行
        /// </summary>
        /// <typeparam name="T">执行的条件对象</typeparam>
        /// <param name="valueProvider">提供值的方法</param>
        /// <param name="condition">判断是否进入下一循环的条件</param>
        /// <param name="validChecker">判断值是否有效的方法</param>
        /// <param name="action">实际执行的方法</param>
        /// <param name="onLoopOver">循环结束后的回调</param>
        /// <returns>协程实例</returns>
        public static Coroutine RunWithLoop<T>(Func<T> valueProvider, Func<T, bool> condition, Func<T, bool> validChecker, Action<T> action, Action onLoopOver = null)
        {
            return StartCoroutine(ExecuteWithLoop(valueProvider, condition, validChecker, action, onLoopOver));
        }

        /// <summary>
        /// 以一定的时间间隔循环执行，直到条件不满足
        /// </summary>
        /// <param name="action">执行的内容</param>
        /// <param name="condition">需要满足的条件</param>
        /// <param name="waitSecond">每次执行的间隔时间</param>
        /// <param name="executeImmediatelyFirst">是否立即执行一次</param>
        /// <param name="onOver">循环执行完毕后的回调</param>
        /// <returns>协程实例</returns>
        public static Coroutine RunLoopUntil(Action action, Func<bool> condition, float waitSecond, bool executeImmediatelyFirst, Action onOver = null)
        {
            return StartCoroutine(ExecuteLoopUntil(action, condition, waitSecond, executeImmediatelyFirst, onOver));
        }

        /// <summary>
        /// 执行并等待结果
        /// </summary>
        /// <param name="action">执行内容</param>
        /// <param name="condition">等待条件</param>
        /// <param name="onOver">完成的回调</param>
        /// <returns>协程实例</returns>
        public static Coroutine RunAndWaitUntil(Action action, Func<bool> condition, Action onOver = null)
        {
            return StartCoroutine(ExecuteAndWaitUntil(action, condition, onOver));
        }

        /// <summary>
        /// 等待条件满足后执行
        /// </summary>
        /// <param name="condition">等待条件</param>
        /// <param name="action">执行内容</param>
        /// <returns>协程实例</returns>
        public static Coroutine RunWaitUntil(Func<bool> condition, Action action)
        {
            return StartCoroutine(ExecuteWaitUntil(condition, action));
        }

        /// <summary>
        /// 执行并等待结果
        /// </summary>
        /// <param name="action">执行内容</param>
        /// <param name="condition">等待条件</param>
        /// <param name="onOver">完成的回调</param>
        /// <returns>协程实例</returns>
        public static Coroutine RunAndWaitUntil(Func<YieldInstruction> action, Func<bool> condition, Action onOver = null)
        {
            return StartCoroutine(ExecuteAndWaitUntil(action, condition, onOver));
        }

        #region Private Coroutine Methods

        private static IEnumerator DelayExecute(Action action, float delayTime)
        {
            if (delayTime <= 0)
            {
                action();
                yield return null;
            }
            else
            {
                yield return new WaitForSeconds(delayTime);
                action();
            }
        }

        private static IEnumerator DelayExecute(Action action, int delayTime)
        {
            var delay = delayTime / 1000f;
            if (delay <= 0)
            {
                action();
                yield return null;
            }
            else
            {
                yield return new WaitForSeconds(delay);
                action();
            }
        }

        private static IEnumerator ExecuteWhenFrameEnd(Action action)
        {
            yield return waitForEndOfFrame;
            action();
        }

        private static IEnumerator ExecutePerDelayTime(Action action, float waitSecond, int count)
        {
            var wait = new WaitForSeconds(waitSecond);
            for (int i = 0; i < count; i++)
            {
                action();
                yield return wait;
            }
        }

        private static IEnumerator ExecutePerDelayTime(Action<int> action, float waitSecond, int count)
        {
            var wait = new WaitForSeconds(waitSecond);
            for (int i = 0; i < count; i++)
            {
                action(i);
                yield return wait;
            }
        }

        private static IEnumerator ExecutePerFrame(Action action, int count)
        {
            for (int i = 0; i < count; i++)
            {
                action();
                yield return waitForEndOfFrame;
            }
        }

        private static IEnumerator ExecutePerFrame(Action<int> action, int count)
        {
            for (int i = 0; i < count; i++)
            {
                action(i);
                yield return waitForEndOfFrame;
            }
        }

        private static IEnumerator WaitUntil(Func<bool> condition, Action action)
        {
            var waitUntil = new WaitUntil(condition);
            yield return waitUntil;

            action?.Invoke();
        }

        private static IEnumerator DownloadImageFromUrl(string url, Action<Sprite> onSuccess)
        {
            UnityWebRequest textureReq = UnityWebRequestTexture.GetTexture(url);

            yield return textureReq.SendWebRequest();

            if (textureReq.result == UnityWebRequest.Result.ProtocolError || textureReq.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError("网络图片请求失败，请检查网络");
            }
            else
            {
                var texture = (textureReq.downloadHandler as DownloadHandlerTexture).texture;
                Sprite sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f));
                onSuccess?.Invoke(sprite);
            }
        }

        private static IEnumerator ExecuteWithLoop<T>(Func<T> valueProvider, Func<T, bool> condition, Func<T, bool> validChecker, Action<T> action, Action onLoopOver = null)
        {
            while (true)
            {
                var value = valueProvider();
                if (validChecker(value))
                {
                    action(value);
                    yield return new WaitUntil(() => condition(value));
                }
                else
                {
                    break;
                }
            }

            onLoopOver?.Invoke();
        }

        private static IEnumerator ExecuteLoopUntil(Action action, Func<bool> condition, float waitSecond, bool executeImmediatelyFirst, Action onOver = null)
        {
            var wait = new WaitForSeconds(waitSecond);
            if (executeImmediatelyFirst && condition())
            {
                action();
            }

            while (condition())
            {
                yield return wait;
                action();
            }

            onOver?.Invoke();
        }

        private static IEnumerator ExecuteAndWaitUntil(Action action, Func<bool> condition, Action onOver = null)
        {
            action();

            yield return new WaitUntil(condition);

            onOver?.Invoke();
        }

        private static IEnumerator ExecuteAndWaitUntil(Func<YieldInstruction> action, Func<bool> condition, Action onOver = null)
        {
            yield return action();
            yield return new WaitUntil(condition);

            onOver?.Invoke();
        }

        private static IEnumerator ExecuteWaitUntil(Func<bool> condition, Action action)
        {
            yield return new WaitUntil(condition);

            action?.Invoke();
        }

        #endregion
    }
}
