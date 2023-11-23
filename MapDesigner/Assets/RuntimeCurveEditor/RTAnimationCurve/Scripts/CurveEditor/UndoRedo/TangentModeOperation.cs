using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuntimeCurveEditor
{
    public class TangentModeOperation : Operation
    {

        CurveLines curveLines;

        ContextMenu contextMenu;

        float inTangent;
        float outTangent;

        public TangentModeOperation(CurveLines curveLines, ContextMenu contextMenu, float inTangent, float outTangent) {
            this.curveLines = curveLines;
            this.contextMenu = new ContextMenu(contextMenu);
            this.inTangent = inTangent;
            this.outTangent = outTangent;
        }

        public void Undo() {
            SwitchContextMenu();
        }

        public void Redo() {
            SwitchContextMenu();
        }

        void SwitchContextMenu() {
            int keyIndex = curveLines.GetSelectedIndex();
            AnimationCurve curve = curveLines.ActiveCurveForm.SelectedCurve();
            Keyframe keyframe = curve[keyIndex];
            float inTangentTemp = keyframe.inTangent;
            float outTangentTemp = keyframe.outTangent;

            ContextMenu contextMenuCurrent = new ContextMenu(curveLines.GetContextMenuForCurrentKey());
            if (contextMenu.clampedAuto) {
                curveLines.ClampedAutoKey();
            } else if (contextMenu.auto) {
                curveLines.AutoKey();
            } else if (contextMenu.flat) {
                curveLines.FlatKey();
            } else if (contextMenu.freeSmooth) {
                curveLines.FreeSmoothKey();
            } else if (contextMenu.broken) {
                curveLines.BrokenKey();
                if (contextMenu.bothTangents.linear) {
                    curveLines.Linear(TangentPart.Both);
                } else if (contextMenu.bothTangents.constant) {
                    curveLines.Constant(TangentPart.Both);
                } else if (contextMenu.leftTangent.linear) {
                    curveLines.Linear(TangentPart.Left);
                } else if (contextMenu.leftTangent.constant) {
                    curveLines.Constant(TangentPart.Left);
                } else if (contextMenu.rightTangent.linear) {
                    curveLines.Linear(TangentPart.Right);
                } else if (contextMenu.rightTangent.constant) {
                    curveLines.Constant(TangentPart.Right);
                }
            }
            contextMenu = contextMenuCurrent;

            keyframe.inTangent = inTangent;
            keyframe.outTangent = outTangent;
            inTangent = inTangentTemp;
            outTangent = outTangentTemp;
            curve.MoveKey(keyIndex, keyframe);
        }
    }
}
