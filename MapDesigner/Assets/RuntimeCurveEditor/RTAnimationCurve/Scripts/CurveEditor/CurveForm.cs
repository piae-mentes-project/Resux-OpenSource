using UnityEngine;

namespace RuntimeCurveEditor
{
    /// <summary>
    /// Keeps information for a curve (or two if the path between two curves is desired).
    /// If curve2 is not null than a path between the two curves is drawn (that's the case when a random between curves could be achieved).
    /// </summary>
    public class CurveForm
    {
        //default values for the gradations rectangle
        public static float X_MAXIM = 5.0f;
        public static float X_MIN = 0.0f;
        public static float Y_MAXIM = 1.7f;
        public static float Y_MIN = -Y_MAXIM;

        public AnimationCurve curve1;
        public AnimationCurve curve2;
        public int curve1KeysCount;//backup keycount, so that if new keys are added outside of this curve editor, the context menu struct can be correctly updated
        public int curve2KeysCount;
        public Color color;
        public Color shadyColor;
        public bool firstCurveSelected;
        public Rect gradRect;

        public CurveForm() : this(null, null, Color.black) {
        }

        public CurveForm(AnimationCurve curve1, AnimationCurve curve2, Color color) {
            this.curve1 = curve1;
            this.curve2 = curve2;
            this.color = color;
            shadyColor = color / 2;
            curve1KeysCount = (curve1 == null) ? 0 : curve1.length;
            curve2KeysCount = (curve2 == null) ? 0 : curve2.length;
            firstCurveSelected = true;
            gradRect = Rect.MinMaxRect(X_MIN, Y_MIN, X_MAXIM, Y_MAXIM);
        }

        public AnimationCurve SelectedCurve() {
            return firstCurveSelected ? curve1 : curve2;
        }

        public AnimationCurve UnselectedCurve()
        {
            return firstCurveSelected ? curve2 : curve1;
        }
    }
}

