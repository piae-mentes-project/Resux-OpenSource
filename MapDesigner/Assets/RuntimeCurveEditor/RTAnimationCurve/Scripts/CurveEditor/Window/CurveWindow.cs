//----------------------------------------------
// Runtime Curve Editor
// Copyright © 2013-2021 Rus Artur PFA
// center@republicofhandball.com
//----------------------------------------------

using System;
using UnityEngine;
using UnityEngine.UI;

namespace RuntimeCurveEditor
{
    /// <summary>
    /// Curve editor's window,draws the window itself and gradations.
    /// </summary>
    public class CurveWindow : MonoBehaviour
    {
        public CurveLines curveLines;//keep a reference to the component drawing the curves

        public Texture2D close;
        public Texture2D textureNS;
        public Texture2D textureWE;
        public Texture2D textureNWSE;
        public Texture2D textureSWNE;
        public Texture2D textureDefault;

        public RectTransform panel;

        public RectTransform headerPanel;

        public RectTransform horGradations;
        public RectTransform verGradations;

        public GameObject number;

        public RectTransform yMaxRect;

        InputField yMaxEditField;

        RectTransform canvas;

        enum ResizeType { No, ResizeNS, ResizeWE, ResizeNWSE, ResizeSWNE };
        bool onLeftEdge;
        bool onRightEdge;
        bool onTopEdge;
        bool onBottomEdge;
        ResizeType mResize = ResizeType.No;

        int screenWidth;
        int screenHeight;

        public static Vector2 hotspot = new Vector2(8, 8);//The offset from the top left of the texture to use as the target point (must be within the bounds of the cursor).

        const float k_hor = 3f;//constant value for initial horizontal scaling

        Vector2 mPrevCursorPos;//(screen coordinates) 

        Rect closeRect = new Rect();//keeps the rect for the close button

        Rect gridRect = Rect.zero;

        const float marginErr = 0.00001f;

        bool mouseDown;
        bool mouseUp;
        bool mouseDrag;

        public bool IsTouchedBegan() {
            if (Input.touchCount == 0) return false;
            return (Input.touches[0].phase == TouchPhase.Began);
        }
        public bool IsDoubleTap() {
            if (Input.touchCount != 1) return false;
            return (Input.touches[0].tapCount == 2);
        }
        public bool IsSingleTap() {
            if (Input.touchCount != 1) return false;
            return (Input.touches[0].tapCount == 1);
        }

        public bool WindowClosed { set; get; }

        float horNumberMaxWidth;
        float verNumberMaxHeight;
        float verNumberMaxWidth;

        int colCount;
        int rowCount;

        float panelHeaderPixels;

        float panelBottomPixels;

        float xLeft;
        float xRight;
        float yTop;
        float yBottom;

        Vector2 ratioScreenCanvas;
        Vector2 invRatioScreenCanvas;

        bool checkBottomNumber;

        const float MIDDLE = 0.5f;

        const string DESTROYED = "destroyed";

        private void OnApplicationQuit() {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
        }

        void Start() {
            textureDefault = null;//remove this line, if you want to use a custom icon for default cursor
            Cursor.SetCursor(textureDefault, Vector2.zero, CursorMode.ForceSoftware);

            canvas = panel.parent.GetComponent<RectTransform>();

            curveLines.InitConstantValues();
                        
            curveLines.TextureNS = textureNS;
            curveLines.TextureDefault = textureDefault;

            horNumberMaxWidth = GetNumberMaxWidthPixels(curveLines.GradRect.xMin, curveLines.GradRect.xMax);//TODO this has to be called, if somehow xMin or xMax are changed at some moment
            curveLines.WidthUnit = horNumberMaxWidth;
            verNumberMaxHeight = number.GetComponent<Text>().fontSize * 2f;

            verNumberMaxWidth = GetNumberMaxWidthPixels(0, curveLines.GradRect.yMax);//TOOD update on ymax change

            Sprite bkgSprite = GetComponent<Image>().sprite;
            panelHeaderPixels = bkgSprite.border.w;
            panelBottomPixels = bkgSprite.border.y;

            UpdateScreenWindowGrid();

            headerPanel.GetComponent<WindowDragging>().curveWindow = this;//TODO to remove once moving CurveWindow component on CurveEditorPanel
            headerPanel.sizeDelta = new Vector2(panel.sizeDelta.x, headerPanel.sizeDelta.y);

            curveLines.enabled = true;

            yMaxEditField = yMaxRect.GetComponent<InputField>();
            yMaxEditField.text = curveLines.GradRect.yMax.ToString();

            if (Screen.dpi > CurveLines.DEFAULT_DPI) {
                curveLines.HeightUnit = verNumberMaxHeight * ((Screen.dpi / CurveLines.DEFAULT_DPI - 1) * 0.35f + 1f);
            } else {
                curveLines.HeightUnit = verNumberMaxHeight;
            }
        }

