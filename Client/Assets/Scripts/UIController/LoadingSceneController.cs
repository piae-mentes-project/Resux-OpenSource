using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Resux.Manager;
using Random = UnityEngine.Random;

namespace Resux.UI.Manager
{
    public class LoadingSceneController : MonoBehaviour
    {
        #region properties

        public static LoadingSceneController Instance;

        // Tips的显示对象
        [SerializeField]
        Text tipsView;

        [SerializeField]
        Image loadingImg;

        public Image BackgroundImage;
        [Header("logo显隐曲线")]
        [SerializeField]
        AnimationCurve curve;
        [Header("background显示曲线")]
        [SerializeField]
        AnimationCurve bgShowCureve;
        [Header("background隐藏曲线")]
        [SerializeField]
        AnimationCurve bgHideCureve;
        public float AnimationLength = 0.6f;
        // 使用txt储存Tips，返璞归真（不是
        private string[] tips;

        private Action callback;

        [SerializeField]
        [Range(1, 10)]
        private float timeScale = 1;
        private float timeCount;

        private float maxAlpha = 0f;
        private Color currentColor = Color.black;
        private bool isMaskinOver;

        #endregion

        private void OnEnable()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            isMaskinOver = true;
            timeCount = 0;
            if (tips == null)
            {
                tips = LocalizedManager.TipKeys.ToArray();
            }
            tipsView.text = GetTips();
        }

        void Start()
        {
            tips = LocalizedManager.TipKeys.ToArray();
        }

        void Update()
        {
            if (loadingImg == null)
            {
                return;
            }

            // 通过曲线获取图像的透明度
            var alpha = curve.Evaluate(GetTimeAxis());
            var newColor = loadingImg.color;
            newColor.a = alpha;
            loadingImg.color = newColor;

            // 0 - 1 - 2，把0-1的变化曲线以1为对称轴做对称处理，以此实现显隐循环（也就是在轴上0-1-0的往复运动）
            timeCount += Time.deltaTime;
            if (timeCount >= 2 * timeScale)
            {
                timeCount -= 2 * timeScale;
            }
        }

        #region Public Method

        public string GetTips()
        {
            return $"Tips: {tips[Random.Range(0, tips.Length)].Localize()}";
        }
        
        public void Show()
        {
            if (!gameObject.activeSelf)
            {
                Logger.Log("Show Loading");
                gameObject.SetActive(true);

                maxAlpha = (currentColor = new Color(0,0,0,.9f)).a;
                StartCoroutine(ShowAnimation());
            }
        }

        public void Hide()
        {
            if (gameObject.activeSelf)
            {
                Logger.Log("Hide Loading");

                StartCoroutine(HideAnimation());
            }
        }

        public void SetCallBack(Action callback)
        {
            this.callback = callback;
        }

        #endregion

        #region Private Method

        float GetTimeAxis()
        {
            float timeAxis;
            if (timeCount >= timeScale)
            {
                timeAxis = 2 * timeScale - timeCount;
            }
            else
            {
                timeAxis = timeCount;
            }

            return timeAxis / timeScale;
        }

        #endregion

        #region Coroutine

        IEnumerator ShowAnimation()
        {
            var waitForFrameEnd = new WaitForEndOfFrame();
            var startTime = Time.time;
            var currentTime = 0f;
            while ((currentTime = Time.time - startTime) <= AnimationLength)
            {
                currentColor.a = maxAlpha * bgShowCureve.Evaluate(currentTime / AnimationLength);
                BackgroundImage.color = currentColor;
                yield return waitForFrameEnd;
            }

            callback?.Invoke();
            callback = null;

            isMaskinOver = true;
        }

        IEnumerator HideAnimation()
        {
            var waitForFrameEnd = new WaitForEndOfFrame();
            yield return new WaitUntil(() => isMaskinOver);

            var startTime = Time.time;
            var currentTime = 0f;
            while ((currentTime = Time.time - startTime) <= AnimationLength)
            {
                currentColor.a = maxAlpha * bgHideCureve.Evaluate(currentTime / AnimationLength);
                BackgroundImage.color = currentColor;
                yield return waitForFrameEnd;
            }

            isMaskinOver = false;
            gameObject.SetActive(false);
        }

        #endregion
    }
}
