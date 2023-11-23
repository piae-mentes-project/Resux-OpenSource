// Runtime Curve Editor
// Copyright © 2013-2021 Rus Artur PFA
// center@republicofhandball.com
//----------------------------------------------
using UnityEngine;
using System.Collections.Generic;

namespace RuntimeCurveEditor
{
    public class RTAnimationCurve : MonoBehaviour
    {
        public GameObject rootCurveEditorObject;

        //root game object for the whole curve editor window
        Transform rootCurveEditor;
        //use this for showing/hiding the curve editor window
        CurveLines curveLines;

        /// <summary>
        /// Gets or sets the gradations rect.
        /// </summary>
        public Rect GradRect {
            set {
                if (curveLines != null) {
                    curveLines.SetGradRectAndActiveForm(value);
                }
            }
            get {
                return (curveLines == null) ? Rect.zero : curveLines.GradRect;
            }
        }

        /// <summary>
        /// Gets the active curve (or curve 1, in the case a path with two curves is active).
        /// </summary>
        public AnimationCurve ActiveCurve {
            get {
                return (null == curveLines) ? null : curveLines.ActiveCurveForm.curve1;
            }
        }

        /// <summary>
        /// Gets the color of the active curve.
        /// </summary>
        public Color ActiveCurveColor {
            get {
                return (null == curveLines) ? Color.clear : curveLines.ActiveCurveForm.color;
            }
        }


        /// <summary>
        /// Shows the curve editor(instantiate if it's not available).
        /// </summary>
        public void ShowCurveEditor() {
            Init();
            if (curveLines != null) {
                curveLines.ShowWindow();
            }
        }

        /// <summary>
        /// Closes(hides) the curve editor.
        /// </summary>
        public void CloseCurveEditor() {
            if (curveLines != null) {
                curveLines.CloseWindow();
            }
        }

        public bool IsCurveEditorClosed() {
            return (curveLines == null) ? true : curveLines.WindowClosed;
        }

        /// <summary>
        /// Add the specified animCurve to the curve editor(if it's already added, nothing happens).
        /// </summary>
        /// <returns>false if the curve couldn't be added.
        /// </returns>
        public bool Add(ref AnimationCurve curve) {
            if (curveLines == null) {
                return false;
            }
            if (!curveLines.CurveShown(curve)) {
                if (curve == null) {
                    curve = new AnimationCurve();
                }
                if (curve.length == 0) {
                    curve.AddKey(0f, 0f);
                }
                curveLines.AddCurveForm(curve, null);
            }
            return true;
        }

        /// <summary>
        /// Similar to the above Add method, but it adds a path of two curves.
        /// If the pair of curve is already added ,nothing happens,if animCurve1 is added, then the second curve is updated.
        /// </summary>
        public bool Add(ref AnimationCurve curve1, ref AnimationCurve curve2) {
            if (curveLines == null) {
                return false;
            }
            if (!curveLines.CurvesShown(curve1, curve2)) {
                if (curve1 == null) {
                    curve1 = new AnimationCurve();
                }
                if (curve1.length == 0) {
                    curve1.AddKey(0f, 0f);
                }
                if (curve2 == null) {
                    curve2 = new AnimationCurve();
                }
                if (curve2.length == 0) {
                    curve2.AddKey(0f, 0f);
                }
                curveLines.AddCurveForm(curve1, curve2);
            }
            return true;
        }

        /// <summary>
        /// Remove the specified curve(or pairs of curves,if curve1 equals the given curve).
        /// </summary>
        public void Remove(AnimationCurve curve) {
            if (null != curveLines) {
                curveLines.RemoveCurve(curve);
            }
        }

        /// <summary>
        /// Set the gradations' Y range(and keeps the existing X range) .
        /// </summary>
        public void SetGradYRange(float yMin, float yMax) {
            if (curveLines != null) {
                Rect tempRect = curveLines.GradRect;
                tempRect.yMin = yMin;
                tempRect.yMax = yMax;
                curveLines.SetGradRectAndActiveForm(tempRect);
            }
        }

        /// <summary>
        /// Set the gradations' X range(and keeps the existing Y range) .
        /// </summary>
        public void SetGradXRange(float xMin, float xMax) {
            if (curveLines != null) {
                Rect tempRect = curveLines.GradRect;
                tempRect.xMin = xMin;
                tempRect.xMax = xMax;
                curveLines.SetGradRectAndActiveForm(tempRect);
            }
        }

        /// <summary>
        /// 启用曲线的初始化限制
        /// </summary>
        /// <param name="isUse"></param>
        public void SetUseInitLimitRect(bool isUse)
        {
            if (curveLines != null)
            {
                curveLines.UseInitLimitRect = isUse;
            }
        }