        void Update() {
            if ((Screen.width != screenWidth) || (Screen.height != screenHeight)) {
                UpdateScreenWindowGrid();
            }

            CheckShowWindowResizeCursor();

            if (Input.GetMouseButtonDown(0)) {
                MouseDownOnUpdate();
            } else if (Input.GetMouseButtonUp(0)) {
                MouseUpOnUpdate();
            } else if (Input.GetMouseButton(0)) {
                MouseDragOnUpdate();
            }
        }

        public void ResetScreenSize() {
            screenWidth = 0;
            screenHeight = 0;
            GetComponent<ZoomBehaviour>().Reset();
        }

        void UpdateScreenWindowGrid() {
            screenWidth = Screen.width;
            screenHeight = Screen.height;
            ratioScreenCanvas.x = screenWidth / canvas.rect.width;
            ratioScreenCanvas.y = screenHeight / canvas.rect.height;
            invRatioScreenCanvas.x = 1f / ratioScreenCanvas.x;
            invRatioScreenCanvas.y = 1f / ratioScreenCanvas.y;
            UpdateWindowAndGrid();
        }

        void UpdateWindowAndGrid() {
            UpdateWindowSizeValues();
            UpdateGrid();
        }

        void UpdateWindowSizeValues() {
            Vector2 size = panel.sizeDelta;
            size.x *= ratioScreenCanvas.x;
            size.y *= ratioScreenCanvas.y;
            xLeft = panel.localPosition.x * ratioScreenCanvas.x - size.x * 0.5f + screenWidth * 0.5f;
            xRight = xLeft + size.x;
            yBottom = panel.localPosition.y * ratioScreenCanvas.y - size.y * 0.5f + screenHeight * 0.5f;
            yTop = yBottom + size.y;
        }

        /// <summary>
        /// Update the grid size
        /// </param>
        void UpdateGrid() {
            gridRect.x = panel.localPosition.x - 0.5f * panel.sizeDelta.x + 3f * horNumberMaxWidth;
            gridRect.width = panel.sizeDelta.x - 4f * horNumberMaxWidth;
            gridRect.y = panel.localPosition.y + GetGridLocalPos();
            gridRect.height = panel.sizeDelta.y - panelBottomPixels - panelHeaderPixels - 2f * verNumberMaxHeight;

            gridRect.x *= ratioScreenCanvas.x;
            gridRect.width *= ratioScreenCanvas.x;
            gridRect.y *= ratioScreenCanvas.y;
            gridRect.height *= ratioScreenCanvas.y;

            Vector2 anchor = (panel.anchorMin + panel.anchorMax) * 0.5f;
            gridRect.x += screenWidth * anchor.x;
            gridRect.y += screenHeight * anchor.y;

            curveLines.UpdateGrid(gridRect);

            float yMin = (panel.localPosition.y - 0.5f * panel.sizeDelta.y) * ratioScreenCanvas.y + screenHeight * anchor.y;
            curveLines.BottomShapesYmin = yMin + panelBottomPixels * ratioScreenCanvas.y * 0.2f;
            curveLines.BottomShapesYmax = yMin + panelBottomPixels * ratioScreenCanvas.y * 0.8f;
        }

        public Vector3 CursorPos() {
            return Input.mousePosition;
        }

        public void OnClose() {
            curveLines.CloseWindow();
        }

        public bool NormalCursorType() {
            return mResize == ResizeType.No;
        }

        void MouseDownOnUpdate() {
            mPrevCursorPos = CursorPos();
        }

        void MouseUpOnUpdate() {
            if (mResize != ResizeType.No) {
                return;
            }

            Vector2 mousePosUp = CursorPos();
            if (closeRect.Contains(mousePosUp)) {
                WindowClosed = true;
                curveLines.WindowClosed = true;
                curveLines.AlterData();
            } else {
                curveLines.MouseUp();
            }
        }

