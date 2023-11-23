using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class UIFrameAnimation : MonoBehaviour
{
    #region inner class/enum

    public enum ActiveType
    {
        Start,
        OnEnable,
        WaitActive,
    }

    public enum FrameImageType
    {
        GoodJudgeCircle,
        GoodJudgeParticle,
        PerfectJudgeCircle,
        PerfectJudgeParticle,
        ScaleCircle,
        EarlyPerfect,
        LatePerfect
    }

    #endregion

    #region properties

    #region static

    private static Dictionary<FrameImageType, Sprite[]> cacheDic;

    #endregion

    public string FrameImagePath;

    [SerializeField] private int fps = 30;
    [SerializeField] private ActiveType ActiveOn;
    [SerializeField] private FrameImageType imageType;
    [SerializeField] private bool isLoop;
    public bool IsLoop => isLoop;

    private Sprite[] images;
    private Action animationCallBack;
    private Action<int> actionPerFrame;
    private Action onForceLoopOver;
    private int index;

    /// <summary>帧动画之间的间隔</summary>
    private float deltaTime;
    /// <summary>当前帧动画播放时间</summary>
    private float currentPlayTime;
    private bool isPlay;

    public Image Image;
    public SpriteRenderer spriteRenderer;

    #endregion

    static UIFrameAnimation()
    {
        cacheDic = new Dictionary<FrameImageType, Sprite[]>();
    }

    private void OnEnable()
    {
        Initialize();
        if (ActiveOn != ActiveType.OnEnable)
        {
            return;
        }
        isPlay = true;
    }

    void Start()
    {
        if (ActiveOn != ActiveType.Start)
        {
            return;
        }
        isPlay = true;
    }

    void Update()
    {
        if (!isPlay)
        {
            return;
        }

        if (!Image && !spriteRenderer)
        {
            return;
        }

        index = (int)(currentPlayTime / deltaTime);

        currentPlayTime += Time.deltaTime;

        if (index >= images.Length)
        {
            if (isLoop)
            {
                index %= images.Length;
            }
            else
            {
                isPlay = false;
                // callback 这里是动画完成后调用的位置，可以写按钮事件监听等等，避免误操作
                animationCallBack?.Invoke();
                return;
            }
        }

        var image = images[index];
        if (Image)
        {
            Image.sprite = image;
        }
        else if (spriteRenderer)
        {
            spriteRenderer.sprite = image;
        }
        actionPerFrame?.Invoke(index);
    }

    #region Public Method

    public void Initialize()
    {
        index = 0;
        deltaTime = 1.0f / fps;
        currentPlayTime = 0;
        if (string.IsNullOrEmpty(FrameImagePath))
        {
            Debug.LogError("帧动画路径为空！");
            return;
        }

        if (!cacheDic.ContainsKey(imageType))
        {
            images = Resources.LoadAll<Sprite>(FrameImagePath);
            cacheDic.Add(imageType, images);
        }
        else
        {
            images = cacheDic[imageType];
        }
    }

    /// <summary>
    /// 添加每帧调用的方法
    /// </summary>
    /// <param name="frameIndex">起始调用的帧数</param>
    /// <param name="action">方法</param>
    public void AddFrameAction(int frameIndex, Action action)
    {
        actionPerFrame += currentFrame =>
        {
            if (frameIndex <= currentFrame)
            {
                action?.Invoke();
            }
        };
    }

    public void AddFrameAction(Action<int> action)
    {
        actionPerFrame += action;
    }

    /// <summary>
    /// 添加每帧调用的方法，参数为 <b>1/fps</b>
    /// </summary>
    /// <param name="frameIndex">起始调用的帧数</param>
    /// <param name="action">含float参数的方法</param>
    public void AddFrameAction(int frameIndex, Action<float> action)
    {
        var deltaTime = 1.0f / fps;
        actionPerFrame += currentFrame =>
        {
            if (frameIndex <= currentFrame)
            {
                action?.Invoke(deltaTime);
            }
        };
    }

    /// <summary>
    /// 添加动画播放完毕后的回调
    /// </summary>
    /// <param name="callback">回调</param>
    public void AddAnimationCallback(Action callback)
    {
        animationCallBack += callback;
    }

    /// <summary>
    /// 添加强制循环结束时的回调
    /// </summary>
    /// <param name="callback"></param>
    public void AddOnForceLoopOverCallback(Action callback)
    {
        onForceLoopOver += callback;
    }

    /// <summary>
    /// 强制循环结束
    /// </summary>
    public void ForceLoopOver()
    {
        isPlay = false;
        onForceLoopOver?.Invoke();
    }

    /// <summary>
    /// 激活动画（仅限<see cref="ActiveType.WaitActive"/>）
    /// </summary>
    public void ActiveAnimationWithType()
    {
        if (ActiveOn == ActiveType.WaitActive)
        {
            ActiveAnimation();
        }
    }

    /// <summary>
    /// 激活动画
    /// </summary>
    public void ActiveAnimation()
    {
        isPlay = true;
        // StartCoroutine(FrameAnimation());
    }

    public void ShowNext()
    {
        index++;
        if (index >= images.Length)
        {
            return;
        }

        if (Image)
        {
            Image.sprite = images[index];
        }
        else if (spriteRenderer)
        {
            spriteRenderer.sprite = images[index];
        }
    }

    /// <summary>
    /// 根据依赖的帧动画的当前帧索引来判断显示哪一帧
    /// </summary>
    /// <param name="startFrame"></param>
    /// <param name="i"></param>
    public void ShowWhen(int startFrame, int i)
    {
        if (startFrame > i)
        {
            return;
        }

        index = i - startFrame;
        if (index >= images.Length)
        {
            return;
        }

        if (Image)
        {
            Image.sprite = images[index];
        }
        else if (spriteRenderer)
        {
            spriteRenderer.sprite = images[index];
        }
    }

    #endregion

    /// <summary>
    /// 帧动画
    /// </summary>
    IEnumerator FrameAnimation()
    {
        if (!Image && !spriteRenderer)
        {
            yield break;
        }
        var timeWait = new WaitForSeconds(1.0f / fps);
        for (index = 0; index < images.Length; index++)
        {
            Sprite image = images[index];
            if (Image)
            {
                Image.sprite = image;
            }
            else if (spriteRenderer)
            {
                spriteRenderer.sprite = image;
            }
            actionPerFrame?.Invoke(index);
            yield return timeWait;
        }

        // callback 这里是动画完成后调用的位置，可以写按钮事件监听等等，避免误操作
        animationCallBack?.Invoke();
    }
}
