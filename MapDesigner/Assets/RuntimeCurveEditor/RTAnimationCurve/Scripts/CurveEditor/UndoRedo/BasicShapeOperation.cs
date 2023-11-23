using System.Collections.Generic;
using UnityEngine;
using System;

namespace RuntimeCurveEditor
{
    public class BasicShapeOperation : Operation
    {

        CurveLines curveLines;

        Keyframe[] keyframes;

        List<ContextMenu> curveContextMenus;

        public BasicShapeOperation(CurveLines curveLines) {
            this.curveLines = curveLines;
            StoreCurentCurve();
        }

        public void Undo() {
            Switch();
        }

        public void Redo() {
            Switch();
        }

        void Switch() {
            Keyframe[] tempKeyframes = keyframes.Clone() as Keyframe[];
            List<ContextMenu> tempCurveContextMenus = new List<ContextMenu>(curveContextMenus);
            StoreCurentCurve();
            curveLines.ReplaceActiveCurve(tempKeyframes, tempCurveContextMenus);
        }

        void StoreCurentCurve() {
            AnimationCurve curve = curveLines.ActiveCurveForm.SelectedCurve();
            if (curve != null) {
                keyframes = new Keyframe[curve.length];
                Array.Copy(curve.keys, keyframes, curve.length);
                curveContextMenus = new List<ContextMenu>(curveLines.GetContextMenuManager().dictCurvesContextMenus[curve]);
                Curves.TriggerUpdateCurve(curve);
            }
        }

    }
}
