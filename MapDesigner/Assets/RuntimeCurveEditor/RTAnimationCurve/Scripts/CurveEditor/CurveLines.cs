//----------------------------------------------
// Runtime Curve Editor
// Copyright © 2013-2021 Rus Artur PFA
// center@republicofhandball.com
//----------------------------------------------
using UnityEngine;
using System.Collections.Generic;

namespace RuntimeCurveEditor
{

    /// <summary>
    /// Draws lines inside curve editor and manage user interaction with curves and keys
    /// </summary>
    public class CurveLines : MonoBehaviour, InterfaceContextMenuListener, InterfaceKeyEditListener//, InterfacePostRenderer
    {
        public Material lineMaterial;//material used for drawing lines

        public CurveWindow curveWindow;

        public Texture2D TextureDefault { set; private get; }

        public float BottomShapesYmin { set; private get; }
        public float BottomShapesYmax { set; private get; }

        //unit in world coordinates(constant in pixels), used for calculating the number of lines that should be visible in the grid
        public float WidthUnit { private get; set; }
        public float HeightUnit { private get; set; }

        public static float DEFAULT_DPI = 96f;

        public KeyHandling KeyHandling { get; private set; }

        List<CurveForm> curveFormList = new List<CurveForm>();

        //possible colors when drawing more curves 
        Color[] colors = { Color.red, Color.green, Color.yellow, Color.blue, Color.magenta, Color.cyan };

        //list with the colors currently used
        List<Color> usedColorList = new List<Color>();

        //grid rect in world coordinates
        Rect gridRect;
        Rect adjGridRect;

        public Rect EntireRect { get; private set; }//on zooming mGridRect remains unchanged, but mEntireRect keeps its size multiplied by the factor of zooming

        float prevEntireRectXMin;
        float prevEntireRectYMin;
        float prevEntireRectHeight;
        float prevEntireRectWidth;

        //rezidual value (normalized), used when calculating the number of horyzontal lines to be displayed
        float mRezid;

        int mHorLines = 2;//have to know how many lines are displayed for the current size of the grid (actually lines+1 will be total number of displayed lines)

        static Color lineColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);

        //Rect given by the gradations limits
        Rect gradRect;
        Rect prevGradRect;
        public Rect GradRect {
            get {
                return gradRect;
            }
            set {
                prevGradRect = gradRect;
                gradRect = value;
            }
        }

        /// <summary>使用限制初始范围的Rect</summary>
        public bool UseInitLimitRect { get; set; }
        public Rect InitLimitRect { get; set; }

        // keeps the form with the active curve (it shouldn't ever be null)
        public CurveForm ActiveCurveForm { get; private set; }

        bool lineDragged;//line touched/pressed, for moving that line
        bool keyDragged;//key touched/pressed, for moving that key
        int selectedKeyIndex = KeyHandling.UNSELECTED;//the index of the selected key, UNSELECTED if none is selected (if keySelected is true, than this is the key whose's moved by the user)
        bool isTangentSelected;//true if the user's now selecting a tangent
        bool leftTangetSelected;//if true the left tangent is selected, else the right tangent is selected(this is used only when tangentSelected is true)
        bool multipleKeysMove;

        ResizePart multipleKeysResize;
        Vector2 selectedKeyStartingPos;//case of single key movement(pos in time/value space)
        List<float> lineDragKeysStartingValue;//case of curve movement
        List<Vector2> selectedKeysStartingPos;//case of more keys movement(pos in time/value space)

        public Texture2D TextureNS { private get; set; }//the cursor used when draging the whole line(curve)

        public GameObject contextMenuKeyObject;
        public GameObject keyValueObject;

        RectTransform contextMenuKey;
        RectTransform contextMenuKeyPanel;

        KeyValue keyValue;

        bool mMidHor;//particular use when choosing how dense the grid horyzontal lines will be displayed

        Vector2 DEFAULT_WINDOW_SIZE = new Vector2(320, 240);

        const float TANG_LENGTH_REF = 50f;
        const float MARGIN_PIXELS_REF = 5f;
        const float BASIC_SHAPE_WIDTH_PIXELS_REF = 35f;
        const float BASIC_SHAPE_SPACE_PIXELS_REF = 6f;

        public static float tangFloat = TANG_LENGTH_REF;//the length of tangents when the respective key is selected 
        public static float marginPixels = MARGIN_PIXELS_REF;//needed when mouse selecting lines, points, tangents...
        static float sqrMarginPixels = marginPixels * marginPixels;

        const float marginErr = 1E-5f;

        float basicShapeWidthPixels = BASIC_SHAPE_WIDTH_PIXELS_REF;//the width of the rectangles keeping the curves basic shapes	
        float basicShapesSpacePixels = BASIC_SHAPE_SPACE_PIXELS_REF;//the space in pixels between two consecutive basic shapes	

        const int SHAPE_COUNT = 9;
        AnimationCurve[] basicShapes = new AnimationCurve[SHAPE_COUNT];
        Rect[] basicShapesRect = new Rect[SHAPE_COUNT];
        Rect normalRect = new Rect(0, 0, 1, 1);//defines basic animation curves in this rect
        float[] basicShapeClips = new float[SHAPE_COUNT];

        const int leftTangIndex = 5;
        const int rightTangIndex = 6;
        const int bothTangIndex = 7;

        bool showCursorNormal = true;

        ContextMenuManager contextMenuManager = new ContextMenuManager();

        ContextMenuUI contextMenuUI;

        Vector2 addKeyPos;

        public bool WindowClosed { get; set; } = true;

        enum ContextOptions { clamped, auto, freesmooth, broken }
        ContextOptions lastSelectedOption = ContextOptions.freesmooth;

        public bool AlteredData { get; private set; }

        int vertLines;
        int vertLineStones;
        float vertLinesAlpha;
        float vertLinesGap;

        float mAlpha;

        bool mMirroredHor;
        int mSegmentsHor;
        float mSampleHor;
        float mStartHor;

        MultipleKeySelection multipleKeySelection;

        UndoRedo undoRedo;

        System.Action onAlterAction;

        Camera currentCamera;

        Mesh meshGridLines;
        Mesh meshBasicCurveQuads;

        const float Z_LINE_POS = 0f;

        Color HALF_GRAY = new Color(0.15f, 0.15f, 0.15f, 0.4f);
        Color LIGHT_GRAY = new Color(0.5f, 0.5f, 0.5f, 0.75f);

        public event System.Action onWindowOpen;
        public event System.Action onWindowClose;
        public event System.Action onCurveChanged;

        void Awake()
        {
            meshGridLines = new Mesh();
            meshBasicCurveQuads = new Mesh();
        }

        public void InitConstantValues() {
            float adjCurves = 0.6f;
            if (Screen.dpi != 0) {
                float adj = Screen.dpi / DEFAULT_DPI;
                marginPixels = adj * MARGIN_PIXELS_REF;
                sqrMarginPixels = marginPixels * marginPixels;
                if (adj > 1) {
                    adj *= 0.5f;
                    if (adj < 1) {
                        adj = 1;
                    }
                    adjCurves /= adj;
                    tangFloat = adj * TANG_LENGTH_REF;
                    adj *= 0.75f;
                    basicShapeWidthPixels = adj * BASIC_SHAPE_WIDTH_PIXELS_REF;
                    basicShapesSpacePixels = adj * BASIC_SHAPE_SPACE_PIXELS_REF;
                }
            }
            Curves.margin = marginPixels * adjCurves;
        }

        void FillListColor() {
            foreach (Color color in colors) {
                usedColorList.Add(color);
            }
        }

        void Start() {

            if (usedColorList.Count == 0) {
                FillListColor();
            }
            if (GradRect == Rect.zero) {
                GradRect = Rect.MinMaxRect(CurveForm.X_MIN, CurveForm.Y_MIN, CurveForm.X_MAXIM, CurveForm.Y_MAXIM);//switched ymin to ymax
            }

            SetupBasicShapes();

            Curves.dictCurvesContextMenus = contextMenuManager.dictCurvesContextMenus;
            Curves.lineMaterial = lineMaterial;

            if (ActiveCurveForm == null) {
                ActiveCurveForm = new CurveForm();
            }

            contextMenuKey = Instantiate(contextMenuKeyObject).GetComponent<RectTransform>();
            keyValue = Instantiate(keyValueObject).GetComponent<KeyValue>();
            contextMenuUI = contextMenuKey.GetComponent<ContextMenuUI>();

            multipleKeySelection = GetComponent<MultipleKeySelection>();

            KeyHandling = new KeyHandling(this);
            undoRedo = new UndoRedo();
            
            currentCamera = GetComponent<Camera>();
            Curves.camera = currentCamera;
        }

        void SetupBasicShapes() {

            //create the animation curves coresponding the basic shapes
            basicShapes[0] = new AnimationCurve();
            basicShapes[0].AddKey(0f, 0.5f);

            basicShapes[1] = new AnimationCurve();
            basicShapes[1].AddKey(0f, 0f);
            basicShapes[1].AddKey(1f, 1f);

            basicShapes[2] = new AnimationCurve();
            basicShapes[2].AddKey(0f, 1f);
            basicShapes[2].AddKey(1f, 0f);

            basicShapes[3] = new AnimationCurve();
            Keyframe keyframe = new Keyframe(0, 0);
            keyframe.outTangent = 0;
            basicShapes[3].AddKey(keyframe);
            keyframe = new Keyframe(1, 1);
            keyframe.inTangent = 2;
            basicShapes[3].AddKey(keyframe);

            basicShapes[4] = new AnimationCurve();
            keyframe = new Keyframe(0, 1);
            keyframe.outTangent = -2;
            basicShapes[4].AddKey(keyframe);
            keyframe = new Keyframe(1, 0);
            keyframe.inTangent = 0;
            basicShapes[4].AddKey(keyframe);

            basicShapes[5] = new AnimationCurve();
            keyframe = new Keyframe(0, 0);
            keyframe.outTangent = 2;
            basicShapes[5].AddKey(keyframe);
            keyframe = new Keyframe(1, 1);
            keyframe.inTangent = 0;
            basicShapes[5].AddKey(keyframe);

            basicShapes[6] = new AnimationCurve();
            keyframe = new Keyframe(0, 1);
            keyframe.outTangent = 0;
            basicShapes[6].AddKey(keyframe);
            keyframe = new Keyframe(1, 0);
            keyframe.inTangent = -2;
            basicShapes[6].AddKey(keyframe);

            basicShapes[7] = new AnimationCurve();
            keyframe = new Keyframe(0, 0);
            keyframe.outTangent = 0;
            basicShapes[7].AddKey(keyframe);
            keyframe = new Keyframe(1, 1);
            keyframe.inTangent = 0;
            basicShapes[7].AddKey(keyframe);

            basicShapes[8] = new AnimationCurve();
            keyframe = new Keyframe(0, 1);
            keyframe.outTangent = 0;
            basicShapes[8].AddKey(keyframe);
            keyframe = new Keyframe(1, 0);
            keyframe.inTangent = 0;
            basicShapes[8].AddKey(keyframe);
        }

