using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 通用协程方法类
/// </summary>
public static class CoroutineUtils
{
    #region properties

    private static WaitForEndOfFrame waitForEndOfFrame;

    public static Func<IEnumerator, Coroutine> StartCoroutine { get; private set; }
    public static Action<Coroutine> StopCoroutine { get; private set; }

    #endregion

    static CoroutineUtils()
    {
        waitForEndOfFrame = new WaitForEndOfFrame();
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
    /// 延迟执行，<paramref name="delayTime"/>是秒数
    /// </summary>
    /// <param name="action"></param>
    /// <param name="delayTime">延迟时间</param>
    /// <returns></returns>
    public static IEnumerator DelayExecute(Action action, float delayTime)
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

    /// <summary>
    /// 延迟执行，<paramref name="delayTime"/>是毫秒数
    /// </summary>
    /// <param name="action"></param>
    /// <param name="delayTime">延迟时间</param>
    /// <returns></returns>
    public static IEnumerator DelayExecute(Action action, int delayTime)
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

    /// <summary>
    /// 等待当前帧结束后执行
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public static IEnumerator ExecuteWhenFrameEnd(Action action)
    {
        yield return waitForEndOfFrame;
        action();
    }

    /// <summary>
    /// 每<paramref name="waitSecond"/>秒执行一次，共执行<paramref name="count"/>次
    /// </summary>
    /// <param name="action"></param>
    /// <param name="waitSecond"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public static IEnumerator ExecutePerDelayTime(Action action, float waitSecond, int count)
    {
        var wait = new WaitForSeconds(waitSecond);
        for (int i = 0; i < count; i++)
        {
            action();
            yield return wait;
        }
    }

    /// <summary>
    /// 每<paramref name="waitSecond"/>秒执行一次，共执行<paramref name="count"/>次
    /// </summary>
    /// <param name="action"></param>
    /// <param name="waitSecond"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public static IEnumerator ExecutePerDelayTime(Action<int> action, float waitSecond, int count)
    {
        var wait = new WaitForSeconds(waitSecond);
        for (int i = 0; i < count; i++)
        {
            action(i);
            yield return wait;
        }
    }

    /// <summary>
    /// 每帧执行一次，共执行<paramref name="count"/>次
    /// </summary>
    /// <param name="action"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public static IEnumerator ExecutePerFrame(Action action, int count)
    {
        var wait = new WaitForEndOfFrame();
        for (int i = 0; i < count; i++)
        {
            action();
            yield return wait;
        }
    }

    /// <summary>
    /// 每帧执行一次，共执行<paramref name="count"/>次
    /// </summary>
    /// <param name="action"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public static IEnumerator ExecutePerFrame(Action<int> action, int count)
    {
        var wait = new WaitForEndOfFrame();
        for (int i = 0; i < count; i++)
        {
            action(i);
            yield return wait;
        }
    }

    /// <summary>
    /// 等待条件满足后运行
    /// </summary>
    /// <param name="condition">运行条件</param>
    /// <param name="action">运行内容</param>
    /// <returns></returns>
    public static IEnumerator WaitUntil(Func<bool> condition, Action action)
    {
        var waitUntil = new WaitUntil(condition);
        yield return waitUntil;

        action?.Invoke();
    }

    public static IEnumerator DownloadImageFromUrl(string url, Action<Sprite> onSuccess)
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
}
