using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

namespace RuntimeCurveEditor
{
    /// <summary>
    /// Store/load data in/from persistent state.
    /// </summary>
    static class PersistenceManager
    {
        const string fileListKey = "fileList";//key for storing with PlayerPref	
        const string lastFileLoaded = "lastFileLoaded";//key for storing with PlayerPref

        /// <summary>
        /// Save all the fields of the AnimationCurve type together with their related gradRect,color and key's context menu options.
        /// </summary>
        public static void SaveData(string configName, Object obj, CurveWindow curveWindow, List<CurveForm> curveFormList,
                                      CurveForm activeCurveForm, Dictionary<AnimationCurve, List<ContextMenu>> dictCurvesContextMenus) {
            string data = "";
            //1st we store window's position and scale
            Vector3 locPos = curveWindow.transform.localPosition;
            Vector2 size = curveWindow.GetComponent<RectTransform>().sizeDelta;
            data += ":" + locPos.x + ":" + locPos.y + ":" + locPos.z + ":" + size.x + ":" + size.y;
            data += ":" + (curveWindow.WindowClosed ? 1 : 0);

            FieldInfo[] fields = obj.GetType().GetFields();
            data += ":" + curveFormList.Count;
            foreach (CurveForm curveForm in curveFormList) {
                data += ":" + ((curveForm.curve2 != null) ? 1 : 0);//is it a pair of curves or not?
                foreach (FieldInfo field in fields) {
                    if (field.FieldType == typeof(AnimationCurve) && (field.GetValue(obj) as AnimationCurve == curveForm.curve1)) {
                        data += ":" + field.Name;
                        break;
                    }
                }

                AnimationCurve curve = curveForm.curve1;
                data += ":" + curve.length;
                if (curve != null) {
                    foreach (Keyframe key in curve.keys) {
                        data += ":" + key.inTangent + ":" + key.outTangent + ":" + key.time + ":" + key.value;
                    }
                    //is this active curve or not
                    data += ":" + ((curve == activeCurveForm.curve1) ? 1 : 0);
                    //so this curve has been inside the curve editor
                    data += ":" + curveForm.firstCurveSelected;
                    data += ":" + curveForm.gradRect.x;
                    data += ":" + curveForm.gradRect.y;
                    data += ":" + curveForm.gradRect.width;
                    data += ":" + curveForm.gradRect.height;
                }
                data += ":";//empty string as the end for a curve data

                AnimationCurve curve2 = curveForm.curve2;
                if (curve2 != null) {
                    foreach (FieldInfo field in fields) {
                        if ((field.FieldType == typeof(AnimationCurve)) && (field.GetValue(obj) as AnimationCurve == curve2)) {
                            data += ":" + field.Name;
                            break;
                        }
                    }
                    data += ":" + curve2.length;
                    foreach (Keyframe key in curve2.keys) {
                        data += ":" + key.inTangent + ":" + key.outTangent + ":" + key.time + ":" + key.value;
                    }
                    data += ":";//empty string as the end for a curve data
                }

                if (curve != null) {
                    foreach (ContextMenu contextMenu in dictCurvesContextMenus[curve]) {
                        data += ":" + contextMenu;
                    }
                    data += ":";
                }

                if (curve2 != null) {
                    foreach (ContextMenu contextMenu in dictCurvesContextMenus[curve2]) {
                        data += ":" + contextMenu;
                    }
                    data += ":";
                }
            }

            foreach (FieldInfo field in fields) {
                if (field.FieldType == typeof(AnimationCurve)) {
                    AnimationCurve curve = field.GetValue(obj) as AnimationCurve;
                    CurveForm curveForm = curveFormList.Find(x => x.curve1 == curve);
                    if (curveForm == null) {
                        curveForm = curveFormList.Find(x => x.curve2 == curve);
                        if (curveForm == null) {//this curve is not added to the curve editor, however we store it too
                            data += ":" + field.Name;
                            data += ":" + curve.length;
                            foreach (Keyframe key in curve.keys) {
                                data += ":" + key.inTangent + ":" + key.outTangent + ":" + key.time + ":" + key.value;
                            }
                            data += ":";//empty string as the end for a curve data
                        }
                    }
                }
            }
            PlayerPrefs.SetString(configName, data);
            string fileList = PlayerPrefs.GetString(fileListKey);
            if (!fileList.Contains(":" + configName)) {
                fileList += ":" + configName;//this is the case,a saveAs took place,or an unnamed file has been saved 
                PlayerPrefs.SetString(fileListKey, fileList);
            }
            PlayerPrefs.SetString(lastFileLoaded, configName);
            PlayerPrefs.Save();
        }

