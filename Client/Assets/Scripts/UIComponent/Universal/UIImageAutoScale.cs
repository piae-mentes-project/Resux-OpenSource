using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Resux.UI
{
    [RequireComponent(typeof(MaskableGraphic))]
    public class UIImageAutoScale : MonoBehaviour
    {
        #region Properties

        [SerializeField] [Tooltip("是否需要主动调用")] private bool isNotAuto;
        [SerializeField] private RectTransform canvasRectTransform;
        private RectTransform rectTransform;

        #endregion

        void Start()
        {
            if (!isNotAuto)
            {
                AutoScale();
            }
        }

        #region Public Method

        /// <summary>
        /// 自动适配缩放
        /// </summary>
        public void AutoScale()
        {
            if (canvasRectTransform == null)
            {
                return;
            }
            var image = GetComponent<MaskableGraphic>();
            rectTransform = GetComponent<RectTransform>();
            var nativeSize = new Vector2(image.mainTexture.width, image.mainTexture.height);
            var canvasSize = canvasRectTransform.sizeDelta;
            // var scaleX = Screen.width / nativeSize.x;
            // var scaleY = Screen.height / nativeSize.y;
            var scaleX = canvasSize.x / nativeSize.x;
            var scaleY = canvasSize.y / nativeSize.y;
            Logger.Log($"scaleX: {scaleX}, scaleY: {scaleY}");
            rectTransform.sizeDelta = nativeSize * Mathf.Max(scaleX, scaleY);
        }

        #endregion
    }
}