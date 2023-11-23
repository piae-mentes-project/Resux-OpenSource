using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Resux.UI
{
    /// <summary>
    /// 判定特效
    /// </summary>
    public class JudgeEffect : MonoBehaviour
    {
        #region properties

        [SerializeField]
        private List<JudgeCircle> circles = new List<JudgeCircle>();

        /// <summary>
        /// 有效的圆
        /// </summary>
        private List<JudgeCircle> validCircles
        {
            get => circles.Where(c => !c.isDisappear).ToList();
        }

        /// <summary>
        /// 是否全部消失
        /// </summary>
        private bool FullDisappear
        {
            get => !circles.Where(c => !c.isDisappear).Any();
        }

        private float currentTime;

        #endregion

        // Start is called before the first frame update
        void Start()
        {
            currentTime = 0;
            circles.ForEach(circle => circle.Hide());
        }

        // Update is called once per frame
        void Update()
        {
            if (FullDisappear)
            {
                Destroy(gameObject);
            }

            currentTime += Time.deltaTime;
            circles.ForEach(circle =>
            {
                if (circle.isDisappear)
                {
                    return;
                }

                if (!circle.IsActive && currentTime >= circle.appearTime)
                {
                    circle.Appear();
                }

                if (circle.IsActive && currentTime >= circle.disappearTime)
                {
                    circle.Disappear();
                }

                if (circle.IsActive)
                {
                    var step = circle.scaleStep;
                    circle.transform.localScale += new Vector3(step, step);
                }
            });
        }
    }

    /// <summary>
    /// 判定圈
    /// </summary>
    [Serializable]
    public class JudgeCircle
    {
        public Transform transform;

        public bool IsActive
        {
            get => transform.gameObject.activeSelf;
        }

        public float appearTime;

        public float disappearTime;

        [Tooltip("每一次缩放的比例")]
        public float scaleStep = 0.3f;

        public bool isDisappear = false;

        public void Hide()
        {
            transform.gameObject.SetActive(false);
        }

        public void Appear()
        {
            transform.gameObject.SetActive(true);
        }

        public void Disappear()
        {
            transform.gameObject.SetActive(false);
            isDisappear = true;
        }
    }
}