        /// <summary>
        /// On each update, check if the mouse is clicked, if so check what (a basic shape, a tangent, key or a whole line).
        /// Also make updates of the mouse context menus list, when new keys are added/deleted.
        /// </summary>
        void Update() {
            //check if new keys have been added/deleted outside of curve
            if (ActiveCurveForm.curve1 != null) {
                if (ActiveCurveForm.firstCurveSelected ? (ActiveCurveForm.curve1.length != ActiveCurveForm.curve1KeysCount) :
                    (ActiveCurveForm.curve2.length != ActiveCurveForm.curve2KeysCount)) {
                    selectedKeyIndex = KeyHandling.UNSELECTED;//just be sure that selected key is not an out of range key
                    UpdateCurveKeys(ActiveCurveForm.curve1);
                }
            }
            //update the list of context menus, when key has been added/deleted
            for (int i = 0; i < curveFormList.Count; ++i) {
                CurveForm curveForm = curveFormList[i];
                if (curveForm.curve1.length != curveForm.curve1KeysCount) {
                    contextMenuManager.UpdateDictContextMenu(curveForm.curve1, curveForm.curve1.length - curveForm.curve1KeysCount);
                    curveForm.curve1KeysCount = curveForm.curve1.length;
                    curveFormList.RemoveAt(i);
                    curveFormList.Insert(i, curveForm);
                    if (curveForm.curve1 == ActiveCurveForm.curve1) {
                        ActiveCurveForm = curveForm;
                    }
                    AlterCurveData(curveForm.curve1);
                } 

                if (curveForm.curve2 != null && curveForm.curve2.length != curveForm.curve2KeysCount) {
                    contextMenuManager.UpdateDictContextMenu(curveForm.curve2, curveForm.curve2.length - curveForm.curve2KeysCount);
                    curveForm.curve2KeysCount = curveForm.curve2.length;
                    curveFormList.RemoveAt(i);
                    curveFormList.Insert(i, curveForm);
                    if (curveForm.curve2 == ActiveCurveForm.curve2) {
                        ActiveCurveForm = curveForm;
                    }
                    AlterCurveData(curveForm.curve2);
                }
            }

            //now check user interaction
            if (curveWindow.IsTouchedBegan() || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) {
                if (keyValue.IsKeyEditVisible() && !keyValue.FocusOnInputFields()) {
                    keyValue.SetKeyEditEnabled(false);
                }
                Vector2 mousePos = curveWindow.CursorPos();
                if (!keyValue.IsKeyEditVisible() && !contextMenuUI.Hover(mousePos)) {
                    //check first if the user tries to drag the tangent of the selected key (these should be selectable even if they are outside of the grid)
                    CheckMouseTangentSelection(mousePos);
                    if (!isTangentSelected) {
                        for (int i = 0; i < SHAPE_COUNT; ++i) {
                            if (basicShapesRect[i].Contains(mousePos)) {
                                undoRedo.AddOperation(new BasicShapeOperation(this));
                                ReplaceActiveCurve(basicShapes[i]);
                                selectedKeyIndex = KeyHandling.UNSELECTED;
                                break;
                            }
                        }
                        if ((ActiveCurveForm.curve1 != null) && (gridRect.xMin - marginPixels < mousePos.x) && (gridRect.xMax + marginPixels > mousePos.x) &&
                            (gridRect.yMin - marginPixels < mousePos.y) && (gridRect.yMax + marginPixels > mousePos.y)) {

                            bool anyCurveSelected = false;
                            if (!CheckMouseSelection(mousePos, ActiveCurveForm)) {
                                foreach (CurveForm curveForm in curveFormList) {
                                    if (curveForm.curve1 == ActiveCurveForm.curve1) continue;
                                    if (CheckMouseSelection(mousePos, curveForm)) {
                                        anyCurveSelected = true;
                                        break;
                                    }
                                }
                            } else {
                                anyCurveSelected = true;
                            }


                            if (multipleKeySelection.MultipleKeysAreSelected()) {

                                if (Input.GetMouseButtonDown(0))
                                {
                                    if (multipleKeySelection.InsideSelectedKeys())
                                    {
                                        multipleKeysMove = true;
                                    } else {
                                        multipleKeysResize = multipleKeySelection.OnResizingLines();
                                    }
                                }

                                if (multipleKeysMove || (multipleKeysResize != ResizePart.None))
                                {
                                    PrepareSelectedKeysForMovement();
                                } else {
                                    multipleKeySelection.ClearMultipleKeySelection();
                                }
                            }
                            if (!multipleKeysMove && (multipleKeysResize == ResizePart.None) && !anyCurveSelected && Input.GetMouseButtonDown(0)) {
                                multipleKeySelection.StartMultipleKeySelection();
                            }
                        }
                    }
                }
            }

            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) {
                if (Input.GetKeyUp(KeyCode.Z)) {
                    Undo();
                } else if (Input.GetKeyUp(KeyCode.Y)) {
                    Redo();
                }
            }
            
