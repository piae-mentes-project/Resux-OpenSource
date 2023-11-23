using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuntimeCurveEditor
{
    public class DeleteOperation : Operation
    {

        CurveLines curveLines;

        Keyframe keyframe;

        ContextMenu contextMenu;

        public DeleteOperation(CurveLines curveLines) {
            this.curveLines = curveLines;
            Init();
        }

        public void Undo() {
            curveLines.AddKey(keyframe, contextMenu);
        }

        public void Redo() {
            curveLines.DeleteKeySimple();
        }

        void Init() {
            AnimationCurve curve = curveLines.ActiveCurveForm.SelectedCurve();
            int selectedKeyIndex = curveLines.GetSelectedIndex();
            keyframe = curve[selectedKeyIndex];
            List<ContextMenu> listContextMenus = curveLines.GetContextMenuManager().dictCurvesContextMenus[curve];
            contextMenu = listContextMenus[selectedKeyIndex];
        }

    }
}
