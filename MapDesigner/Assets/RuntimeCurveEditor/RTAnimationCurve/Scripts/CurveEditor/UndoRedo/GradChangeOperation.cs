using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuntimeCurveEditor
{
    public class GradChangeOperation : Operation
    {

        CurveLines curveLines;

        float yMax;

        public GradChangeOperation(CurveLines curveLines, float yMax) {
            this.curveLines = curveLines;
            this.yMax = yMax;//prev yMax
        }

        public void Undo() {
            SwitchWithPrev();
        }

        public void Redo() {
            SwitchWithPrev();
        }

        void SwitchWithPrev() {
            float prev = curveLines.GradRect.yMax;
            curveLines.SetGradRectYMax(yMax);
            yMax = prev;
        }

    }
}
