using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Resux
{
    /// <summary>
    /// 遮罩动画UI图
    /// </summary>
    public class AnimationUIImage : MonoBehaviour
    {
        #region properties

        [SerializeField] private AnimationCurve speedCurve;
        [SerializeField] private float totalSecond = 1.0f;
        [SerializeField][Tooltip("Can be null")] private Texture2D maskTex;

        private Material material;

        #endregion

        #region Unity

        private void Start()
        {
            material = GetComponent<Image>().material;
        }

        private void OnEnable()
        {
            if (maskTex != null)
            {
                material.SetTexture("_MaskTex", maskTex);
            }

            StartCoroutine(PlayMaskAnimation());
        }

        #endregion

        #region Coroutine

        IEnumerator PlayMaskAnimation()
        {
            var waitFrame = new WaitForEndOfFrame();
            var time = Time.time;
            // 避免浮点数误差导致没法达到真正的1
            var realTotalSecond = totalSecond + 0.05f;
            while (Time.time - time <= realTotalSecond)
            {
                material.SetFloat("_Threshold", 2 * speedCurve.Evaluate(Mathf.Min((Time.time - time) / totalSecond, 1)));
                yield return waitFrame;
            }
        }

        #endregion
    }
}