        void MouseDragOnUpdate() {
            Vector2 newCursorPos = CursorPos();
            if (closeRect.Contains(newCursorPos)) {
                return;
            }

            if (mResize == ResizeType.No) {
                newCursorPos.x = Mathf.Clamp(newCursorPos.x, gridRect.xMin, gridRect.xMax);
                newCursorPos.y = Mathf.Clamp(newCursorPos.y, gridRect.yMin, gridRect.yMax);
                curveLines.MouseDrag(newCursorPos - mPrevCursorPos);
            } else {
                Vector2 cursorDiff = newCursorPos - mPrevCursorPos;
                cursorDiff.x *= invRatioScreenCanvas.x;
                cursorDiff.y *= invRatioScreenCanvas.y;
                if (ResizeType.ResizeNS == mResize) {
                    panel.sizeDelta += new Vector2(0, RevY(cursorDiff.y));
                    panel.Translate(0, ScaleY(cursorDiff.y) * 0.5f, 0);
                } else if (ResizeType.ResizeWE == mResize) {
                    panel.sizeDelta += new Vector2(RevX(cursorDiff.x), 0);
                    panel.Translate(ScaleX(cursorDiff.x) * 0.5f, 0, 0);
                } else if (ResizeType.ResizeNWSE == mResize || ResizeType.ResizeSWNE == mResize) {
                    panel.sizeDelta += new Vector2(RevX(cursorDiff.x), RevY(cursorDiff.y));
                    panel.Translate(ScaleX(cursorDiff.x) * 0.5f, ScaleY(cursorDiff.y) * 0.5f, 0);
                }
                headerPanel.sizeDelta = new Vector2(panel.sizeDelta.x, headerPanel.sizeDelta.y);

                UpdateWindowAndGrid();
                curveLines.AlterData();
            }
            mPrevCursorPos = newCursorPos;
        }

        float RevX(float diffX) {
            return onLeftEdge ? -diffX : diffX;
        }

        float RevY(float diffY) {
            return onBottomEdge ? -diffY : diffY;
        }

        float ScaleX(float diffX) {
            return diffX * canvas.localScale.x;
        }

        float ScaleY(float diffY) {
            return diffY * canvas.localScale.y;
        }

        float GetNumberMaxWidthPixels(float val1, float val2) {

            int length1 = val1.ToString().Length;
            int length2 = val2.ToString().Length;
            float maxDigits = (length1 >= length2) ? length1 : length2;

            maxDigits += 2;//considering that in between values are no longer than 2 digits(including '.')

            //float numberWidth = number.GetComponent<RectTransform>().sizeDelta.x;//TODO this should be constant, and so should be calculated only once
            return number.GetComponent<Text>().fontSize * 0.6f * maxDigits;//TODO this should be constant, and so should be calculated only once
        }

        public void OnYmaxUpdate() {
            float value = 0;
            if (float.TryParse(yMaxEditField.text, out value)) {
                curveLines.SetGradRectYMax(value, true);
            }
        }

        void CreateNumber(float value, float anchorY) {
            RectTransform numberRect = (Instantiate(number) as GameObject).GetComponent<RectTransform>();
            numberRect.SetParent(verGradations);
            numberRect.SetAsFirstSibling();

            float numberValue = (float)Math.Round(value, 3);

            numberRect.GetComponent<Text>().text = numberValue.ToString();
            numberRect.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;

            numberRect.localPosition = Vector3.zero;
            numberRect.anchorMin = new Vector2(0.5f, anchorY);
            numberRect.anchorMax = numberRect.anchorMin;

            numberRect.sizeDelta = new Vector2(verNumberMaxWidth, numberRect.sizeDelta.y);

            numberRect.localScale = Vector3.one;
            numberRect.gameObject.name = numberValue.ToString();
        }

        public void UpdatePosition(Vector2 cursorDiff) {
            cursorDiff.x *= invRatioScreenCanvas.x;
            cursorDiff.y *= invRatioScreenCanvas.y;
            panel.Translate(ScaleX(cursorDiff.x), ScaleY(cursorDiff.y), 0);
            UpdateWindowAndGrid();
            curveLines.AlterData();
        }

