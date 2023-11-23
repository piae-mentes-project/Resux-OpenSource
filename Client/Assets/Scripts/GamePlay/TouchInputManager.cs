using System;
using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace Resux
{
    public enum TouchInputState
    {
        Moving = 1,  // 触摸点在移动
        Static = 0 // 静止的触摸点
    }

    public class TouchInputInfo
    {
        /// <summary>Unity接口下的原始触摸信息，注意这是结构体，没有按指针引用</summary>
        public Touch RawTouch;
        /// <summary>触摸ID（唯一的，即使别的手指离开了这个也不会变）</summary>
        public int FingerId;
        /// <summary>当前的坐标</summary>
        public Vector2 Pos;
        /// <summary>上次的稳定位置</summary>
        public Vector2 LastPos;
        /// <summary>上次更改状态的时间点</summary>
        public int LastStateChangeTime;
        /// <summary>已经按住的时长</summary>
        public int HoldTime;
        /// <summary>已被用于判定</summary>
        public bool IsUsed;
        /// <summary>当前触摸点的状态</summary>
        public TouchInputState State = TouchInputState.Static;
        /// <summary>已经离手</summary>
        public bool Leaved;


        /*
        * 现在这个游戏有着3种判定类型：Tap/Flick/Hold
        * 加入一个`TouchInputState State`。
        *
        * | Tap ：Tap判定只需要判定时有符合位置且
        * | `State == TouchInputState.Static`且`isUsed == false`的触摸点即可判定
        * | 判定成功后将`isUsed 改为 true`
        *
        * | Flick ：Flick判定时需要有一个位置符合且
        * | `State == TouchInputState.Moving`的触摸点即可完成判定
        * | 同样，判定成功后将`isUsed 改为 true`
        *
        * | Hold ：开始判定时需要符合位置且`State == TouchInputState.Static`
        * | 且`isUsed == false`的触摸点即可。开始判定后将`isUsed 改为 true`，将
        * | 这个触摸点信息（`TouchInputInfo`）保存到`HoldJudgeNote`即可，这里不再重复触发事件
        *
        * | 对Hold保存触摸点的数据持续更新：因为C#对class对象的引用，所以直接修改这个类实例的值，
        * | `HoldJudgeNote`里保存的触摸点信息也会更新。
        *
        * | 注意：对于没有成功进行一个判定的触摸点，将`isUsed 改为 true`
        *
        * 这样子就不需要保存啥`JudgeType JudgeType`了
        */

        public TouchInputInfo(Touch touch)
        {
            State = TouchInputState.Static;
            RawTouch = touch;
            FingerId = touch.fingerId;
        }
    }


    /// <summary>
    /// 触摸输入
    /// </summary>
    public class TouchInputManager : MonoBehaviour
    {
        #region singleton

        public static TouchInputManager Instance;

        #endregion

        #region properties

        private bool isEnable;

        public bool IsEnable => isEnable;
        // 触摸信息列表
        private List<TouchInputInfo> touchInfos = new List<TouchInputInfo>(10);
        private Touch[] touches = null;

        #region delegate

        private Action<TouchInputInfo> onBeginTouch;
        private Action<TouchInputInfo> onTouchMoving;
        private Action<TouchInputInfo> onContinueTouch;

        #endregion

        #endregion

        #region Unity Engine
        public List<TouchInputInfo> TouchInfos => touchInfos;
        void Awake()
        {
            Instance = this;
        }

        void Update()
        {
            if (!IsEnable)
            {
                return;
            }

            touches = Input.touches;
            // 首先移除掉手移开了的触摸信息
            // 与此同时，更新触摸信息
            var lengthOfTouchInfos = touchInfos.Count;
            // 这个是“下一个要release的索引”，也可以认为是已release的数量
            var touchInfosReleasedIndex = 0;
            for (int i = 0; i < lengthOfTouchInfos; i++)
            {
                var touchInfo = touchInfos[i];
                // 找到对应的原始触摸信息
                if (!FindTouchByFingerId(touchInfo.FingerId, out var touchRaw)) continue;
                if (touchRaw.phase == TouchPhase.Ended || touchRaw.phase == TouchPhase.Canceled)
                {
                    touchInfo.Leaved = true;
                    var swapTemp = touchInfos[touchInfosReleasedIndex];
                    touchInfos[touchInfosReleasedIndex] = touchInfo;
                    touchInfos[i] = swapTemp;
                    touchInfosReleasedIndex++;
                    continue;
                }
                touchInfo.RawTouch = touchRaw;
                touchInfo.Pos = touchRaw.position;
            }


            // 移除已经移出的触摸
            var lengthOfNewTouchInfos = lengthOfTouchInfos - touchInfosReleasedIndex;
            var newTouchInfos = new TouchInputInfo[lengthOfNewTouchInfos];
            // 是从0+（index -> (length-index)）的索引，而不是从index开始计算
            // 原来的从index开始计算到length的写法，巧妙的避免了越界而没有避免空（
            for (int i = 0; i < lengthOfNewTouchInfos; i++)
            {
                newTouchInfos[i] = touchInfos[touchInfosReleasedIndex + i];
            }
            touchInfos = new List<TouchInputInfo>(newTouchInfos);


            // 剩下的触摸信息每个加上时间 并判断是否需要类型转变
            for (int i = touchInfosReleasedIndex; i < lengthOfNewTouchInfos; i++)
            {
                var touchInfo = touchInfos[i];
                // 触摸时间累计
                touchInfo.HoldTime += Mathf.FloorToInt((Time.deltaTime * 1000));

                if (Vector2.Distance(touchInfo.Pos, touchInfo.LastPos) > GameConfigs.StableDistance)
                {
                    touchInfo.LastPos = touchInfo.Pos;
                    touchInfo.State = TouchInputState.Moving;
                    touchInfo.LastStateChangeTime = touchInfo.HoldTime;
                    onTouchMoving?.Invoke(touchInfo);
                    continue;
                }
                else if (touchInfo.LastStateChangeTime + 1 < touchInfo.HoldTime) // 加个时间限制，防止转为Static太快了
                {
                    touchInfo.State = TouchInputState.Static;
                }

                onContinueTouch?.Invoke(touchInfo);
            }

            // 处理原始触摸
            foreach (var touch in touches)
            {
                if (TouchPhase.Began == touch.phase)
                {
                    // 手指进入 新建对象并加入列表
                    var touchInputInfo = new TouchInputInfo(touch)
                    {
                        Pos = touch.position,
                        LastPos = touch.position,
                    };
                    touchInfos.Add(touchInputInfo);
                    onBeginTouch?.Invoke(touchInputInfo);
                }
            }
        }

        #endregion

        #region Private Method

        private bool FindTouchByFingerId(int fingerId, out Touch touch)
        {
            var lengthOfTouches = touches.Length;
            for (int i = 0; i < lengthOfTouches; i++)
            {
                var element = touches[i];
                if (fingerId == element.fingerId)
                {
                    touch = element;
                    return true;
                }
            }
            touch = new Touch();
            return false;
        }

        #endregion

        #region Public Method

        /// <summary>
        /// 启用
        /// </summary>
        public void Enable()
        {
            if (IsEnable)
            {
                return;
            }
            isEnable = true;
        }

        /// <summary>
        /// 设置启用状态
        /// </summary>
        /// <param name="isEnable">是否启用</param>
        public void SetEnable(bool isEnable)
        {
            // 异或，不同值为true
            if (isEnable ^ IsEnable)
            {
                this.isEnable = isEnable;
            }
        }

        /// <summary>
        /// 禁用
        /// </summary>
        public void Disable()
        {
            if (!IsEnable)
            {
                return;
            }
            isEnable = false;
            touchInfos.Clear();
        }

        public void AddBeginTouchListener(Action<TouchInputInfo> action)
        {
            onBeginTouch += action;
        }

        public void AddTouchMovingListener(Action<TouchInputInfo> action)
        {
            onTouchMoving += action;
        }

        public void AddCoutinueTouchListener(Action<TouchInputInfo> action)
        {
            onContinueTouch += action;
        }

        public void RemoveAllListeners()
        {
            foreach (var action in onBeginTouch.GetInvocationList())
            {
                if (action is Action<TouchInputInfo> onBegin)
                {
                    onBeginTouch -= onBegin;
                }
            }

            foreach (var action in onTouchMoving.GetInvocationList())
            {
                if (action is Action<TouchInputInfo> onMove)
                {
                    onTouchMoving -= onMove;
                }
            }
        }

        #endregion
    }
}