        /// <summary>
        /// 设置曲线的初始Y范围（使用预置曲线覆盖时的范围）
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public void SetCurveInitYRange(float min, float max)
        {
            if (curveLines != null)
            {
                var rect = curveLines.InitLimitRect;
                rect.yMin = min;
                rect.yMax = max;
                curveLines.InitLimitRect = rect;
            }
        }

        /// <summary>
        /// 设置曲线的初始X范围（使用预置曲线覆盖时的范围）
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public void SetCurveInitXRange(float min, float max)
        {
            if (curveLines != null)
            {
                var rect = curveLines.InitLimitRect;
                rect.xMin = min;
                rect.xMax = max;
                curveLines.InitLimitRect = rect;
            }
        }

        /// <summary>
        /// Saves the data.
        /// </summary>
        /// <param name='name'>
        /// Name of the configuration to be stored.
        /// </param>
        /// <param name='obj'>
        /// Object whose public fields of AnimationCurve type, will be stored.
        /// </param>
        public void SaveData(string name, Object obj) {
            Init();
            if (curveLines != null) {
                curveLines.SaveData(name, obj);
            }
        }

        /// <summary>
        /// Loads the data.
        /// </summary>
        /// <param name='name'>
        /// Name of the configuration to be loaded.
        /// </param>
        /// <param name='obj'>
        /// Object whose fields of AnimationCurve type, will be loaded.
        /// </param>
        public void LoadData(string name, Object obj) {
            Init();
            if (curveLines != null) {
                curveLines.ShowWindow();
                curveLines.LoadData(name, obj);
            }
        }

        /// <summary>
        /// Remove all the curves and place/resize the window to initial.
        /// </summary>
        public void NewWindow() {
            if (curveLines != null) {
                curveLines.NewWindow();
            }
        }

        /// <summary>
        /// True if the curve is visible in the editor(if it's added to the editor).
        /// </summary>
        public bool CurveVisible(AnimationCurve curve) {
            return (curveLines == null) ? false : curveLines.CurveShown(curve);
        }

        /// <summary>
        /// True if the curves are visible ,as path, in the editor(if the path is added to the editor).
        /// </summary>	
        public bool CurvesVisible(AnimationCurve curve1, AnimationCurve curve2) {
            return (curveLines == null) ? false : curveLines.CurvesShown(curve1, curve2);
        }

        /// <summary>
        /// Returns true if something has been modified in the current configuration
        /// (a curve added/removed/modified/selected, the editor's window resized/moved, grad Y range changed,
        /// or any key's context menu changed)
        /// </summary>
        public bool DataAltered() {
            return (curveLines == null) ? false : curveLines.AlteredData;
        }

        /// <summary>
        /// Register a callback, when curve editor data is altered
        /// </summary>
        /// <param name="onAlterAction">a basic signature method to be called back</param>
        /// <returns></returns>
        public bool ListenOnDataAlter(System.Action onAlterAction) {
            bool success = false;
            if (curveLines != null) {
                curveLines.SetOnAlterDelegate(onAlterAction);
                success = true;
            }
            return success;
        }

        /// <summary>
        /// Returns the names of all saved configurations.
        /// </summary>
        public List<string> GetNamesList() {
            Init();
            return (curveLines == null) ? new List<string>() : curveLines.GetNamesList();
        }

        /// <summary>
        /// Deletes file.
        /// </summary>
        public void DeleteFile(string name) {
            if (curveLines != null) {
                curveLines.DeleteFile(name);
            }
        }

        /// <summary>
        /// Gets the last loaded file(if no such file, returns null).
        /// </summary>
        public string GetLastFile() {
            Init();
            return (curveLines == null) ? null : curveLines.GetLastFile();//?. operator is not suported w
        }

        /// <summary>
        /// 窗口关闭的监听
        /// </summary>
        /// <param name="onOpen"></param>
        public void AddWindowOpenListener(System.Action onOpen)
        {
            curveLines.onWindowOpen += onOpen;
        }

        /// <summary>
        /// 窗口关闭的监听
        /// </summary>
        /// <param name="onClose"></param>
        public void AddWindowCloseListener(System.Action onClose)
        {
            curveLines.onWindowClose += onClose;
        }

        /// <summary>
        /// 动画曲线发生改变的监听
        /// </summary>
        /// <param name="onChanged"></param>
        public void AddAnimationCurveChangedListener(System.Action onChanged)
        {
            curveLines.onCurveChanged += onChanged;
        }

        void Init() {
            if (rootCurveEditor == null) {
                rootCurveEditor = Instantiate(rootCurveEditorObject).transform;
                if (curveLines == null) {
                    curveLines = rootCurveEditor.Find("CameraCurve").GetComponent<CurveLines>();
                }
            }
        }

    }
}
