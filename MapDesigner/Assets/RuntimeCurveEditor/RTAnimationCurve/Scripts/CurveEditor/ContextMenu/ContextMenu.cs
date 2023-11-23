
namespace RuntimeCurveEditor
{
    /// <summary>
    /// Context menu struct.
    /// Keeps current user selection for a key of a curve.
    /// </summary>
    public class ContextMenu
    {
        public bool clampedAuto;
        public bool auto;
        public bool freeSmooth;
        public bool flat;
        public bool broken;
        public TangentMenuStruct leftTangent;
        public TangentMenuStruct rightTangent;
        public TangentMenuStruct bothTangents;

        public ContextMenu() {

        }

        public ContextMenu(ContextMenu src) {
            clampedAuto = src.clampedAuto;
            auto = src.auto;
            freeSmooth = src.freeSmooth;
            flat = src.flat;
            broken = src.broken;
            leftTangent = src.leftTangent;
            rightTangent = src.rightTangent;
            bothTangents = src.bothTangents;
        }

        public void Reset() {
            clampedAuto = false;
            auto = false;
            freeSmooth = false;
            flat = false;
            broken = false;
            leftTangent.Reset();
            rightTangent.Reset();
            bothTangents.Reset();
        }

        internal int PackData() {
            int compactValue;
            compactValue = clampedAuto ? 1 : 0;
            compactValue <<= 1;
            compactValue += auto ? 1 : 0;
            compactValue <<= 1;
            compactValue += freeSmooth ? 1 : 0;
            compactValue <<= 1;
            compactValue += flat ? 1 : 0;
            compactValue <<= 1;
            compactValue += broken ? 1 : 0;
            compactValue <<= 3;
            compactValue += leftTangent.PackValue();
            compactValue <<= 3;
            compactValue += rightTangent.PackValue();
            compactValue <<= 3;
            compactValue += bothTangents.PackValue();
            return compactValue;
        }

        internal void UnpackData(string data) {
            int intData = int.Parse(data);
            bothTangents.UnpackValue(intData & 0x7);
            intData >>= 3;
            rightTangent.UnpackValue(intData & 0x7);
            intData >>= 3;
            leftTangent.UnpackValue(intData & 0x7);
            intData >>= 3;
            clampedAuto = (intData & 1 << 4) != 0;
            auto = (intData & 1 << 3) != 0;
            freeSmooth = (intData & 1 << 2) != 0;
            flat = (intData & 1 << 1) != 0;
            broken = (intData & 1) != 0;
        }

        public override string ToString() {
            return "" + PackData();
        }
    }

    /// <summary>
    /// Keeps the tangents for a key.
    /// </summary>
    public struct TangentMenuStruct
    {
        public bool free;
        public bool linear;
        public bool constant;
        public void Reset() {
            free = false;
            linear = false;
            constant = false;
        }

        internal int PackValue() {
            int compactValue;
            compactValue = free ? 1 : 0;
            compactValue <<= 1;
            compactValue += linear ? 1 : 0;
            compactValue <<= 1;
            compactValue += constant ? 1 : 0;
            return compactValue;
        }

        internal void UnpackValue(int val) {
            constant = (val & 1) != 0;
            linear = (val & 1 << 1) != 0;
            free = (val & 1 << 2) != 0;
        }
    }
}


