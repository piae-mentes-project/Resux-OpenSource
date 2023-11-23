using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Resux.GamePlay
{
    /// <summary>
    /// 缩放动画
    /// </summary>
    public class ScaleAnimation : MonoBehaviour
    {
        #region properties

        [SerializeField]
        [Tooltip("缩放持续时间")]
        private float continueTime = 0.5f;

        /// <summary>
        /// 动画的持续时间
        /// </summary>
        public float AnimationContinueTime => continueTime;

        [SerializeField]
        [Tooltip("缩放间隔/帧率，越大帧数越高")]
        [Range(10, 30)]
        private int times = 15;

        [Tooltip("是否是放大动画")]
        public bool IsLargerAnimation = false;

        private Action OnPlayEnd;

        #endregion

        #region Public Method

        public void StartAnimation()
        {
            if (IsLargerAnimation)
            {
                StartCoroutine(ScaleLarger());
            }
            else
            {
                StartCoroutine(ScaleSmaller());
            }
        }

        public void AddAnimationEndListener(Action onEnd)
        {
            OnPlayEnd += onEnd;
        }

        #endregion

        #region Coroutine

        IEnumerator ScaleSmaller()
        {
            var scaleStep = transform.localScale / times;
            var stepStayTime = new WaitForSeconds(continueTime / times);
            for (int i = 0; i < times; i++)
            {
                transform.localScale -= scaleStep;
                yield return stepStayTime;
            }

            OnPlayEnd?.Invoke();
        }

        IEnumerator ScaleLarger()
        {
            // TODO: 有待修改，这里只是先占位
            var scaleStep = transform.localScale / times;
            var stepStayTime = new WaitForSeconds(continueTime / times);
            for (int i = 0; i < times; i++)
            {
                transform.localScale += scaleStep;
                yield return stepStayTime;
            }

            OnPlayEnd?.Invoke();
        }

        #endregion
    }
}
