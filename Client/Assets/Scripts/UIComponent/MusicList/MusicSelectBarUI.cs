using System;
using UnityEngine;
using System.Collections;
using Resux.LevelData;
using UnityEngine.UI;

namespace Resux.UI
{
    /// <summary>
    /// 选歌界面的单曲条
    /// </summary>
    public class MusicSelectBarUI : MonoBehaviour
    {
        #region properties

        private LevelData.LevelDetail levelDetail;

        [SerializeField]
        private new Text name;

        /// <summary>
        /// 所处的位置
        /// </summary>
        private float position;

        /// <summary>
        /// 索引
        /// </summary>
        private int index;

        /// <summary>
        /// 滚动条位置
        /// </summary>
        private Scrollbar scroll;

        /// <summary>
        /// 两个条之间的距离
        /// </summary>
        private float distance;

        /// <summary>
        /// 被聚焦
        /// </summary>
        private bool isFocused;

        private Swipe swipe;

        [SerializeField]
        private Vector3 minScale = new Vector3(.8f, .8f, 1f);

        [SerializeField]
        private Vector3 maxScale = new Vector3(1f, 1f, 1f);

        /// <summary>
        /// 当前正在运行的协程
        /// </summary>
        private Coroutine currentCoroutine;

        /// <summary>
        /// 存在的回调
        /// </summary>
        private Action callback;

        #endregion

        #region Delegate

        private Action<int, float, LevelData.LevelDetail> onFocus;

        #endregion

        void Start()
        {
            // 还需要初始化config

            // 添加点击事件
            GetComponent<Button>().onClick.AddListener((() =>
            {
                swipe.needWait = true;
                ForceFocusOn((() => swipe.needWait = false));
            }));
        }

        void Update()
        {

        }

        void FixedUpdate()
        {
            if (swipe.needWait)
            {
                // 等待中
                return;
            }
            // 滚动条位置与歌曲条目位置差在一个距离单位内（浮点数不能直接与0比）
            if (Mathf.Abs(1 - scroll.value - position) < distance)
            {
                // Debug.Log($"need focus && {isFocused} && {index} && {scroll.value} && {position} && {distance}");
                if (!isFocused)
                {
                    OnFocusEnter();
                }
            }
            else
            {
                // 在被聚焦的时候且无协程运作的状态下取消聚焦
                // 后者主要避免强行聚焦引起的异常
                if (isFocused && currentCoroutine == null)
                {
                    OnFocusExit();
                }
            }
        }

        #region Public Method

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="index">索引</param>
        /// <param name="pos">位置</param>
        /// <param name="dis">两条的间距</param>
        /// <param name="scrollbar">滚动条</param>
        public void Initialize(int index, float pos, float dis, Scrollbar scrollbar, Swipe swipe)
        {
            Logger.Log($"Load选歌条目： 索引：{index}，位置：{pos}");
            this.swipe = swipe;
            this.index = index;
            position = pos;
            scroll = scrollbar;
            distance = dis;
            OnFocusExit();
        }

        /// <summary>
        /// 设置音乐配置信息
        /// </summary>
        /// <param name="config"></param>
        public void SetMusicConfig(LevelData.LevelDetail config)
        {
            this.levelDetail = config;
            name.text = $"{config._songName} - {config._artistName}";
        }

        /// <summary>
        /// 强制聚焦
        /// </summary>
        public void ForceFocusOn(Action callback = null)
        {
            this.callback = callback;
            StopAllCoroutines();
            OnFocusEnter();
            StartCoroutine(FocusToSelf());
        }

        /// <summary>
        /// 强制不聚焦
        /// </summary>
        public void ForceFocusOff(Action callback = null)
        {
            this.callback = callback;
            StopAllCoroutines();
            OnFocusExit();
        }

        /// <summary>
        /// 添加聚焦监听
        /// </summary>
        /// <param name="focus"></param>
        public void AddListener(Action<int, float, LevelData.LevelDetail> focus)
        {
            onFocus += focus;
        }

        #endregion

        #region Private Method

        /// <summary>
        /// 当聚焦时调用的方法
        /// </summary>
        private void OnFocusEnter()
        {
            isFocused = true;
            Logger.Log($"聚焦至：{index}, name：{levelDetail?._songName}");
            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
            }
            currentCoroutine = StartCoroutine(FocusOn());
            onFocus?.Invoke(index, position, levelDetail);
        }

        /// <summary>
        /// 当不被聚焦时调用的方法
        /// </summary>
        private void OnFocusExit()
        {
            isFocused = false;
            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
            }
            currentCoroutine = StartCoroutine(FocusOff());
        }

        #endregion

        #region 协程Coroutine

        /// <summary>
        /// 聚焦时的动画
        /// </summary>
        /// <returns></returns>
        IEnumerator FocusOn()
        {
            var scale = transform.localScale;
            for (int i = 1; Vector3.Distance(transform.localScale, maxScale) > 1e-7; i++)
            {
                // Debugger.Log("Scale On Animation");
                // Debugger.Log($"{position} && {scroll?.value} && {distance}");
                // Debugger.Log($"{index} && {transform.localScale} && {Vector3.Distance(transform.localScale, maxScale) > 1e-7}");
                transform.localScale = Vector3.Lerp(scale, maxScale, .1f * i);
                yield return new WaitForSeconds(0.02f);
            }
            // 存在则执行回调。
            if (callback != null)
            {
                callback();
                callback = null;
            }
        }

        /// <summary>
        /// 聚焦取消时的动画
        /// </summary>
        /// <returns></returns>
        IEnumerator FocusOff()
        {
            var scale = transform.localScale;
            for (int i = 1; Vector3.Distance(transform.localScale, minScale) > 1e-7; i++)
            {
                // Debugger.Log($"Scale Off Animation : {index}");
                transform.localScale = Vector3.Lerp(scale, minScale, .1f * i);
                yield return new WaitForSeconds(0.02f);
            }
            // 存在则执行回调。
            if (callback != null)
            {
                callback();
                callback = null;
            }
        }

        /// <summary>
        /// 在强行聚焦的时候移动到聚焦对象
        /// </summary>
        /// <returns></returns>
        IEnumerator FocusToSelf()
        {
            // 位置偏移在合理范围内（浮点数）
            for (int i = 1; Mathf.Abs(1 - scroll.value - position) > 1e-5; i++)
            {
                scroll.value = 1 - Mathf.Lerp(1 - scroll.value, position, 0.05f * i);
                yield return new WaitForFixedUpdate();
            }
        }

        #endregion
    }
}