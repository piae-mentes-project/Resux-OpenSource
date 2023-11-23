using System.Collections.Generic;
using UnityEngine;

namespace RuntimeCurveEditor
{
    public class MoveOperation : Operation
    {

        CurveLines curveLines;

        List<int> keyIndices;

        List<Vector2> keyDiffs;

        List<float> keyYDiffs;

        public MoveOperation(CurveLines curveLines, List<int> keyIndices, List<Vector2> keyDiffs) {
            this.curveLines = curveLines;
            this.keyIndices = keyIndices;
            this.keyDiffs = keyDiffs;
        }

        public MoveOperation(CurveLines curveLines, int keyIndex, Vector2 keyDiff) {
            this.curveLines = curveLines;
            keyIndices = new List<int>(1);
            keyIndices.Add(keyIndex);
            keyDiffs = new List<Vector2>(1);
            keyDiffs.Add(keyDiff);
        }

        public MoveOperation(CurveLines curveLines, List<float> keyYDiffs) {
            this.curveLines = curveLines;
            this.keyYDiffs = keyYDiffs;
        }

        public void Undo() {
            MoveKeys(true);
            if ((keyDiffs != null) && keyDiffs.Count > 1) {//TODO improve, rather move back the keys selection rectangle (and disable it just when a different operation is undone)
                curveLines.ClearMultipleKeySelection();
            }
        }

        public void Redo() {
            MoveKeys(false);
        }

        void MoveKeys(bool back) {
            float sign = back ? 1 : -1;
            AnimationCurve curve = curveLines.ActiveCurveForm.SelectedCurve();
            if ((keyIndices != null) && (keyDiffs != null)) {
                int selectedKeyIndex = curveLines.GetSelectedIndex();
                bool revOrder = false;
                if ((keyIndices.Count > 1) && ((keyDiffs[0].x * sign < -Mathf.Epsilon) || (keyDiffs[keyIndices.Count - 1].x * sign < -Mathf.Epsilon))) {
                    //just do the movement of the keys, from the last to first, to avoid the change of order of the selected keys 
                    revOrder = true;
                }
                for (int i = 0; i < keyIndices.Count; ++i) {
                    int ii = revOrder ? (keyIndices.Count - i - 1) : i;
                    int index = keyIndices[ii];
                    int newIndex = MoveKey(curve, index, sign * keyDiffs[ii]);

                    curveLines.KeyHandling.CheckMovingBeyond(index, newIndex, selectedKeyIndex, keyIndices, ii);
                }
            } else {
                for (int i = 0; i < curve.length; ++i) {
                    Vector2 diff = sign * keyYDiffs[i] * Vector2.up;
                    MoveKey(curve, i, diff);
                }
            }
        }

        int MoveKey(AnimationCurve curve, int index, Vector2 diff) {
            Curves.TriggerUpdateCurve(curve);
            return curveLines.KeyHandling.MoveKeyByDiff(curve, index, diff);
        }

    }
}