            DrawGridAndLines();
        }

        void PrepareSelectedKeysForMovement()
        {
            AnimationCurve curve = ActiveCurveForm.SelectedCurve();
            List<int> selectedKeyIndices = multipleKeySelection.SelectedKeyIndices();
            selectedKeysStartingPos = new List<Vector2>(selectedKeyIndices.Count);
            foreach (int index in selectedKeyIndices)
            {
                Keyframe keyframe = curve[index];
                selectedKeysStartingPos.Add(new Vector2(keyframe.time, keyframe.value));
            }
        }

        public void Undo() {
            undoRedo.Undo();
        }

        public void Redo() {
            undoRedo.Redo();
        }

        public void ShowWindow() {
            if (WindowClosed) {
                EnableWindow(true);
                onWindowOpen?.Invoke();
            }
        }

        public void CloseWindow() {
            if (!WindowClosed) {
                EnableWindow(false);
                if ((multipleKeySelection != null) && multipleKeySelection.MultipleKeysAreSelected()) {
                    multipleKeySelection.ClearMultipleKeySelection();
                }

                onWindowClose?.Invoke();
            }
        }

        void EnableWindow(bool enable) {
            WindowClosed = !enable;
            curveWindow.WindowClosed = WindowClosed;
            enabled = enable;
            curveWindow.transform.parent.gameObject.SetActive(enable);
            AlterData();
        }

        void UpdateCurveKeys(AnimationCurve animCurve) {
            float ratio = gradRect.height * prevGradRect.width / (gradRect.width * prevGradRect.height);
            for (int i = 0; i < animCurve.length; ++i) {
                Keyframe keyframe = animCurve[i];
                keyframe.value = (keyframe.value - prevGradRect.yMin) * gradRect.height / prevGradRect.height + gradRect.yMin;
                keyframe.inTangent *= ratio;
                keyframe.outTangent *= ratio;
                animCurve.MoveKey(i, keyframe);
            }

            onCurveChanged?.Invoke();
        }
        
        public void UpdateActiveCurveKeys() {
            if ((gradRect.width != 0f) && (gradRect.height != 0f) && (ActiveCurveForm.curve1 != null)) {
                UpdateCurveKeys(ActiveCurveForm.curve1);
                if (ActiveCurveForm.curve2 != null) {
                    UpdateCurveKeys(ActiveCurveForm.curve2);
                }
            }
        }

        public void SetGradRectAndActiveForm(Rect gradRect) {
            GradRect = gradRect;
            if (ActiveCurveForm != null) {
                ActiveCurveForm.gradRect = gradRect;
            }
        }

        /// <summary>
        /// Adds a new curve form, with references to the given curve1 (the usual case is of a single curve shown, when the curve2 is null).
        /// </param>
        public void AddCurveForm(AnimationCurve curve1, AnimationCurve curve2) {
            //if there is a curve form having the curve1, then only the second curve is updated
            CurveForm curveForm = curveFormList.Find(x => x.curve1 == curve1);
            if (curveForm == null) {
                if (usedColorList.Count == 0) {
                    FillListColor();
                }
                curveForm = new CurveForm(curve1, curve2, usedColorList[0]);
                if (ActiveCurveForm != null) {
                    GradRect = curveForm.gradRect;//always set the grad to the default values
                }
                curveFormList.Add(curveForm);
                usedColorList.RemoveAt(0);
                if (usedColorList.Count == 0) {
                    FillListColor();
                }
                selectedKeyIndex = KeyHandling.UNSELECTED;
            } else {
                curveForm.curve2 = curve2;
                if ((curve2 == null) && !curveForm.firstCurveSelected) {
                    selectedKeyIndex = KeyHandling.UNSELECTED;
                    curveForm.firstCurveSelected = true;
                }
            }

            if ((multipleKeySelection != null) && multipleKeySelection.MultipleKeysAreSelected()) {
                multipleKeySelection.ClearMultipleKeySelection();
            }

            AddContextMenuStructs(curve1);
            AddContextMenuStructs(curve2);

            if (undoRedo != null) {
                undoRedo.ClearStack();//add curve has no undo/redo support(because is external) so clear the stack
            }

            AlterData();
            ActiveCurveForm = curveForm;
        }

        public void AddRestoredCurveForm(AnimationCurve curve1, AnimationCurve curve2, Rect gradRect) {
            AddCurveForm(curve1, curve2);
            ActiveCurveForm.gradRect = gradRect;
            GradRect = gradRect;
        }

        public void RestoreActiveCurve(AnimationCurve curve) {
            if (curve != ActiveCurveForm.curve1) {
                CurveForm curveForm = curveFormList.Find(x => x.curve1 == curve);
                if (curveForm != null) {
                    ActiveCurveForm = curveForm;
                    if ((multipleKeySelection != null) && multipleKeySelection.MultipleKeysAreSelected()) {
                        multipleKeySelection.ClearMultipleKeySelection();
                    }
                    GradRect = curveForm.gradRect;
                }
            }
        }

        public void ResetRect() {
            prevEntireRectXMin = 0;
            prevEntireRectYMin = 0;
            prevEntireRectHeight = 0;
            prevEntireRectWidth = 0;
            gridRect = Rect.zero;
            prevGradRect = Rect.zero;
        }

        void AddContextMenuStructs(AnimationCurve curve) {
            contextMenuManager.AddContextMenuObjects(curve);
        }

        /// <summary>
        /// Remove the curve form related to the given curve.
        /// </param>
        public void RemoveCurve(AnimationCurve curve) {
            CurveForm curveForm = curveFormList.Find(x => x.curve1 == curve);
            if (curveForm != null) {
                usedColorList.Insert(0, curveForm.color);
                curveFormList.Remove(curveForm);
                contextMenuManager.Remove(curve);

                if (curveFormList.Count == 0) {
                    ActiveCurveForm = new CurveForm();
                    GradRect = ActiveCurveForm.gradRect;
                } else {
                    UpdateActiveCurveForm(curveFormList[0]);
                }
                if ((multipleKeySelection != null) && multipleKeySelection.MultipleKeysAreSelected()) {
                    multipleKeySelection.ClearMultipleKeySelection();
                }
                selectedKeyIndex = KeyHandling.UNSELECTED;
                undoRedo.ClearStack();//remove curve has no undo/redo support(because is external) so clear the stack
                AlterData();
            }
        }

        /// <summary>
        /// Replace the active curve when the user clicks on a basic shape.
        /// </param>
        void ReplaceActiveCurve(AnimationCurve curve) {
            ReplaceActiveCurve(curve.keys);
        }

        public void ReplaceActiveCurve(Keyframe[] keyframes, List<ContextMenu> tempCurveContextMenus) {
            AnimationCurve curve = ActiveCurveForm.SelectedCurve();
            while (curve.length > 0) {
                curve.RemoveKey(0);//remove all the keys
            }
            for (int i = 0; i < keyframes.Length; ++i) {
                Keyframe keyframe = keyframes[i];
                curve.AddKey(keyframe);
            }
            contextMenuManager.dictCurvesContextMenus[curve] = tempCurveContextMenus;
            SetActiveCurveFormKeyCount(curve.length);
        }

        void ReplaceActiveCurve(Keyframe[] keyframes) {
            if (ActiveCurveForm.curve1 != null) {
                var usingGradRect = UseInitLimitRect ? InitLimitRect : gradRect;
                AnimationCurve newCurve = ActiveCurveForm.firstCurveSelected ? ActiveCurveForm.curve1 : ActiveCurveForm.curve2;
                while (newCurve.length > 0) {
                    newCurve.RemoveKey(0);//remove all the keys
                }

                float ratio = usingGradRect.height * normalRect.width / (usingGradRect.width * normalRect.height);
                for (int i = 0; i < keyframes.Length; ++i) {
                    Keyframe keyframe = keyframes[i];
                    keyframe.value = (keyframe.value - normalRect.yMin) * usingGradRect.height / normalRect.height + usingGradRect.yMin;
                    keyframe.time = (keyframe.time - normalRect.xMin) * usingGradRect.width / normalRect.width + usingGradRect.xMin;
                    keyframe.inTangent *= ratio;
                    keyframe.outTangent *= ratio;
                    newCurve.AddKey(keyframe);
                }

                contextMenuManager.UpdateContextMenuList(newCurve);
                SetActiveCurveFormKeyCount(newCurve.length);
            }
        }

        void SetActiveCurveFormKeyCount(int length) {
            if (ActiveCurveForm.firstCurveSelected) {
                ActiveCurveForm.curve1KeysCount = length;
            } else {
                ActiveCurveForm.curve2KeysCount = length;
            }
            AlterData();
            if (multipleKeySelection.MultipleKeysAreSelected()) {
                multipleKeySelection.ClearMultipleKeySelection();
            }
        }

        /// <summary>
        /// True if the given curve is visible in the editor.
        /// </param>
        public bool CurveShown(AnimationCurve curve) {
            CurveForm curveForm = curveFormList.Find(x => x.curve1 == curve);
            return (curveForm != null) && (curveForm.curve1 != null) && (curveForm.curve2 == null);
        }

        /// <summary>
        /// True if the given curves are added as a path to the editor.
        /// </param>
        public bool CurvesShown(AnimationCurve curve1, AnimationCurve curve2) {
            CurveForm curveForm = curveFormList.Find(x => x.curve1 == curve1);
            return (curveForm != null) && (curveForm.curve1 != null) && (curveForm.curve2 != null);
        }

        public void SetGradRectYMax(float yMax, bool addToUndoStack = false) {
            if (addToUndoStack) {
                undoRedo.AddOperation(new GradChangeOperation(this, GradRect.yMax));
            }
            Rect gradRectTemp = GradRect;
            gradRectTemp.yMax = yMax;
            GradRect = gradRectTemp;

            if (ActiveCurveForm != null) {
                ActiveCurveForm.gradRect = GradRect;
                UpdateActiveCurveKeys();
            }
        }

        public void UpdateGrid(Rect newGridRect) {
            if (gridRect.width == 0) {
                EntireRect = newGridRect;
            } else {
                //proportions should stay the same, as the grid is moved or resized (not zoomed)
                float ratioX = newGridRect.width / gridRect.width;
                float ratioY = newGridRect.height / gridRect.height;
                EntireRect = new Rect(ratioX * (EntireRect.xMin - gridRect.xMin) + newGridRect.xMin,
                                        ratioY * (EntireRect.yMin - gridRect.yMin) + newGridRect.yMin,
                                        ratioX * EntireRect.width, ratioY * EntireRect.height);
            }
            gridRect = newGridRect;
            adjGridRect = newGridRect;
            adjGridRect.xMin -= 0.5f;
            adjGridRect.xMax += 0.5f;
            adjGridRect.yMin -= 0.5f;
            adjGridRect.yMax += 0.5f;

            multipleKeySelection.UpdateSelectedKnots(gridRect);
        }

        void DrawGridAndLines() {
            if (gradRect.height > Mathf.Epsilon) {  
                UpdateGridLinesMesh();
                Graphics.DrawMesh(meshGridLines, Vector3.zero, Quaternion.identity, lineMaterial, 5);
                DrawCurves();                
                DrawBasicShapes();
            }
        }
        
        void UpdateGridLinesMesh()
        {
            bool recalculate = false;
            bool gradUpdate = false;
            bool gridVerticalUpdate = false;

            if (prevGradRect != gradRect)
            {
                prevGradRect = gradRect;
                recalculate = true;
                gradUpdate = true;
            }

            if (prevEntireRectYMin != EntireRect.yMin)
            {
                prevEntireRectYMin = EntireRect.yMin;
                recalculate = true;
                gridVerticalUpdate = true;
            }

            if (prevEntireRectHeight != EntireRect.height)
            {
                prevEntireRectHeight = EntireRect.height;
                if (!gridVerticalUpdate)
                {
                    recalculate = true;
                    gridVerticalUpdate = true;
                }
            }

            if (recalculate)
            {
                CalculateHorizontalLines(gradUpdate, gridVerticalUpdate);
                mMirroredHor = gradRect.yMin < 0;
                mSegmentsHor = mMirroredHor ? 2 : 1;
                mSampleHor = (1f - mRezid) * EntireRect.height / (mSegmentsHor * mHorLines);
                mStartHor = mMirroredHor ? (EntireRect.yMin + EntireRect.yMax) * 0.5f : EntireRect.yMin;
            }

            //vertical lines (it was tested only with default values for xMin and xMax)
            bool gridHorizontallUpdate = false;
            if (prevEntireRectWidth != EntireRect.width)
            {
                prevEntireRectWidth = EntireRect.width;
                gridHorizontallUpdate = true;
            }

            if (prevEntireRectXMin != EntireRect.xMin)
            {
                prevEntireRectXMin = EntireRect.xMin;
                gridHorizontallUpdate = true;
            }

            if (gridHorizontallUpdate)
            {
                CalculateVerticalLines();
            }

            if(recalculate || gridHorizontallUpdate)
            {                 
                List<Vector3> vertices = new List<Vector3>();
                List<Color> colors = new List<Color>();
                
                //horyzontal lines(it was tested for gradations that ranges from ymin = 0(or '-ymax') to ymax = 'posivitve value' )	      
                for (int i = 0; i <= mHorLines; i++)
                {
                    if (i % (mMidHor ? 2 : 5) == 0 || LastLine(i) || i == 0)
                    {
                        lineColor.a = 1.0f;
                    } else {
                        lineColor.a = mAlpha;
                    }
                    
                    if (AddLineVertices(i, mStartHor, mSampleHor, EntireRect.yMax, vertices))
                    {
                        AddColorTwice(colors, lineColor);
                    }
                    if ((i != 0) && mMirroredHor)
                    {
                        if(AddLineVertices(i, mStartHor, -mSampleHor, EntireRect.yMin, vertices))
                        {
                            AddColorTwice(colors, lineColor);
                        }
                    }

                    if (LastLineRezid(i))
                    {
                        lineColor.a = 1.0f;
                        if(AddRezidualLineVertices(EntireRect.yMax, vertices))
                        {
                            AddColorTwice(colors, lineColor);
                        }
                        if (mMirroredHor)
                        {
                            if(AddRezidualLineVertices(EntireRect.yMin, vertices))
                            {
                                AddColorTwice(colors, lineColor);
                            }
                        }
                    }
                }

                //positions of vertical lines are calculate each drawing cycle, as an improvement, these positions should be calculated(re - calculated)
                //only when the size of the rectangle gets modified
                for (int i = 0; i <= vertLines; i++)
                {
                    float gradation = EntireRect.xMin + i * vertLinesGap;
                    if ((adjGridRect.xMin <= gradation) && (gradation <= adjGridRect.xMax))
                    {
                        if (i % vertLineStones == 0)
                        {
                            lineColor.a = 1.0f;
                        }
                        else
                        {
                            lineColor.a = vertLinesAlpha;
                        }

                        AddColorTwice(colors, lineColor);
                        vertices.Add(currentCamera.ScreenToWorldPoint(new Vector3(gradation, gridRect.yMin, Z_LINE_POS)));
                        vertices.Add(currentCamera.ScreenToWorldPoint(new Vector3(gradation, gridRect.yMax, Z_LINE_POS)));
                    }
                }
                SetupMeshLines(meshGridLines, vertices, colors);                           
                Curves.TriggerUpdateCurves();
                Curves.TriggerUpdateBasicCurves();
            }
        }
        
        void AddColorTwice(List<Color> colors, Color color)
        {
            colors.AddRange(new Color[] { color, color });
        }

        bool LastLine(int i) {
            return (i == mHorLines) && (mRezid == 0);
        }

        bool LastLineRezid(int i) {
            return (i == mHorLines) && (mRezid > 0);
        }

        bool AddLineVertices(int i, float start, float sample, float limit, List<Vector3> vertices) {
            bool add = false;
            float gradation = LastLine(i) ? limit : (start + i * sample);
            if ((adjGridRect.yMin <= gradation) && (gradation <= adjGridRect.yMax)) {
                vertices.Add(currentCamera.ScreenToWorldPoint(new Vector3(gridRect.xMin, gradation, Z_LINE_POS)));
                vertices.Add(currentCamera.ScreenToWorldPoint(new Vector3(gridRect.xMax, gradation, Z_LINE_POS)));
                add = true;
            }
            return add;
        }

        bool AddRezidualLineVertices(float limit, List<Vector3> vertices) {
            bool add = false;
            if ((adjGridRect.yMin <= limit) && (limit <= adjGridRect.yMax)) {
                vertices.Add(currentCamera.ScreenToWorldPoint(new Vector3(gridRect.xMin, limit, Z_LINE_POS)));
                vertices.Add(currentCamera.ScreenToWorldPoint(new Vector3(gridRect.xMax, limit, Z_LINE_POS)));
                add = true;
            }
            return add;
        }

        void SetupMeshLines(Mesh mesh, List<Vector3> vertices, List<Color> colors)
        {
            mesh.Clear();
            mesh.SetVertices(vertices);
            int[] indices = new int[vertices.Count];
            for (int i = 0; i < vertices.Count; ++i)
            {
                indices[i] = i;
            }
            mesh.SetIndices(indices, MeshTopology.Lines, 0);
            mesh.SetColors(colors);
        }

        void CalculateHorizontalLines(bool gradUpdate, bool gridHeightUpdate) {
            //calculate how many horyzontal lines should be drawn (based of the grid size, and the gradations ranges)        
            float ratio = EntireRect.height / HeightUnit;
            int segments = 1;
            float prevRezid = 0;
            int prevHorlines = 0;
            bool mirrored;
            // 直接取消区别对待
            // if ((gradRect.yMin < 0) && (0 < gradRect.yMax))
            // {
            //     segments = 2;
            //     GetHorLinesCountAndRezid(gradRect.yMax, ratio / segments, out mRezid, out prevRezid, out mHorLines, out prevHorlines);
            //     mirrored = true;
            // }
            // else
            {
                GetHorLinesCountAndRezid(gradRect.yMax - gradRect.yMin, ratio, out mRezid, out prevRezid, out mHorLines, out prevHorlines);
                mirrored = false;
            }

            mAlpha = (ratio / (mHorLines * segments) - 0.2f) * 1.25f;//the intermediate lines are more transparent             

            int rowCount = mHorLines;
            float rezid = mRezid;
            if (mAlpha < 0.35f) {
                rowCount = prevHorlines;
                rezid = prevRezid;
            }

            if (gridHeightUpdate || gradUpdate) {
                curveWindow.UpdateVerGradations(rowCount, rezid, mirrored, gradUpdate);
            }
        }

        void CalculateVerticalLines() {
            float ratio = EntireRect.width / (WidthUnit * 2f);
            vertLines = 1;
            bool middVer = true;
            while (ratio >= vertLines) {
                vertLines *= middVer ? 5 : 2;
                middVer = !middVer;
            }
            vertLineStones = middVer ? 2 : 5;
            int colCount = vertLines;
            vertLinesAlpha = (ratio / vertLines - 0.2f) * 1.25f;
            if (vertLinesAlpha < 0.5f) {
                colCount /= vertLineStones;
            }
            vertLinesGap = EntireRect.width / vertLines;
            curveWindow.UpdateHorGradations(colCount);
        }

        public void Zoom(float factor, Vector2 mousePos) {
            float invFactor = 1f / factor;
            float x = mousePos.x - (mousePos.x - EntireRect.x) * invFactor;
            float width = EntireRect.width * invFactor;
            float y = mousePos.y - (mousePos.y - EntireRect.y) * invFactor;
            float height = EntireRect.height * invFactor;
            UpdateEntireRect(x, y, width, height);
        }

        public void Pan(Vector2 diff) {
            float width = EntireRect.width;
            float height = EntireRect.height;
            float x = EntireRect.x + diff.x;
            float y = EntireRect.y + diff.y;
            UpdateEntireRect(x, y, width, height);
        }

        void UpdateEntireRect(float x, float y, float width, float height) {
            if (gridRect.xMin < x) {
                x = gridRect.xMin;
            } else if (x + width < gridRect.xMax) {
                x = gridRect.xMax - width;
            }
            if (gridRect.yMin < y) {
                y = gridRect.yMin;
            } else if (y + height < gridRect.yMax) {
                y = gridRect.yMax - height;
            }
            EntireRect = new Rect(x, y, width, height);

            multipleKeySelection.UpdateSelectedKnots(gridRect);
        }

        void ResetBasicClips()
        {

            for (int i = 0; i < SHAPE_COUNT; ++i)
            {
                basicShapeClips[i] = 0;
            }
        }

        void DrawBasicShapes() {
            if (gradRect.height > 0f) {
                if (Curves.BasicCurvesUpdate(basicShapes[0])) {
                    float alignMiddle = 0f;
                    if (gridRect.width > SHAPE_COUNT * (basicShapeWidthPixels + basicShapesSpacePixels)) {
                        alignMiddle = gridRect.width * 0.5f - (SHAPE_COUNT * 0.5f * (basicShapeWidthPixels + basicShapesSpacePixels));
                    }
                    ResetBasicClips();
                    List<Vector3> verticesQuads = new List<Vector3>();
                    for (int i = 0; i < SHAPE_COUNT; ++i) {
                        float shapeMin = (basicShapeWidthPixels + basicShapesSpacePixels) * i;
                        if (gridRect.xMin + shapeMin > gridRect.xMax) {
                            break;
                        }

                        float shapeMax = basicShapeWidthPixels + shapeMin;

                        if (gridRect.xMin + shapeMax > gridRect.xMax) {
                            shapeMax = gridRect.xMax - gridRect.xMin;
                        }
                       
                        Rect shapeRect = Rect.MinMaxRect(gridRect.xMin + shapeMin + alignMiddle, BottomShapesYmin, gridRect.xMin + shapeMax + alignMiddle, BottomShapesYmax);
                        
                        verticesQuads.Add(currentCamera.ScreenToWorldPoint(new Vector3(shapeRect.xMin, shapeRect.yMin, 0)));
                        verticesQuads.Add(currentCamera.ScreenToWorldPoint(new Vector3(shapeRect.xMin, shapeRect.yMax, 0)));
                        verticesQuads.Add(currentCamera.ScreenToWorldPoint(new Vector3(shapeRect.xMax, shapeRect.yMax, 0)));
                        verticesQuads.Add(currentCamera.ScreenToWorldPoint(new Vector3(shapeRect.xMax, shapeRect.yMin, 0)));

                        basicShapeClips[i] = shapeRect.width;
                        shapeRect.xMax = gridRect.xMin + basicShapeWidthPixels + shapeMin + alignMiddle;
                        if (basicShapeClips[i] < shapeRect.width) {
                            basicShapeClips[i] = basicShapeClips[i] / shapeRect.width;
                        } else {
                            basicShapeClips[i] = 1f;
                        }
                        basicShapesRect[i] = shapeRect;
                    }

                    meshBasicCurveQuads.Clear();
                    meshBasicCurveQuads.SetVertices(verticesQuads);
                    int[] indices = new int[verticesQuads.Count];
                    Color[] colors = new Color[verticesQuads.Count];
                    for (int i = 0; i < verticesQuads.Count; ++i)
                    {
                        indices[i] = i;
                        colors[i] = HALF_GRAY;
                    }
                    meshBasicCurveQuads.SetColors(colors);
                    meshBasicCurveQuads.SetIndices(indices, MeshTopology.Quads, 0);
                }

                Graphics.DrawMesh(meshBasicCurveQuads, Vector3.zero, Quaternion.identity, lineMaterial, 5);

                for (int i = 0; i < SHAPE_COUNT; ++i)
                {
                    float clip = basicShapeClips[i];
                    if (clip == 0f)
                    {
                        break;
                    }
                    Curves.DrawCurveForm(LIGHT_GRAY, basicShapes[i], null, false, false, -1, basicShapesRect[i], basicShapesRect[i], normalRect, true, clip);
                }
            }
        }

        void UpdateActiveCurveForm(CurveForm curveForm) {
            if (ActiveCurveForm.curve1 == curveForm.curve1) {
                ActiveCurveForm.firstCurveSelected = curveForm.firstCurveSelected;
            } else {
                ActiveCurveForm = curveForm;
                GradRect = ActiveCurveForm.gradRect;
            }
            AlterData();
        }

        void CheckChangeCurveSelection(CurveForm curveForm, int keyIndex) {
            if ((ActiveCurveForm != curveForm) || (ActiveCurveForm.firstCurveSelected != curveForm.firstCurveSelected) || (selectedKeyIndex != keyIndex)) {
                undoRedo.AddOperation(new CurveSelectionOperation(this, curveFormList.IndexOf(ActiveCurveForm), ActiveCurveForm.firstCurveSelected, selectedKeyIndex));
            }
        }

        public void SelectCurveForm(int curveFormIndex) {
            UpdateActiveCurveForm(curveFormList[curveFormIndex]);
        }

        public int GetCurveFormIndex() {
            return curveFormList.IndexOf(ActiveCurveForm);
        }

        public bool GetFirstCurveSelected() {
            return (ActiveCurveForm != null) && ActiveCurveForm.firstCurveSelected;
        }

        public void SelectFirstCurve(bool firstCurve) {
            ActiveCurveForm.firstCurveSelected = firstCurve;
        }

        /// <summary>
        /// Checks if the user wanna drag a tangent of the selected key
        /// </param>
        void CheckMouseTangentSelection(Vector2 mousePos) {
            if ((selectedKeyIndex >= 0) && Input.GetMouseButtonDown(0)) {
                AnimationCurve curve = ActiveCurveForm.firstCurveSelected ? ActiveCurveForm.curve1 : ActiveCurveForm.curve2;
                float ratio = EntireRect.height * gradRect.width / (EntireRect.width * gradRect.height);
                Vector2 keyScreenPos = Utils.Convert(new Vector2(curve[selectedKeyIndex].time, curve[selectedKeyIndex].value), EntireRect, ActiveCurveForm.gradRect);

                if (adjGridRect.Contains(keyScreenPos)) {//check first the keyframe(and tangents) are visible
                    Keyframe keyframe = curve[selectedKeyIndex];
                    if (curve.length - selectedKeyIndex > 1) {
                        float tangOut = keyframe.outTangent;
                        float tangOutScaled = Mathf.Atan(tangOut * ratio);
                        Vector2 tangPeak = new Vector2(keyScreenPos.x + tangFloat * Mathf.Cos(tangOutScaled), keyScreenPos.y + tangFloat * Mathf.Sin(tangOutScaled));
                        if (Vector2.SqrMagnitude(tangPeak - mousePos) <= sqrMarginPixels) {
                            isTangentSelected = true;
                            leftTangetSelected = false;
                        }
                    }
                    if (!isTangentSelected && (selectedKeyIndex > 0)) {
                        float tangIn = keyframe.inTangent;
                        float tangInScaled = Mathf.Atan(tangIn * ratio);
                        Vector2 tangPeak = new Vector2(keyScreenPos.x - tangFloat * Mathf.Cos(tangInScaled), keyScreenPos.y - tangFloat * Mathf.Sin(tangInScaled));
                        if (Vector2.SqrMagnitude(tangPeak - mousePos) <= sqrMarginPixels) {
                            isTangentSelected = true;
                            leftTangetSelected = true;
                        }
                    }
                    if (isTangentSelected) {
                        undoRedo.AddOperation(new TangentModeOperation(this, GetContextMenuForCurrentKey(), keyframe.inTangent, keyframe.outTangent));
                    }
                }
            }
        }

        /// <summary>
        /// Checks what the user clicks(selects), a tangent, a key or a whole curve. 
        /// </summary>
        bool CheckMouseSelection(Vector2 mousePos, CurveForm curveForm) {
            int i;
            List<AnimationCurve> curves = new List<AnimationCurve>();
            curves.Add(curveForm.curve1);
            if (curveForm.curve2 != null) {
                curves.Add(curveForm.curve2);
            }

            foreach (AnimationCurve curve in curves) {
                for (i = 0; i < curve.length; ++i) {
                    Keyframe keyframe = curve[i];
                    Vector2 keyframePos = new Vector2(keyframe.time, keyframe.value);
                    Vector2 keyScreenPos = Utils.Convert(keyframePos, EntireRect, curveForm.gradRect);
                    if (adjGridRect.Contains(keyScreenPos)) {//check first the keyframe is visible
                        if (Vector2.SqrMagnitude(keyScreenPos - mousePos) <= sqrMarginPixels) {
                            CheckChangeCurveSelection(curveForm, i);
                            selectedKeyIndex = i;
                            curveForm.firstCurveSelected = curveForm.curve1 == curve;

                            Curves.TriggerUpdateCurve(curveForm.UnselectedCurve());
                            UpdateActiveCurveForm(curveForm);
                            if (multipleKeySelection.MultipleKeysAreSelected()) {
                                multipleKeySelection.ClearMultipleKeySelection();
                            }
                            if (curveWindow.IsDoubleTap() || Input.GetMouseButtonDown(1)) {
                                ShowContextMenuKey(mousePos);
                            } else if (curveWindow.IsSingleTap() || Input.GetMouseButtonDown(0)) {
                                keyDragged = true;
                                keyValue.SetLabelEnabled(true, this);
                                selectedKeyStartingPos = keyframePos;
                            }
                            break;
                        }
                    }
                }
                if (i == curve.length) {
                    if (Utils.PointLineSqrDist(new Vector2(mousePos.x, mousePos.y), curve, EntireRect, curveForm.gradRect, contextMenuManager) <= sqrMarginPixels) {
                        if (curveWindow.IsDoubleTap() || Input.GetMouseButton(1)) {
                            ShowContextMenuAddKey(mousePos);
                        } else if (Input.GetMouseButtonDown(0)) {
                            lineDragged = true;
                            showCursorNormal = false;
                            Cursor.SetCursor(TextureNS, CurveWindow.hotspot, CursorMode.ForceSoftware);
                            lineDragKeysStartingValue = new List<float>(curve.length);
                            foreach (Keyframe keyframe in curve.keys) {
                                lineDragKeysStartingValue.Add(keyframe.value);
                            }
                        }
                        CheckChangeCurveSelection(curveForm, KeyHandling.UNSELECTED);
                        curveForm.firstCurveSelected = curveForm.curve1 == curve;

                        Curves.TriggerUpdateCurve(curveForm.UnselectedCurve());
                        UpdateActiveCurveForm(curveForm);
                        if (multipleKeySelection.MultipleKeysAreSelected()) {
                            multipleKeySelection.ClearMultipleKeySelection();
                        }
                        selectedKeyIndex = KeyHandling.UNSELECTED;
                        return true;
                    }
                } else {
                    return true;
                }
            }
            return false;
        }

        void AddKeyByScreenPos(Vector3 screenPos) {
            Vector2 pos = Utils.Convert(new Vector2(screenPos.x, screenPos.y), gradRect, EntireRect);
            AddKey(pos);
            undoRedo.AddOperation(new AddOperation(this, pos));
        }

        public void AddKey(Vector2 pos, ContextMenu definedContextMenu = null) {
            AnimationCurve activeCurve = ActiveCurveForm.SelectedCurve();
            List<ContextMenu> listContextMenus = contextMenuManager.dictCurvesContextMenus[activeCurve];
            ContextMenu contextMenu = definedContextMenu;
            if (definedContextMenu == null) {
                contextMenu = new ContextMenu();
                switch (lastSelectedOption) {
                    case ContextOptions.freesmooth:
                        contextMenu.freeSmooth = true;
                        break;
                    case ContextOptions.clamped:
                        contextMenu.clampedAuto = true;
                        break;
                    case ContextOptions.auto:
                        contextMenu.auto = true;
                        break;
                    case ContextOptions.broken:
                        contextMenu.broken = true;
                        contextMenu.leftTangent.free = true;
                        contextMenu.rightTangent.free = true;
                        contextMenu.bothTangents.free = true;
                        break;
                }
            }

            if ((pos.x < activeCurve[0].time) || (pos.x > activeCurve[activeCurve.length - 1].time)) {
                Keyframe keyframeNeighbour = (pos.x < activeCurve[0].time) ? activeCurve[0] : activeCurve[activeCurve.length - 1];
                selectedKeyIndex = activeCurve.AddKey(pos.x, pos.y);
                if (activeCurve.length > 2) {
                    //hack needed( wierd:when a key is added in the clamped area, the neighbour's key changes its tangents...)
                    activeCurve.MoveKey((pos.x < activeCurve[1].time) ? 1 : activeCurve.length - 2, keyframeNeighbour);
                }
                Keyframe keyframe = activeCurve[selectedKeyIndex];
                keyframe.inTangent = 0;
                keyframe.outTangent = 0;

                if (contextMenu.freeSmooth) {
                    contextMenu.flat = true;
                }
                listContextMenus.Insert(selectedKeyIndex, contextMenu);
            } else {
                for (int i = 0; i < activeCurve.length - 1; ++i) {
                    if ((pos.x > activeCurve[i].time) && (pos.x < activeCurve[i + 1].time)) {
                        Keyframe keyframe = new Keyframe(pos.x, pos.y);
                        if (listContextMenus[i].rightTangent.constant || listContextMenus[i + 1].leftTangent.constant ||
                           (activeCurve[i].outTangent == float.PositiveInfinity) || (activeCurve[i + 1].inTangent == float.PositiveInfinity)) {
                            keyframe.inTangent = 0;
                            keyframe.outTangent = 0;
                            contextMenu.freeSmooth = true;
                        } else {
                            Vector2 val = new Vector2(activeCurve[i].time, activeCurve[i].value);
                            val = Utils.Convert(val, EntireRect, gradRect);
                            Vector2 val2 = new Vector2(activeCurve[i + 1].time, activeCurve[i + 1].value);
                            val2 = Utils.Convert(val2, EntireRect, gradRect);
                            float tangOut = activeCurve[i].outTangent;
                            float tangIn = activeCurve[i + 1].inTangent;
                            float ratio = EntireRect.height * gradRect.width / (EntireRect.width * gradRect.height);

                            Vector2 c1 = Vector2.zero;
                            Vector2 c2 = Vector2.zero;
                            Curves.GetControlPoints(val, val2, tangOut * ratio, tangIn * ratio, out c1, out c2);

                            float t = Utils.closestPointTValue;
                            //de Casteljau's algorithm for dividing a bezier curve
                            Vector2 p00 = (1 - t) * val + t * c1;
                            Vector2 p11 = (1 - t) * c1 + t * c2;
                            Vector2 p22 = (1 - t) * c2 + t * val2;
                            Vector2 newC2 = (1 - t) * p00 + t * p11;
                            Vector2 newC1 = (1 - t) * p11 + t * p22;

                            //got the control points ,now find the tangents for the new point
                            Curves.GetTangents(val, Utils.closestPoint, c1, newC2, out tangOut, out tangIn);
                            tangIn /= ratio;
                            keyframe.inTangent = -tangIn;
                            Curves.GetTangents(Utils.closestPoint, val2, newC1, c2, out tangOut, out tangIn);
                            tangOut /= ratio;

                            keyframe.outTangent = tangOut;
                        }

                        selectedKeyIndex = activeCurve.AddKey(keyframe);

                        if (contextMenu.freeSmooth && ContextMenuManager.IsKeyframeFlat(keyframe)) {
                            contextMenu.flat = true;
                        }
                        listContextMenus.Insert(selectedKeyIndex, contextMenu);
                        break;
                    }
                }
            }

            CheckUpdateAutoTangents(contextMenu, activeCurve, selectedKeyIndex);

            //update neighbours if they are auto	
            if (selectedKeyIndex > 0) {
                CheckUpdateAutoTangents(listContextMenus[selectedKeyIndex - 1], activeCurve, selectedKeyIndex - 1);
                //update the neighbour if it is linear in this direction
                if (listContextMenus[selectedKeyIndex - 1].leftTangent.linear) {
                    UpdateLinearTangent(activeCurve, selectedKeyIndex - 1, false);
                }
            }
            if (selectedKeyIndex < activeCurve.keys.Length - 1) {
                CheckUpdateAutoTangents(listContextMenus[selectedKeyIndex + 1], activeCurve, selectedKeyIndex + 1);
                //update the neighbour if it is linear on this direction
                if (listContextMenus[selectedKeyIndex + 1].rightTangent.linear) {
                    UpdateLinearTangent(activeCurve, selectedKeyIndex + 1, true);
                }
            }

            if (ActiveCurveForm.firstCurveSelected) {
                ActiveCurveForm.curve1KeysCount += 1;
            } else {
                ActiveCurveForm.curve2KeysCount += 1;
            }
            AlterCurveData(activeCurve);
        }

        public void CheckUpdateAutoTangents(ContextMenu contextMenu, AnimationCurve animationCurve, int keyIndex) {
            if (contextMenu.auto) {
                UpdateLegacyAutoTangents(animationCurve, keyIndex);
            } else if (contextMenu.clampedAuto) {
                UpdateClampedAutoTangents(animationCurve, keyIndex);
            }
        }

        void ShowContextMenuKey(Vector2 mousePos) {
            contextMenuUI.EnablePanel();
            contextMenuUI.SetPos(mousePos);
            contextMenuUI.SetListener(this);
            ContextMenu contextMenu = contextMenuManager.dictCurvesContextMenus[ActiveCurveForm.SelectedCurve()][selectedKeyIndex];
            contextMenuUI.SetSelectedOption(contextMenu);
        }

        void ShowContextMenuAddKey(Vector2 mousePos) {
            contextMenuUI.EnableAddPanel(true);
            contextMenuUI.SetPos(mousePos);
            contextMenuUI.SetListener(this);
            addKeyPos = mousePos;
        }

        public void DeleteKey() {
            if (ActiveCurveForm.SelectedCurve().length > 1) {
                undoRedo.AddOperation(new DeleteOperation(this));
            }
            DeleteKeySimple();
        }

        public void DeleteKeySimple() {
            KeyHandling.DeleteKey();
            selectedKeyIndex = KeyHandling.UNSELECTED;
            AlterCurveData(ActiveCurveForm.SelectedCurve());
        }

        public void AddKey() {
            AddKeyByScreenPos(addKeyPos);
        }

        public void AddKey(Keyframe keyframe, ContextMenu contextMenu) {
            AnimationCurve activeCurve = ActiveCurveForm.SelectedCurve();
            AddKey(new Vector2(keyframe.time, keyframe.value), contextMenu);
            activeCurve.MoveKey(selectedKeyIndex, keyframe);//activate tangents
        }

        public void EditKey() {
            keyValue.SetKeyEditEnabled(true, this);
            Keyframe keyframe = ActiveCurveForm.SelectedCurve()[selectedKeyIndex];
            Vector2 keyframePos = Utils.Convert(new Vector2(keyframe.time, keyframe.value), EntireRect, gradRect);
            keyValue.SetPanelEditPos(keyframePos);
            keyValue.SetTimeValueEditFields(keyframe.time, keyframe.value);
        }

        public ContextMenu GetContextMenuForCurrentKey() {
            AnimationCurve activeCurve = ActiveCurveForm.SelectedCurve();
            return contextMenuManager.dictCurvesContextMenus[activeCurve][selectedKeyIndex];
        }

        void TangentModeChange() {
            Keyframe keyframe = ActiveCurveForm.SelectedCurve()[selectedKeyIndex];
            undoRedo.AddOperation(new TangentModeOperation(this, GetContextMenuForCurrentKey(), keyframe.inTangent, keyframe.outTangent));
        }

        public void ClampedAutoKey(bool addToUndoStack = false) {
            if (addToUndoStack) {
                TangentModeChange();
            }
            AnimationCurve activeCurve = ActiveCurveForm.SelectedCurve();
            ContextMenu contextMenu = contextMenuManager.dictCurvesContextMenus[activeCurve][selectedKeyIndex];
            if (!contextMenu.clampedAuto) {
                contextMenu.Reset();
                contextMenu.clampedAuto = true;
                if (activeCurve.keys.Length > 0) {
                    UpdateClampedAutoTangents(activeCurve, selectedKeyIndex);
                }
                lastSelectedOption = ContextOptions.clamped;
            }
            AlterCurveData(activeCurve);
        }

        public void AutoKey(bool addToUndoStack = false) {
            if (addToUndoStack) {
                TangentModeChange();
            }
            AnimationCurve activeCurve = ActiveCurveForm.SelectedCurve();
            ContextMenu contextMenu = contextMenuManager.dictCurvesContextMenus[activeCurve][selectedKeyIndex];
            if (!contextMenu.auto) {
                contextMenu.Reset();
                contextMenu.auto = true;
                if (activeCurve.keys.Length > 0) {
                    UpdateLegacyAutoTangents(activeCurve, selectedKeyIndex);
                }
                lastSelectedOption = ContextOptions.auto;
            }
            AlterCurveData(activeCurve);
        }

        public void FreeSmoothKey(bool addToUndoStack = false) {
            if (addToUndoStack) {
                TangentModeChange();
            }
            AnimationCurve activeCurve = ActiveCurveForm.SelectedCurve();
            ContextMenu contextMenu = contextMenuManager.dictCurvesContextMenus[activeCurve][selectedKeyIndex];
            if (!contextMenu.freeSmooth) {
                contextMenu.Reset();
                contextMenu.freeSmooth = true;
                Keyframe keyframe = activeCurve.keys[selectedKeyIndex];
                float outTangRad = Mathf.Atan(keyframe.outTangent);
                float inTangRad = Mathf.Atan(keyframe.inTangent);
                float diff = Mathf.Abs(outTangRad - inTangRad) * 0.5f * ((outTangRad > inTangRad) ? 1 : -1);
                outTangRad -= diff;
                inTangRad += diff;
                keyframe.inTangent = Mathf.Tan(inTangRad);
                keyframe.outTangent = Mathf.Tan(outTangRad);
                activeCurve.MoveKey(selectedKeyIndex, keyframe);
                lastSelectedOption = ContextOptions.freesmooth;
            }
            AlterCurveData(activeCurve);
        }

        public void FlatKey(bool addToUndoStack = false) {
            if (addToUndoStack) {
                TangentModeChange();
            }
            AnimationCurve activeCurve = ActiveCurveForm.SelectedCurve();
            ContextMenu contextMenu = contextMenuManager.dictCurvesContextMenus[activeCurve][selectedKeyIndex];
            if (!contextMenu.flat) {
                contextMenu.Reset();
                contextMenu.freeSmooth = true;
                contextMenu.flat = true;
                Keyframe keyframe = activeCurve.keys[selectedKeyIndex];
                keyframe.inTangent = 0;
                keyframe.outTangent = 0;
                activeCurve.MoveKey(selectedKeyIndex, keyframe);
                lastSelectedOption = ContextOptions.freesmooth;
            }
            AlterCurveData(activeCurve);
        }

        public void BrokenKey(bool addToUndoStack = false) {
            if (addToUndoStack) {
                TangentModeChange();
            }
            AnimationCurve activeCurve = ActiveCurveForm.SelectedCurve();
            ContextMenu contextMenu = contextMenuManager.dictCurvesContextMenus[activeCurve][selectedKeyIndex];
            if (!contextMenu.broken) {
                contextMenu.Reset();
                contextMenu.broken = true;
                lastSelectedOption = ContextOptions.broken;
                contextMenu.leftTangent.free = true;
                contextMenu.rightTangent.free = true;
                contextMenu.bothTangents.free = true;
            }
            AlterCurveData(activeCurve);
        }

        public void Free(TangentPart tangentPart, bool addToUndoStack = false) {
            if (addToUndoStack) {
                TangentModeChange();
            }
            ContextMenu contextMenu = InitTangentPart(tangentPart);
            if (tangentPart == TangentPart.Left) {
                contextMenu.leftTangent.free = true;
                contextMenu.bothTangents.free = contextMenu.rightTangent.free;
            } else if (tangentPart == TangentPart.Right) {
                contextMenu.rightTangent.free = true;
                contextMenu.bothTangents.free = contextMenu.leftTangent.free;
            } else if (tangentPart == TangentPart.Both) {
                contextMenu.bothTangents.free = true;
                contextMenu.leftTangent.free = true;
                contextMenu.rightTangent.free = true;
            }
        }

        public void Linear(TangentPart tangentPart, bool addToUndoStack = false) {
            if (addToUndoStack) {
                TangentModeChange();
            }
            ContextMenu contextMenu = InitTangentPart(tangentPart);
            if (tangentPart == TangentPart.Left) {
                contextMenu.leftTangent.linear = true;
                contextMenu.bothTangents.linear = contextMenu.rightTangent.linear;
            } else if (tangentPart == TangentPart.Right) {
                contextMenu.rightTangent.linear = true;
                contextMenu.bothTangents.linear = contextMenu.leftTangent.linear;
            } else if (tangentPart == TangentPart.Both) {
                contextMenu.bothTangents.linear = true;
                contextMenu.leftTangent.linear = true;
                contextMenu.rightTangent.linear = true;
            }
            AnimationCurve activeCurve = ActiveCurveForm.SelectedCurve();
            Keyframe keyframe = activeCurve.keys[selectedKeyIndex];
            if (contextMenu.leftTangent.linear && selectedKeyIndex > 0) {
                Keyframe keyframePrev = activeCurve.keys[selectedKeyIndex - 1];
                keyframe.inTangent = (keyframePrev.value - keyframe.value) / (keyframePrev.time - keyframe.time);
            }
            if (contextMenu.rightTangent.linear && (selectedKeyIndex < activeCurve.keys.Length - 1)) {
                Keyframe keyframeNext = activeCurve.keys[selectedKeyIndex + 1];
                keyframe.outTangent = (keyframeNext.value - keyframe.value) / (keyframeNext.time - keyframe.time);
            }
            activeCurve.MoveKey(selectedKeyIndex, keyframe);
            AlterCurveData(activeCurve);
        }

        public void Constant(TangentPart tangentPart, bool addToUndoStack = false) {
            if (addToUndoStack) {
                TangentModeChange();
            }
            ContextMenu contextMenu = InitTangentPart(tangentPart);
            if (tangentPart == TangentPart.Left) {
                contextMenu.leftTangent.constant = true;
                contextMenu.bothTangents.constant = contextMenu.rightTangent.constant;
            } else if (tangentPart == TangentPart.Right) {
                contextMenu.rightTangent.constant = true;
                contextMenu.bothTangents.constant = contextMenu.leftTangent.constant;
            } else if (tangentPart == TangentPart.Both) {
                contextMenu.bothTangents.constant = true;
                contextMenu.leftTangent.constant = true;
                contextMenu.rightTangent.constant = true;
            }
            AnimationCurve activeCurve = ActiveCurveForm.SelectedCurve();
            Keyframe keyframe = activeCurve.keys[selectedKeyIndex];
            if (contextMenu.leftTangent.constant && (selectedKeyIndex > 0)) {
                keyframe.inTangent = float.PositiveInfinity;
            }
            if (contextMenu.rightTangent.constant && (selectedKeyIndex < activeCurve.keys.Length - 1)) {
                keyframe.outTangent = float.PositiveInfinity;
            }
            activeCurve.MoveKey(selectedKeyIndex, keyframe);
            AlterCurveData(activeCurve);
        }

        public bool MousePosOverContextMenu(Vector2 mousePos) {
            return contextMenuUI.Hover(mousePos);
        }

        ContextMenu InitTangentPart(TangentPart tangentPart) {
            AnimationCurve activeCurve = ActiveCurveForm.SelectedCurve();
            ContextMenu contextMenu = contextMenuManager.dictCurvesContextMenus[activeCurve][selectedKeyIndex];
            if (!contextMenu.broken) {
                contextMenu.Reset();
                contextMenu.broken = true;
                contextMenu.bothTangents.free = true;
                contextMenu.leftTangent.free = true;
                contextMenu.rightTangent.free = true;
            }
            if (tangentPart == TangentPart.Left) {
                contextMenu.leftTangent.Reset();
            } else if (tangentPart == TangentPart.Right) {
                contextMenu.rightTangent.Reset();
            } else if (tangentPart == TangentPart.Both) {
                contextMenu.leftTangent.Reset();
                contextMenu.rightTangent.Reset();
            }
            contextMenu.bothTangents.Reset();
            return contextMenu;
        }

        public void MouseUp() {
            ResetSelections();
        }

        void ResetSelections() {
            if (lineDragged) {
                lineDragged = false;
                showCursorNormal = true;
                //Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
                Cursor.SetCursor(TextureDefault, Vector2.zero, CursorMode.ForceSoftware);
                AnimationCurve curve = ActiveCurveForm.SelectedCurve();
                List<float> keyDiffs = new List<float>();
                bool significantDiff = false;//need to know if to add operation do undo/redo stack
                int i = 0;
                float minSignificantDiff = GradRect.height * marginErr;
                foreach (Keyframe keyframe in curve.keys) {
                    float diff = keyframe.value - lineDragKeysStartingValue[i];
                    if (!significantDiff) {
                        significantDiff = diff > minSignificantDiff;
                    }
                    keyDiffs.Add(diff);
                    i += 1;
                }
                if (significantDiff) {
                    undoRedo.AddOperation(new MoveOperation(this, keyDiffs));
                }
            } else if (keyDragged) {
                keyDragged = false;
                Keyframe keyframe = ActiveCurveForm.SelectedCurve()[selectedKeyIndex];
                Vector2 keyDiff = new Vector2(keyframe.time, keyframe.value) - selectedKeyStartingPos;
                Vector2 minSignificantDiff = new Vector2(GradRect.width, GradRect.height) * marginErr;
                if ((keyDiff.x > minSignificantDiff.x) || (keyDiff.y > minSignificantDiff.y)) {
                    undoRedo.AddOperation(new MoveOperation(this, selectedKeyIndex, keyDiff));
                }
                keyValue.SetLabelEnabled(false);
            } else if (isTangentSelected) {
                isTangentSelected = false;
            } else if (multipleKeysMove) {
                multipleKeysMove = false;
                AnimationCurve curve = ActiveCurveForm.SelectedCurve();
                List<int> selectedKeyIndices = multipleKeySelection.SelectedKeyIndices();
                List<Vector2> keyDiffs = new List<Vector2>(selectedKeyIndices.Count);
                int i = 0;
                bool significantDiff = false;//need to know if to add operation do undo/redo stack
                Vector2 minSignificantDiff = new Vector2(GradRect.width, GradRect.height) * marginErr;
                foreach (int index in selectedKeyIndices) {
                    Keyframe keyframe = curve[index];
                    Vector2 keyDiff = new Vector2(keyframe.time, keyframe.value) - selectedKeysStartingPos[i];
                    if (!significantDiff) {
                        significantDiff = (keyDiff.x > minSignificantDiff.x) || (keyDiff.y > minSignificantDiff.y);
                    }
                    keyDiffs.Add(keyDiff);
                    i += 1;
                }
                if (significantDiff) {
                    undoRedo.AddOperation(new MoveOperation(this, new List<int>(selectedKeyIndices), keyDiffs));
                }
            } else if (multipleKeysResize != ResizePart.None) {
                multipleKeysResize = ResizePart.None;
                //TODO check if that significantDiff might be needed
            }
        }

        public void UpdateContextMenus(int newIndex, int index) {
            KeyHandling.UpdateContextMenus(newIndex, index);
        }

        void UpdateClampedAutoTangents(AnimationCurve curve, int selectedKey) {
            Keyframe keyframe = curve.keys[selectedKey];
            if ((selectedKey > 0) && (selectedKey < curve.keys.Length - 1)) {
                Keyframe keyframePrev = curve.keys[selectedKey - 1];
                Keyframe keyframeNext = curve.keys[selectedKey + 1];
                if (((keyframePrev.value < keyframe.value) && (keyframe.value < keyframeNext.value)) || ((keyframeNext.value < keyframe.value) && (keyframe.value < keyframePrev.value))) {
                    keyframe.inTangent = Mathf.Sqrt((keyframePrev.value - keyframe.value) * (keyframe.value - keyframeNext.value)) * 2f / (keyframeNext.time - keyframePrev.time);
                } else {
                    keyframe.inTangent = 0;
                }
                keyframe.outTangent = keyframe.inTangent;
            } else if (curve.keys.Length >= 2) {
                if (selectedKey == 0) {
                    keyframe.outTangent = 0;
                } else if (selectedKey == curve.keys.Length - 1) {
                    keyframe.inTangent = 0;
                }
            }
            curve.MoveKey(selectedKey, keyframe);

            onCurveChanged?.Invoke();
        }

        void UpdateLegacyAutoTangents(AnimationCurve curve, int selectedKey) {
            Keyframe keyframe = curve.keys[selectedKey];
            if (selectedKey > 0 && (selectedKey < curve.keys.Length - 1)) {
                Keyframe keyframePrev = curve.keys[selectedKey - 1];
                Keyframe keyframeNext = curve.keys[selectedKey + 1];
                float tangPrev = (keyframe.value - keyframePrev.value) / (keyframe.time - keyframePrev.time);
                float tangNext = (keyframe.value - keyframeNext.value) / (keyframe.time - keyframeNext.time);
                keyframe.inTangent = (tangPrev + tangNext) * 0.5f;
                keyframe.outTangent = keyframe.inTangent;
            } else if (curve.keys.Length >= 2) {
                if (selectedKey == 0) {
                    Keyframe keyframeNext = curve.keys[selectedKey + 1];
                    keyframe.outTangent = (keyframe.value - keyframeNext.value) / (keyframe.time - keyframeNext.time);
                } else if (selectedKey == curve.keys.Length - 1) {
                    Keyframe keyframePrev = curve.keys[selectedKey - 1];
                    keyframe.inTangent = (keyframePrev.value - keyframe.value) / (keyframePrev.time - keyframe.time);
                }
            }
            curve.MoveKey(selectedKey, keyframe);

            onCurveChanged?.Invoke();
        }

        public void UpdateLinearTangent(AnimationCurve activeCurve, int keyIndex, bool leftTangent = false) {
            Keyframe keyframe = activeCurve.keys[keyIndex];
            if (leftTangent) {
                Keyframe keyframePrev = activeCurve.keys[keyIndex - 1];
                keyframe.inTangent = (keyframePrev.value - keyframe.value) / (keyframePrev.time - keyframe.time);
            } else {
                Keyframe keyframeNext = activeCurve.keys[keyIndex + 1];
                keyframe.outTangent = (keyframeNext.value - keyframe.value) / (keyframeNext.time - keyframe.time);
            }
            activeCurve.MoveKey(keyIndex, keyframe);

            onCurveChanged?.Invoke();
        }

        public void UpdateAutoLinearSideEffects() {//TODO should be temporary
            KeyHandling.UpdateAutoLinearSideEffects(selectedKeyIndex);
        }

        public void MouseDrag(Vector2 diff) {
            AnimationCurve activeCurve = ActiveCurveForm.SelectedCurve();
            if (activeCurve != null) {
                List<ContextMenu> listContextMenus = contextMenuManager.dictCurvesContextMenus[activeCurve];
                if (isTangentSelected || keyDragged) {
                    Keyframe keyframe = activeCurve[selectedKeyIndex];
                    Vector2 keyframePos = Utils.Convert(new Vector2(keyframe.time, keyframe.value), EntireRect, gradRect);
                    if (isTangentSelected) {//if any tangent is selected
                        Vector2 mousePos = curveWindow.CursorPos();
                        float ratio = gradRect.height * EntireRect.width / (gradRect.width * EntireRect.height);

                        if (listContextMenus[selectedKeyIndex].auto || listContextMenus[selectedKeyIndex].clampedAuto) {
                            listContextMenus[selectedKeyIndex].Reset();
                            listContextMenus[selectedKeyIndex].freeSmooth = true;
                        }
                        if (leftTangetSelected) {
                            if (keyframePos.x - mousePos.x < marginErr) {
                                keyframe.inTangent = float.PositiveInfinity;
                            } else {
                                keyframe.inTangent = ratio * (mousePos.y - keyframePos.y) / (mousePos.x - keyframePos.x);
                            }
                            if (listContextMenus[selectedKeyIndex].freeSmooth) {
                                keyframe.outTangent = keyframe.inTangent;
                                ContextMenu contextMenu = listContextMenus[selectedKeyIndex];
                                contextMenu.flat = keyframe.inTangent == 0;
                                listContextMenus[selectedKeyIndex] = contextMenu;
                            }
                            activeCurve.MoveKey(selectedKeyIndex, keyframe);
                        } else {//TODO it duplicates the above branch
                            if (mousePos.x - keyframePos.x < marginErr) {
                                keyframe.outTangent = float.PositiveInfinity;
                            } else {
                                keyframe.outTangent = ratio * (mousePos.y - keyframePos.y) / (mousePos.x - keyframePos.x);
                            }
                            if (listContextMenus[selectedKeyIndex].freeSmooth) {
                                keyframe.inTangent = keyframe.outTangent;
                                ContextMenu contextMenu = listContextMenus[selectedKeyIndex];
                                contextMenu.flat = keyframe.outTangent == 0;
                                listContextMenus[selectedKeyIndex] = contextMenu;
                            }
                            activeCurve.MoveKey(selectedKeyIndex, keyframe);
                        }
                    } else if (keyDragged) {//if any key is selected   
                        selectedKeyIndex = KeyHandling.MoveKey(selectedKeyIndex, diff);
                        if (selectedKeyIndex == KeyHandling.UNSELECTED) {
                            keyDragged = false;
                        } else {//if key still selected
                            keyValue.SetTimeValueText(keyframe.value, keyframe.time);
                            keyValue.SetLabelPos(keyframePos);
                        }
                    }
                    AlterCurveData(activeCurve);
                } else if (lineDragged) {//if any curve is selected
                    if (showCursorNormal && EntireRect.Contains(curveWindow.CursorPos())) {
                        showCursorNormal = false;
                        Cursor.SetCursor(TextureNS, CurveWindow.hotspot, CursorMode.ForceSoftware);
                    } else if (!showCursorNormal && !EntireRect.Contains(curveWindow.CursorPos())) {
                        showCursorNormal = true;
                        //Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
                        Cursor.SetCursor(TextureDefault, Vector2.zero, CursorMode.ForceSoftware);
                    }
                    diff.x = 0;
                    for (int i = 0; i < activeCurve.length; ++i) {
                        KeyHandling.MoveKey(i, diff);
                    }
                    AlterCurveData(activeCurve);
                } else if (multipleKeysMove) {
                    List<int> selectedKeyIndices = multipleKeySelection.SelectedKeyIndices();
                    if (((activeCurve[selectedKeyIndices[0]].time > CurveForm.X_MIN) || (diff.x > 0)) && ((activeCurve[selectedKeyIndices[selectedKeyIndices.Count - 1]].time < CurveForm.X_MAXIM) || (diff.x < 0)))
                    {
                        bool revOrder = false;
                        if ((selectedKeyIndices.Count > 1) && (diff.x > Mathf.Epsilon))
                        {
                            //just do the movement of the keys, from the last to first, to avoid the change of order of the selected keys 
                            revOrder = true;
                        }

                        int length = activeCurve.length;
                        for (int i = 0; i < selectedKeyIndices.Count; ++i)
                        {
                            int ii = revOrder ? (selectedKeyIndices.Count - i - 1) : i;
                            int index = selectedKeyIndices[ii];
                            int newIndex = KeyHandling.MoveKey(index, diff);

                            if ((i == 0) && !revOrder && (activeCurve.length < length))
                            {
                                //case when the first key of a curve is deleted because it's overlapped by the moving neighbour
                                //so, shift all the other selected keys
                                for (int j = 0; j < selectedKeyIndices.Count; ++j)
                                {
                                    selectedKeyIndices[j] -= 1;
                                }

                                /// ask for this particular update
                                Curves.TriggerUpdateCurve(activeCurve);
                                DrawCurves();//TODO try to do update just the 'selected knots' list
                                selectedKeyIndex = KeyHandling.UNSELECTED;
                            }
                            KeyHandling.CheckMovingBeyond(index, newIndex, selectedKeyIndex, selectedKeyIndices, ii);
                        }
                        multipleKeySelection.UpdateSelectedKnots(gridRect);
                        AlterCurveData(activeCurve);
                    }
                } else if (multipleKeysResize != ResizePart.None) {
                    List<int> selectedKeyIndices = multipleKeySelection.SelectedKeyIndices();

                    bool revOrder = false;
                    if ((selectedKeyIndices.Count > 1) && (diff.x > Mathf.Epsilon))
                    {
                        //just do the movement of the keys, from the last to first, to avoid the change of order of the selected keys 
                        revOrder = true;
                    }

                    if ((multipleKeysResize == ResizePart.Left) || (multipleKeysResize == ResizePart.Right))
                    {
                        bool leftPivot = multipleKeysResize == ResizePart.Right;
                        int pivotIndex = selectedKeyIndices[leftPivot ? 0 : (selectedKeyIndices.Count - 1)];
                        int movingKeyIndex = selectedKeyIndices[leftPivot ? (selectedKeyIndices.Count - 1) : 0];
                        const float LIMIT = 0.1f;

                        float timeDiff = activeCurve[movingKeyIndex].time - activeCurve[pivotIndex].time;
                        bool resizeable = (pivotIndex < movingKeyIndex) ? ((timeDiff > LIMIT) || (diff.x >= 0)) : ((-timeDiff > LIMIT) || (diff.x <= 0));

                        if(resizeable)
                        {
                            int length = activeCurve.length;
                            for (int i = 0; i < selectedKeyIndices.Count; ++i)
                            {
                                int ii = revOrder ? (selectedKeyIndices.Count - i - 1) : i;
                                int index = selectedKeyIndices[ii];
                                if (index == pivotIndex)
                                {
                                    continue;//don't move the pivot
                                }
                                float ratio = (activeCurve[index].time - activeCurve[pivotIndex].time) / (activeCurve[movingKeyIndex].time - activeCurve[pivotIndex].time);
                                //Debug.LogError("ratio:" + ratio + " index:" + index);
                                int newIndex = KeyHandling.MoveKey(index, new Vector2(diff.x * ratio, 0));
                                if ((i == 0) && !revOrder && (activeCurve.length < length))
                                {
                                    //case when the first key of a curve is deleted because it's overlapped by the moving neighbour
                                    //so, shift all the other selected keys
                                    for (int j = 1; j < selectedKeyIndices.Count; ++j)
                                    {
                                        selectedKeyIndices[j] -= 1;
                                    }
                                }
                                KeyHandling.CheckMovingBeyond(index, newIndex, selectedKeyIndex, selectedKeyIndices, ii);
                            }
                        }
                    } else {
                        int pivotIndex = selectedKeyIndices[0];
                        int movingIndex = selectedKeyIndices[0];
                        for (int i = 1; i < selectedKeyIndices.Count; ++i)
                        {
                            if ((multipleKeysResize == ResizePart.Top) ^ (activeCurve[pivotIndex].value < activeCurve[selectedKeyIndices[i]].value))
                            {
                                pivotIndex = selectedKeyIndices[i];
                            }

                            if ((multipleKeysResize == ResizePart.Bottom) ^ (activeCurve[movingIndex].value < activeCurve[selectedKeyIndices[i]].value))
                            {
                                movingIndex = selectedKeyIndices[i];
                            }
                        }

                        for (int i = 0; i < selectedKeyIndices.Count; ++i)
                        {
                            int index = selectedKeyIndices[i];
                            float ratio = (activeCurve[index].value - activeCurve[pivotIndex].value) / (activeCurve[movingIndex].value - activeCurve[pivotIndex].value);
                            KeyHandling.MoveKey(index, new Vector2(0, diff.y * ratio));
                        }
                    }

                    multipleKeySelection.UpdateSelectedKnots(gridRect);
                    AlterCurveData(activeCurve);
                }
            }
        }

        public void ChangeKeyValue(float value) {
            ChangeKeyFramePosition();
        }

        public void ChangeKeyTime(float time) {
            ChangeKeyFramePosition();
        }

        void ChangeKeyFramePosition() {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) {
                AnimationCurve curve = ActiveCurveForm.SelectedCurve();
                Keyframe keyframe = curve[selectedKeyIndex];
                float time = keyframe.time;
                float value = keyframe.value;
                keyValue.ReadKeyFrameValues(out time, out value);
                Vector2 diff = new Vector2(time - keyframe.time, value - keyframe.value);
                keyframe.time = time;
                keyframe.value = value;
                selectedKeyIndex = KeyHandling.MoveKey(curve, selectedKeyIndex, keyframe);
                undoRedo.AddOperation(new MoveOperation(this, selectedKeyIndex, diff));
                keyValue.SetKeyEditEnabled(false);
                AlterCurveData(curve);
            } else {
                keyValue.SetKeyEditEnabled(false);
            }
        }

        /// <summary>
        /// Draw the curve forms. Calls static DrawCurve method of Curves class.
        /// </summary>
        void DrawCurves() {
            AnimationCurve activeCurve = ActiveCurveForm.curve1;
            if (activeCurve != null) {
                foreach (CurveForm curveForm in curveFormList) {
                    if (curveForm.curve1 == ActiveCurveForm.curve1) {
                        continue;
                    }
                    Curves.DrawCurveForm(curveForm.shadyColor, curveForm.curve1, curveForm.curve2, false, false, selectedKeyIndex, EntireRect, adjGridRect, curveForm.gradRect);
                }
                Curves.DrawCurveForm(ActiveCurveForm.color, ActiveCurveForm.curve1, ActiveCurveForm.curve2, ActiveCurveForm.firstCurveSelected, !ActiveCurveForm.firstCurveSelected, selectedKeyIndex, EntireRect, adjGridRect, gradRect);
            }
        }

        //for the given vertical interval and ratio between grid height and unit height, calculate:
        //intervalInt the number of horizontal lines to be drawn in the grid,
        //rezid which is the percentage to the grid height of the difference between the max value and the highest milestone (e.g. 1.7 to 1.5 or 1.7 to 1.0)
        //prev rezid and prev intervalInt are needed to know the density of the vertical gradations
        void GetHorLinesCountAndRezid(float interval, float ratio, out float rezid, out float prevRezid, out int intervalInt, out int prevIntervalInt) {
            // 最大值和最高里程碑之间的差值占网格高度的百分比
            rezid = 0;
            prevRezid = 0;
            // 水平线数量
            intervalInt = 0;
            prevIntervalInt = 0;
            if (interval > 0) { //interval should allways be positive
                if (interval >= 10) {
                    GetHorLinesCountAndRezid(interval / 10.0f, ratio, out rezid, out prevRezid, out intervalInt, out prevIntervalInt);
                } else if (interval < 1) {
                    GetHorLinesCountAndRezid(interval * 10.0f, ratio, out rezid, out prevRezid, out intervalInt, out prevIntervalInt);
                } else {
                    intervalInt = Mathf.FloorToInt(interval);
                    if (!Mathf.Approximately(intervalInt, interval)) {
                        rezid = (interval - intervalInt) / interval;
                    }
                    mMidHor = true;
                    prevRezid = rezid;
                    prevIntervalInt = intervalInt;

                    while (ratio >= intervalInt) {
                        prevIntervalInt = intervalInt;
                        intervalInt *= mMidHor ? 2 : 5;
                        mMidHor = !mMidHor;
                    }
                    mMidHor = !mMidHor;

                    bool intervalModified = prevIntervalInt != intervalInt;
                    CalculateExtraLines(ref rezid, ref intervalInt);
                    if (intervalModified) {
                        CalculateExtraLines(ref prevRezid, ref prevIntervalInt);
                    } else {
                        prevRezid = rezid;
                        prevIntervalInt = intervalInt;
                    }
                }
            }
        }

        void CalculateExtraLines(ref float rezid, ref int intervalInt) {
            float percentSample = (1f - rezid) / intervalInt;
            if (rezid > percentSample) {
                float floatExtraLines = rezid / percentSample + marginErr;//add an error margin , e.g. 4 might pe represented like 3.999 etc...
                int extralines = (int)floatExtraLines;
                rezid -= extralines * percentSample;
                if (rezid < marginErr) {
                    rezid = 0;
                }
                intervalInt += extralines;
            }
        }


        //below are the methods which deal with PersistenceManager
        public void SaveData(string configName, Object obj) {
            PersistenceManager.SaveData(configName, obj, curveWindow, curveFormList, ActiveCurveForm, contextMenuManager.dictCurvesContextMenus);
            AlteredData = false;
        }

        public void LoadData(string configName, Object obj) {
            RemoveData();
            PersistenceManager.LoadData(configName, obj, curveWindow, this, curveFormList, contextMenuManager.dictCurvesContextMenus);
            WindowClosed = curveWindow.WindowClosed;
            AlteredData = false;
        }

        void RemoveData() {
            selectedKeyIndex = -1;
            curveFormList.Clear();
            contextMenuManager.dictCurvesContextMenus.Clear();
            usedColorList.Clear();
            FillListColor();
            ActiveCurveForm = new CurveForm();
            GradRect = ActiveCurveForm.gradRect;
        }

        public void NewWindow() {
            RemoveData();
            curveWindow.transform.localPosition = Vector3.zero;
            curveWindow.GetComponent<RectTransform>().sizeDelta = DEFAULT_WINDOW_SIZE;
            PersistenceManager.RemoveLastFileKey();
            PersistenceManager.Reset(curveWindow, this);
            AlteredData = false;
        }

        public List<string> GetNamesList() {
            return PersistenceManager.GetNamesList();
        }

        public void DeleteFile(string name) {
            PersistenceManager.DeleteFile(name);
        }

        public string GetLastFile() {
            return PersistenceManager.GetLastFile();
        }

        public int GetSelectedIndex() {
            return selectedKeyIndex;
        }

        public void SelectKey(int keyIndex) {
            selectedKeyIndex = keyIndex;
        }

        public ContextMenuManager GetContextMenuManager() {
            return contextMenuManager;
        }

        public void ResetKeyDragged() {
            keyDragged = false;
        }

        public void ClearMultipleKeySelection() {
            multipleKeySelection.ClearMultipleKeySelection();
        }

        public void SetOnAlterDelegate(System.Action onAlterAction) {
            this.onAlterAction = onAlterAction;
        }

        public void AlterData() {
            if (onAlterAction != null) {
                if (!AlteredData) {
                    AlteredData = true;
                    onAlterAction();
                }
            }

            onCurveChanged?.Invoke();
        }

        void AlterCurveData(AnimationCurve curve)
        {
            AlterData();
            Curves.TriggerUpdateCurve(curve);
        }
    }
}