        float GetGridLocalPos() {
            return panelBottomPixels + 1.25f * verNumberMaxHeight - panel.sizeDelta.y * 0.5f;
        }

        public void UpdateVerGradations(int rowCount, float rezid, bool mirrored, bool gradUpdate) {
            var gradYMax = curveLines.GradRect.yMax;
            var gradYMin = curveLines.GradRect.yMin;
            if (gradUpdate) {
                yMaxEditField.text = gradYMax.ToString();
            }
            verGradations.sizeDelta = new Vector2(1.5f * verNumberMaxWidth, curveLines.EntireRect.height * invRatioScreenCanvas.y);
            float displ = (curveLines.EntireRect.yMin - gridRect.yMin) * invRatioScreenCanvas.y;
            verGradations.localPosition = new Vector2(verGradations.sizeDelta.x * 0.5f - panel.sizeDelta.x * 0.5f, GetGridLocalPos() + verGradations.sizeDelta.y * 0.5f + displ);

            if ((this.rowCount != rowCount) || gradUpdate) {
                this.rowCount = rowCount;

                float normalizedGap = (1f - rezid) / rowCount;
                float gap = (gradYMax - gradYMin) * normalizedGap;
                float normalizedAnchorsGap = normalizedGap * (mirrored ? MIDDLE : 1f);
                foreach (RectTransform numberRect in verGradations) {
                    if (numberRect != yMaxRect) {
                        numberRect.gameObject.name = "DESTROYED";
                        Destroy(numberRect.gameObject);//the destroy takes place at the end of the current frame, not instantly
                    }
                }

                checkBottomNumber = false;
                float anchorStart = mirrored ? MIDDLE : 0f;
                int start = mirrored ? -rowCount : 0;
                if (!Mathf.Approximately(rezid, 0)) {
                    if (mirrored) {
                        CreateNumber(-gradYMax, 0);
                        checkBottomNumber = true;
                    }
                } else {
                    rowCount -= 1;
                }

                for (int i = start; i <= rowCount; ++i) {
                    CreateNumber(gradYMin + i * gap, anchorStart + i * normalizedAnchorsGap);
                }
            }

            foreach (RectTransform numberRect in verGradations) {
                if (numberRect.gameObject.name != DESTROYED) {
                    if (numberRect == yMaxRect) {
                        numberRect.localPosition = new Vector2(numberRect.localPosition.x, (gridRect.yMax - Screen.height * 0.5f) * invRatioScreenCanvas.y - verGradations.localPosition.y - panel.localPosition.y);
                    } else {
                        float numberPosY = (numberRect.localPosition.y + verGradations.localPosition.y + panel.localPosition.y) * ratioScreenCanvas.y + Screen.height * 0.5f;
                        numberRect.gameObject.SetActive((Mathf.Round(gridRect.yMin) <= Mathf.Round(numberPosY)) && (Mathf.Round(numberPosY) <= Mathf.Round(gridRect.yMax)));
                    }
                }
            }

            if (checkBottomNumber) {//decide if bottom number (not the mirroring of the grad max) should be visible or not, if it's too close to the 'the mirroring of the grad max'                         
                float normalizedGap = (1f - rezid) / rowCount;
                float normalizedAnchorsGap = normalizedGap * MIDDLE;
                //verGradations
                Transform bottomNumber = verGradations.GetChild(2 * rowCount);
                bottomNumber.gameObject.SetActive((MIDDLE - rowCount * normalizedAnchorsGap) * verGradations.sizeDelta.y > verNumberMaxHeight * 0.35f);
            }
        }