        public static void LoadData(string configName, Object obj, CurveWindow curveWindow, CurveLines curveLines, List<CurveForm> curveFormList,
                                        Dictionary<AnimationCurve, List<ContextMenu>> dictCurvesContextMenus) {
            string data = PlayerPrefs.GetString(configName);
            string[] elements = data.Split(':');
            int i = 1;//first element should be empty string

            Vector3 locPos = Vector3.zero;
            locPos.x = float.Parse(elements[i]);
            i += 1;
            locPos.y = float.Parse(elements[i]);
            i += 1;
            locPos.z = float.Parse(elements[i]);
            i += 1;
            curveWindow.transform.localPosition = locPos;

            Vector2 size;
            size.x = float.Parse(elements[i]);
            i += 1;
            size.y = float.Parse(elements[i]);
            i += 1;
            curveWindow.GetComponent<RectTransform>().sizeDelta = size;

            curveWindow.WindowClosed = int.Parse(elements[i]) == 1;
            i += 1;

            int curveFormCount = int.Parse(elements[i]);
            i += 1;
            string name;
            AnimationCurve activeCurve = null;
            for (int k = 0; k < curveFormCount; ++k) {
                bool isPair = int.Parse(elements[i]) == 1;
                i += 1;
                name = elements[i];
                i += 1;
                int length = int.Parse(elements[i]);
                i += 1;

                FieldInfo fieldInfo = obj.GetType().GetField(name);
                AnimationCurve curve = fieldInfo.GetValue(obj) as AnimationCurve;

                if (length > 0) {
                    if (curve == null) {
                        curve = new AnimationCurve();//the curve is null,but after loading won't be
                    }

                    while (curve.length > 0) {
                        curve.RemoveKey(0);
                    }

                    for (int j = 0; j < length; ++j) {
                        Keyframe key = new Keyframe(float.Parse(elements[i + 2]), float.Parse(elements[i + 3]), float.Parse(elements[i]), float.Parse(elements[i + 1]));
                        curve.AddKey(key);
                        i += 4;
                    }

                    if (int.Parse(elements[i]) == 1) {
                        activeCurve = curve;
                    }
                    i += 1;
                    bool firstCurveSelected = bool.Parse(elements[i]);
                    i += 1;
                    Rect loadGradRect = new Rect();
                    loadGradRect.x = float.Parse(elements[i]);
                    i += 1;
                    loadGradRect.y = float.Parse(elements[i]);
                    i += 1;
                    loadGradRect.width = float.Parse(elements[i]);
                    i += 1;
                    loadGradRect.height = float.Parse(elements[i]);
                    i += 1;

                    if (elements[i] == "") {
                        i += 1;
                    }
                    int length2 = 0;
                    AnimationCurve curve2 = null;
                    if (isPair) {
                        name = elements[i];
                        i += 1;
                        length2 = int.Parse(elements[i]);
                        i += 1;
                        FieldInfo fieldInfo2 = obj.GetType().GetField(name);
                        curve2 = fieldInfo2.GetValue(obj) as AnimationCurve;
                        if (length2 > 0) {
                            if (curve2 == null) {
                                curve2 = new AnimationCurve();//the curve is null,but after loading won't be
                            }
                            while (curve2.length > 0) {
                                curve2.RemoveKey(0);
                            }
                            for (int j = 0; j < length2; ++j) {
                                Keyframe key = new Keyframe(float.Parse(elements[i + 2]), float.Parse(elements[i + 3]), float.Parse(elements[i]), float.Parse(elements[i + 1]));
                                curve2.AddKey(key);
                                i += 4;
                            }
                        } else {
                            curve2 = null;
                        }
                        fieldInfo2.SetValue(obj, curve2);
                        if (elements[i] == "") {
                            i += 1;
                        }
                    }

                    curveLines.AddRestoredCurveForm(curve, curve2, loadGradRect);

                    dictCurvesContextMenus[curve].Clear();
                    for (int j = 0; j < length; ++j) {
                        ContextMenu contextMenu = new ContextMenu();
                        contextMenu.UnpackData(elements[i]);
                        dictCurvesContextMenus[curve].Add(contextMenu);
                        i += 1;
                    }
                    if (elements[i] == "") {
                        i += 1;
                    }
                    if (null != curve2) {
                        dictCurvesContextMenus[curve2].Clear();
                        for (int j = 0; j < length2; ++j) {
                            ContextMenu contextMenu = new ContextMenu();
                            contextMenu.UnpackData(elements[i]);
                            dictCurvesContextMenus[curve2].Add(contextMenu);
                            i += 1;
                        }

                        if (!firstCurveSelected) {
                            CurveForm curveForm = curveFormList.Find(x => x.curve1 == curve);
                            curveForm.firstCurveSelected = false;
                        }
                        if (elements[i] == "") {
                            i += 1;
                        }
                    }

                } else {//the curve is not null,but after loading saved data,it will be
                    curve = null;
                }
                fieldInfo.SetValue(obj, curve);
            }

            curveLines.RestoreActiveCurve(activeCurve);
            Reset(curveWindow, curveLines);

            FieldInfo[] fields = obj.GetType().GetFields();

            foreach (FieldInfo field in fields) {
                if (field.FieldType == typeof(AnimationCurve)) {
                    AnimationCurve curve = field.GetValue(obj) as AnimationCurve;
                    if (curveFormList.Find(x => x.curve1 == curve) == null && curveFormList.Find(x => x.curve2 == curve) == null) {
                        //these is a curve not added to editor curve
                        name = elements[i];
                        i += 1;
                        int length = int.Parse(elements[i]);
                        i += 1;

                        if (curve == null) {
                            curve = new AnimationCurve();//the curve is null, but after loading it won't be
                        }

                        while (curve.length > 0) {
                            curve.RemoveKey(0);
                        }

                        for (int j = 0; j < length; ++j) {
                            Keyframe key = new Keyframe(float.Parse(elements[i + 2]), float.Parse(elements[i + 3]), float.Parse(elements[i]), float.Parse(elements[i + 1]));
                            curve.AddKey(key);
                            i += 4;
                        }
                        if (elements[i] == "") {
                            i += 1;
                        }
                        field.SetValue(obj, curve);
                    }
                }
            }
            PlayerPrefs.SetString(lastFileLoaded, configName);
            PlayerPrefs.Save();
        }

