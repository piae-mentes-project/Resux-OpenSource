using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Resux.UI
{
    /// <summary>
    /// 屏幕适配脚本
    /// </summary>
    public class ScreenAdaptation : MonoBehaviour
    {
        public static ScreenAdaptation Instance;

        #region properties

        #region Static Properties

        public static (int width, int height) StandardSize = (1920, 1080);
        /// <summary>标准尺寸缩放后的尺寸</summary>
        public static (int width, int height) ScaledSize;
        /// <summary>标准宽高比</summary>
        public const float StandardAspectRatio = 16f / 9f;
        /// <summary>屏幕以标准尺寸为基准的两轴分别的单位缩放值（假设x为1920或y为1080）</summary>
        public static (float xScale, float yScale, float aspectRatio) ScreenScale;
        /// <summary>安全区以标准尺寸为基准的两轴分别的单位缩放值（假设x为1920或y为1080）</summary>
        public static (float xScale, float yScale, float aspectRatio) SafeAreaScale;
        /// <summary>屏幕尺寸/标准尺寸</summary>
        public static (float xScale, float yScale) sizeScale;
        /// <summary>安全区尺寸/标准尺寸</summary>
        public static (float xScale, float yScale) safeSizeScale;
        /// <summary>屏幕尺寸/缩放后的标准尺寸</summary>
        public static (float xScale, float yScale) scaledSizeScale;
        /// <summary>屏幕尺寸/（可能）纵向缩放后的标准尺寸</summary>
        public static (float xScale, float yScale) screenScaledSizeScale;
        /// <summary>缩放后的标准尺寸/标准尺寸</summary>
        public static (float xScale, float yScale) standardScaledSizeScale;
        public static Vector2 UnAdaptationCenter = new Vector2(StandardSize.width / 2, StandardSize.height / 2);
        public static Vector2 CurrentSafeAreaSize;
        public static Vector2 ScreenSize;
        /// <summary>分辨率高度的缩放值</summary>
        public static float HeightScale;
        /// <summary>分辨率宽度的缩放值</summary>
        public static float WidthScale;

        /// <summary>安全区中心偏移(1920*1080)</summary>
        private static Vector3 safeCenterOffset;
        /// <summary>安全区中心原始偏移（屏幕尺寸，用于输入）</summary>
        private static Vector3 safeCenterRawOffset;
        /// <summary>用于判定适配的偏移，为以安全区中心为中心的16：9范围的左下角的x坐标</summary>
        private static float xOffset;

        #endregion

        [SerializeField] private CanvasScaler canvasScaler;

        [SerializeField] private Camera playerCamera;

        [SerializeField] private RectTransform[] otherSafeAreas;

        #endregion

        void Awake()
        {
            Instance = this;
            CalculateParameters();
            //ReAdaptationScreen();
            AdaptationSafeAreaPosition();
            AdaptationCameraPosition();
            if (otherSafeAreas != null && otherSafeAreas.Length > 0)
            {
                foreach (var safeArea in otherSafeAreas)
                {
                    AdaptationSafeAreaPosition(safeArea);
                }
            }
        }

        public void CalculateParameters()
        {
            var safeArea = Screen.safeArea;
            CurrentSafeAreaSize = Screen.safeArea.size;
            ScreenSize = new Vector2(Screen.width, Screen.height);

            safeCenterRawOffset = safeCenterOffset = safeArea.position - (ScreenSize - CurrentSafeAreaSize) / 2;
            HeightScale = CurrentSafeAreaSize.y / StandardSize.height;
            WidthScale = CurrentSafeAreaSize.x / StandardSize.width;

            Logger.Log($"safe area size : {CurrentSafeAreaSize},  safe pos : {safeArea.position}, safe center : {safeCenterOffset}");

            sizeScale = (ScreenSize.x / StandardSize.width, ScreenSize.y / StandardSize.height);
            safeSizeScale = (CurrentSafeAreaSize.x / StandardSize.width, CurrentSafeAreaSize.y / StandardSize.height);
            var aspectRatio = CurrentSafeAreaSize.x / CurrentSafeAreaSize.y;
            var xScale = aspectRatio / StandardAspectRatio;
            var yScale = StandardAspectRatio / aspectRatio;
            SafeAreaScale = (xScale, yScale, aspectRatio);
            aspectRatio = ScreenSize.x / ScreenSize.y;
            xScale = aspectRatio / StandardAspectRatio;
            yScale = StandardAspectRatio / aspectRatio;
            ScreenScale = (xScale, yScale, aspectRatio);
            
            // 宽屏
            if (SafeAreaScale.xScale - 1 > 1e-3)
            {
                // 以安全区尺寸为基准，但画面还是要铺满整个屏幕
                ScaledSize = ((int) (StandardSize.width * ScreenScale.xScale + 0.5f), StandardSize.height);
                safeCenterOffset.x = safeCenterOffset.x * ScaledSize.width / ScreenSize.x;
                xOffset = Mathf.Abs(safeCenterOffset.x) + safeCenterOffset.x + StandardSize.width * (SafeAreaScale.xScale - 1) / 2;
            }
            // 高屏
            else if (SafeAreaScale.yScale - 1 > 1e-3)
            {
                // 安全区高屏的情况下还是异型屏
                if (safeCenterRawOffset.magnitude > 1)
                {
                    xScale = ScreenSize.x / CurrentSafeAreaSize.x;
                    ScaledSize = ((int) (StandardSize.width * xScale + 0.5f), (int)(StandardSize.height * SafeAreaScale.yScale + 0.5f));
                    safeCenterOffset.x = safeCenterOffset.x * ScaledSize.width / ScreenSize.x;
                    xOffset = Mathf.Abs(safeCenterOffset.x) + safeCenterOffset.x;
                }
                else
                {
                    ScaledSize = (StandardSize.width, (int)(StandardSize.height * ScreenScale.yScale + 0.5f));
                    safeCenterOffset.x = safeCenterOffset.x * ScaledSize.width / ScreenSize.x;
                }
            }
            else
            {
                ScaledSize = StandardSize;
                safeCenterOffset.x = safeCenterOffset.x * ScaledSize.width / ScreenSize.x;
            }
            Logger.Log($"offset: {xOffset}");
            safeCenterOffset.y = safeCenterOffset.y * ScaledSize.height / ScreenSize.y;
            scaledSizeScale = (ScreenSize.x / ScaledSize.width, ScreenSize.y / ScaledSize.height);
            screenScaledSizeScale = (ScreenSize.x / StandardSize.width, ScreenSize.y / ScaledSize.height);
            standardScaledSizeScale = (ScaledSize.width / (float)StandardSize.width, ScaledSize.height / (float)StandardSize.height);
        }

        public void AdaptationSafeAreaPosition()
        {
            var rectTransform = GetComponent<RectTransform>();
            AdaptationSafeAreaPosition(rectTransform);
        }

        public void AdaptationSafeAreaPosition(RectTransform rectTransform)
        {
            rectTransform.position += safeCenterOffset;
            var width = rectTransform.rect.width;
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width * SafeAreaScale.xScale);
        }

        public void AdaptationCameraPosition()
        {
            if (playerCamera == null)
            {
                return;
            }

            var pos = playerCamera.transform.position;
            // 高屏
            // if (SafeAreaScale.yScale - 1 > 1e-3)
            // {
            //     pos.y *= ScreenScale.yScale;
            // }
            pos.y = playerCamera.orthographicSize = ScaledSize.height / 2;
            Logger.Log($"screen y scale: {ScreenScale.yScale}, scaled size: ({ScaledSize.width}, {ScaledSize.height})");
            // pos.x = ScaledSize.width / 2;
            // 相机左移对应画面右移
            pos -= safeCenterOffset;
            playerCamera.transform.position = pos;
        }

        /// <summary>
        /// 重置屏幕适配
        /// </summary>
        public void ReAdaptationScreen()
        {
            var safeArea = Screen.safeArea;
            Logger.Log($"safeArea: {safeArea}, screenSize: {ScreenSize}");
            var rectTransform = GetComponent<RectTransform>();
            var playSize = safeArea.size;
            Vector3 safeCenter = safeArea.position - (ScreenSize - playSize) / 2;

            Logger.Log($"safeCenter: {safeCenter}");
            canvasScaler.referenceResolution = playSize;

            Logger.Log($"position: {rectTransform.position} plus safeCenter: {rectTransform.position + safeCenter}");
            rectTransform.sizeDelta = playSize;
            rectTransform.position += safeCenter;
            CurrentSafeAreaSize = playSize;
            HeightScale = playSize.y / StandardSize.height;

            // Debugger.Log(safeArea.center);
            Logger.Log($"playSize: {playSize}");

            // 这里是相机的跟随适配
            if (playerCamera == null)
            {
                return;
            }

            // 原始位置： 1920/2 = 960， 1080/2 = 540
            Vector3 cameraPos = AdaptationJudgePosition(UnAdaptationCenter);
            cameraPos.z = playerCamera.transform.position.z;
            playerCamera.transform.position = cameraPos;
            playerCamera.orthographicSize = playSize.y / 2;
        }

        #region static Method

        /// <summary>
        /// 根据适配获取适配后的坐标
        /// </summary>
        /// <param name="pos">经过<see cref="AdaptationNotePosition(Vector2)"/>缩放后的位置坐标</param>
        /// <returns>新位置坐标</returns>
        public static Vector2 AdaptationJudgePosition(Vector2 pos)
        {
            // Debugger.Log($"原本判定坐标（适配后）： {pos}");
            pos.x += xOffset;

            pos.y *= scaledSizeScale.yScale;
            pos.x *= scaledSizeScale.xScale;
            // Debugger.Log($"实际判定坐标（按分辨率和安全区适配）： {pos}");

            return pos;
        }

        public static Vector2 AdaptationNotePosition(Vector2 pos)
        {
            // Debugger.Log($"原本坐标： {pos}");
            // 高屏
            if (SafeAreaScale.yScale - 1 > 1e-3)
            {
                pos.y *= standardScaledSizeScale.yScale;
            }
            // Debugger.Log($"屏幕适配后实际坐标： {pos}");

            return pos;
        }

        /// <summary>
        /// 简单的按照比例缩放一个vector2
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static Vector2 Scale(Vector2 pos)
        {
            pos.y *= scaledSizeScale.yScale;
            pos.x *= scaledSizeScale.xScale;
            return pos;
        }

        #endregion
    }
}
