using System.Collections.Generic;
using UnityEngine;

namespace RuntimeCurveEditor
{

    public class KeyHandling
    {

        public static int UNSELECTED = -1;

        CurveLines curveLines;

        public KeyHandling(CurveLines curveLines) {
            this.curveLines = curveLines;
        }

        public int MoveKey(AnimationCurve curve, int index, Vector2 screenDiff) {
            Vector2 keyframePos = Utils.Convert(new Vector2(curve[index].time, curve[index].value), curveLines.EntireRect, curveLines.GradRect);
            keyframePos.x += screenDiff.x;
            keyframePos.y += screenDiff.y;
            Vector2 keyframeGrad = Utils.Convert(keyframePos, curveLines.GradRect, curveLines.EntireRect);
            Keyframe keyframe = curve[index];
            keyframe.time = keyframeGrad.x;
            keyframe.value = keyframeGrad.y;
            return MoveKey(curve, index, keyframe);
        }

        public int MoveKey(int index, Vector2 screenDiff) {
            AnimationCurve curve = curveLines.ActiveCurveForm.SelectedCurve();
            return MoveKey(curve, index, screenDiff);
        }

        public int MoveKeyByDiff(AnimationCurve curve, int index, Vector2 diff) {
            Keyframe keyframe = curve[index];
            keyframe.value -= diff.y;
            keyframe.time -= diff.x;
            return MoveKey(curve, index, keyframe);
        }

        public int MoveKey(int index, Keyframe keyframe) {
            AnimationCurve curve = curveLines.ActiveCurveForm.SelectedCurve();
            return MoveKey(curve, index, keyframe);
        }

        public int MoveKey(AnimationCurve curve, int index, Keyframe newKeyframe) {
            int newIndex = curve.MoveKey(index, newKeyframe);

            if (newIndex == UNSELECTED) {//that's the case the first or last key has been removed, this key is not recovered by undo redo

                newIndex = (newKeyframe.time > 0) ? (curve.length - 1) : 0;

                List<ContextMenu> listContextMenus = curveLines.GetContextMenuManager().dictCurvesContextMenus[curve];
                listContextMenus.RemoveAt(newIndex + 1);
                CurveForm curveForm = curveLines.ActiveCurveForm;
                if (curveForm.firstCurveSelected) {
                    curveForm.curve1KeysCount -= 1;
                } else {
                    curveForm.curve2KeysCount -= 1;
                }

                if ((newIndex == 0) & (index > 0)) {
                    index -= 1;
                }
            }

            if (newIndex != index) {
                UpdateContextMenus(newIndex, index);
                UpdateAutoLinearSideEffects(index);
            }
            UpdateAutoLinearSideEffects(newIndex);

            return newIndex;
        }

        /// <summary>
        /// Deletes the key (normally only by context menu, but in some rare cases, the key is deleted when the user drags it outside of interval).
        /// </summary>
        public void DeleteKey(bool byContextMenu = true) {
            AnimationCurve curve = curveLines.ActiveCurveForm.SelectedCurve();
            if ((curve.keys.Length > 1) || !byContextMenu) {
                int selectedKeyIndex = curveLines.GetSelectedIndex();
                List<ContextMenu> listContextMenus = curveLines.GetContextMenuManager().dictCurvesContextMenus[curve];
                if (byContextMenu) {
                    curve.RemoveKey(selectedKeyIndex);
                }
                if (selectedKeyIndex > 0) {
                    //update neighbours if they are auto
                    curveLines.CheckUpdateAutoTangents(listContextMenus[selectedKeyIndex - 1], curve, selectedKeyIndex - 1);
                    //update the neighbour if is linear on this direction
                    if (listContextMenus[selectedKeyIndex - 1].leftTangent.linear) {
                        curveLines.UpdateLinearTangent(curve, selectedKeyIndex - 1, false);
                    }
                }

                if (selectedKeyIndex < curve.keys.Length) {//this condition should always be true          
                    curveLines.CheckUpdateAutoTangents(listContextMenus[selectedKeyIndex], curve, selectedKeyIndex);
                    //update the neighbour if is linear on this direction
                    if (listContextMenus[selectedKeyIndex].rightTangent.linear) {
                        curveLines.UpdateLinearTangent(curve, selectedKeyIndex, true);
                    }
                }
                listContextMenus.RemoveAt(selectedKeyIndex);
                selectedKeyIndex = UNSELECTED;
                curveLines.ResetKeyDragged();

                CurveForm curveForm = curveLines.ActiveCurveForm;
                if (curveForm.firstCurveSelected) {
                    curveForm.curve1KeysCount -= 1;
                } else {
                    curveForm.curve2KeysCount -= 1;
                }
            }
        }

        public void UpdateContextMenus(int newIndex, int index) {
            AnimationCurve curve = curveLines.ActiveCurveForm.SelectedCurve();
            List<ContextMenu> contextMenus = curveLines.GetContextMenuManager().dictCurvesContextMenus[curve];
            ContextMenu contextMenu = contextMenus[index];
            contextMenus.RemoveAt(index);
            contextMenus.Insert(newIndex, contextMenu);

        }

        public void UpdateAutoLinearSideEffects(int keyIndex) {
            AnimationCurve curve = curveLines.ActiveCurveForm.SelectedCurve();
            List<ContextMenu> listContextMenus = curveLines.GetContextMenuManager().dictCurvesContextMenus[curve];

            curveLines.CheckUpdateAutoTangents(listContextMenus[keyIndex], curve, keyIndex);
            //adapt neighbours also if they are auto
            if (keyIndex > 0) {
                curveLines.CheckUpdateAutoTangents(listContextMenus[keyIndex - 1], curve, keyIndex - 1);
            }
            if (keyIndex < curve.keys.Length - 1) {
                curveLines.CheckUpdateAutoTangents(listContextMenus[keyIndex + 1], curve, keyIndex + 1);
            }

            if (keyIndex > 0) {
                if (listContextMenus[keyIndex].leftTangent.linear) {
                    curveLines.UpdateLinearTangent(curve, keyIndex, true);
                }
                //update the neighbour if is linear on this direction
                if (listContextMenus[keyIndex - 1].rightTangent.linear) {
                    curveLines.UpdateLinearTangent(curve, keyIndex - 1, false);
                }
            }
            if (keyIndex < curve.keys.Length - 1) {
                if (listContextMenus[keyIndex].rightTangent.linear) {
                    curveLines.UpdateLinearTangent(curve, keyIndex, false);
                }
                //update the neighbour if is linear on this direction
                if (listContextMenus[keyIndex + 1].leftTangent.linear) {
                    curveLines.UpdateLinearTangent(curve, keyIndex + 1, true);
                }
            }
        }

        public void CheckMovingBeyond(int index, int newIndex, int selectedKeyIndex, List<int> keyIndices, int ii) {
            if (newIndex != index) {
                if (index == selectedKeyIndex) {
                    selectedKeyIndex = newIndex;
                } else if ((index < selectedKeyIndex) && (selectedKeyIndex <= newIndex)) {
                    selectedKeyIndex -= 1;
                } else if ((selectedKeyIndex < index) && (newIndex <= selectedKeyIndex)) {
                    selectedKeyIndex += 1;
                }
                keyIndices[ii] = newIndex;
                curveLines.SelectKey(selectedKeyIndex);
            }
        }

    }
}