        public static List<string> GetNamesList() {
            List<string> namesList = new List<string>();
            if (PlayerPrefs.HasKey(fileListKey)) {
                string[] names = PlayerPrefs.GetString(fileListKey).Split(':');
                //start with 1 index, because at 0 we should have an empty string
                for (int i = 1; i < names.Length; ++i) {
                    namesList.Add(names[i]);
                }
            }
            return namesList;
        }

        public static void DeleteFile(string name) {
            string fileList = PlayerPrefs.GetString(fileListKey);
            fileList = fileList.Replace(":" + name, "");
            PlayerPrefs.SetString(fileListKey, fileList);
            PlayerPrefs.DeleteKey(name);
            if (PlayerPrefs.GetString(lastFileLoaded) == name) {
                PlayerPrefs.DeleteKey(lastFileLoaded);
            }
            PlayerPrefs.Save();
        }

        public static string GetLastFile() {
            string lastFileName = PlayerPrefs.GetString(lastFileLoaded);
            if (lastFileName == "") {
                lastFileName = null;
            }
            return lastFileName;
        }

        public static void RemoveLastFileKey() {
            PlayerPrefs.DeleteKey(lastFileLoaded);
        }

        public static void Reset(CurveWindow curveWindow, CurveLines curveLines) {
            curveWindow.ResetScreenSize();
            curveLines.ResetRect();
        }

    }
}
