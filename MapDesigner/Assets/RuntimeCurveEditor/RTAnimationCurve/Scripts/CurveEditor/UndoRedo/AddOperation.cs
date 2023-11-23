using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuntimeCurveEditor
{
    public class AddOperation : Operation
    {

        CurveLines curveLines;

        Vector2 pos;

        public AddOperation(CurveLines curveLines, Vector2 pos) {
            this.curveLines = curveLines;
            this.pos = pos;
        }

        public void Undo() {
            curveLines.DeleteKeySimple();
        }

        public void Redo() {
            curveLines.AddKey(pos);
        }

    }
}