        public void UpdateHorGradations(int colCount) {
            horGradations.sizeDelta = new Vector2(curveLines.EntireRect.width * invRatioScreenCanvas.x, horGradations.sizeDelta.y);
            Vector3 posTemp = horGradations.localPosition;
            posTemp.x = horNumberMaxWidth + (curveLines.EntireRect.xMin - gridRect.xMin + curveLines.EntireRect.xMax - gridRect.xMax) * 0.5f * invRatioScreenCanvas.x;
            horGradations.localPosition = posTemp;

            float normalizedGap = 1.0f / colCount;
            float gap = (curveLines.GradRect.xMax - curveLines.GradRect.xMin) * normalizedGap;

            if (this.colCount != colCount) {
                this.colCount = colCount;

                foreach (RectTransform numberRect in horGradations) {
                    Destroy(numberRect.gameObject);
                }

                for (int i = 0; i <= colCount; ++i) {
                    RectTransform numberRect = (Instantiate(number) as GameObject).GetComponent<RectTransform>();
                    numberRect.SetParent(horGradations);
                    float numberValue = (float)Math.Round(i * gap + curveLines.GradRect.xMin, 3);
                    numberRect.GetComponent<Text>().text = numberValue.ToString();

                    numberRect.localPosition = Vector3.zero;
                    numberRect.anchorMin = new Vector2(i * normalizedGap, 0.5f);
                    numberRect.anchorMax = numberRect.anchorMin;

                    numberRect.sizeDelta = new Vector2(horNumberMaxWidth, numberRect.sizeDelta.y);

                    numberRect.localScale = Vector3.one;
                    numberRect.gameObject.name = numberValue.ToString();
                }
            }

            foreach (RectTransform numberRect in horGradations) {
                float numberPosX = (numberRect.localPosition.x + horGradations.localPosition.x + panel.localPosition.x) * ratioScreenCanvas.x + Screen.width * 0.5f;
                numberRect.gameObject.SetActive((Mathf.Round(gridRect.xMin) <= Mathf.Round(numberPosX)) && (Mathf.Round(numberPosX) <= Mathf.Round(gridRect.xMax)));
            }
        }

        public Rect GetGridRect() {
            return gridRect;
        }

        public void Zoom(float factor, Vector2 mousePos) {
            curveLines.Zoom(factor, mousePos);
        }

        public void Pan(Vector2 diff) {
            curveLines.Pan(diff);
        }

        //this method gets called each update cycle
        void CheckShowWindowResizeCursor() {
            //show the cursor if it's over any of the window's edges
            bool touch = Input.touchCount == 1;//in case of touch screen(mobile), the resize cursor could be visible only when the touch is actually very close to edge(there's no hovering feature)

            if (touch && (Input.touches[0].phase == TouchPhase.Ended)) {
                DisableResizeCursor();
            }

            if (!(touch && (Input.touches[0].phase == TouchPhase.Began))) {
                if (Input.GetMouseButton(0)) {
                    return;
                } else if (curveLines.MousePosOverContextMenu(CursorPos())) {
                    DisableResizeCursor();
                    return;
                }
            }

            Vector3 cursorPos = CursorPos();
            onLeftEdge = Mathf.Abs(xLeft - cursorPos.x) < CurveLines.marginPixels;
            onRightEdge = Mathf.Abs(xRight - cursorPos.x) < CurveLines.marginPixels;
            onTopEdge = Mathf.Abs(yTop - cursorPos.y) < CurveLines.marginPixels;
            onBottomEdge = Mathf.Abs(yBottom - cursorPos.y) < CurveLines.marginPixels;
            if ((cursorPos.x < xLeft - CurveLines.marginPixels) || (cursorPos.x > xRight + CurveLines.marginPixels) || (cursorPos.y < yBottom - CurveLines.marginPixels) || (cursorPos.y > yTop + CurveLines.marginPixels) ||
                (!onLeftEdge && !onRightEdge && !onTopEdge && !onBottomEdge)) {
                DisableResizeCursor();
            } else if ((onLeftEdge && onTopEdge) || (onRightEdge && onBottomEdge)) {
                mResize = ResizeType.ResizeNWSE;
                Cursor.SetCursor(textureNWSE, hotspot, CursorMode.ForceSoftware);
            } else if ((onLeftEdge && onBottomEdge) || (onRightEdge && onTopEdge)) {
                mResize = ResizeType.ResizeSWNE;
                Cursor.SetCursor(textureSWNE, hotspot, CursorMode.ForceSoftware);
            } else if (onLeftEdge || onRightEdge) {
                mResize = ResizeType.ResizeWE;
                Cursor.SetCursor(textureWE, hotspot, CursorMode.ForceSoftware);
            } else if (onTopEdge || onBottomEdge) {
                mResize = ResizeType.ResizeNS;
                Cursor.SetCursor(textureNS, hotspot, CursorMode.ForceSoftware);
            }
        }

        void DisableResizeCursor() {
            if (mResize != ResizeType.No) {
                //Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                Cursor.SetCursor(textureDefault, Vector2.zero, CursorMode.ForceSoftware);
                mResize = ResizeType.No;
            }
        }

    }
}