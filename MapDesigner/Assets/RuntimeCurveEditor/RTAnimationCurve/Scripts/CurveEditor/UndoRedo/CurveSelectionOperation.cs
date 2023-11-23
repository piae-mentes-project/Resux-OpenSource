
namespace RuntimeCurveEditor
{
    public class CurveSelectionOperation : Operation
    {
        CurveLines curveLines;

        int prevCurveFormIndex;
        bool prevCurveSelected;
        int prevSelectedIndex;

        public CurveSelectionOperation(CurveLines curveLines, int curveFormIndex, bool firstCurveSelected, int selectedIndex) {
            this.curveLines = curveLines;
            prevCurveFormIndex = curveFormIndex;
            prevCurveSelected = firstCurveSelected;
            prevSelectedIndex = selectedIndex;
        }

        public void Undo() {
            SwitchCurveKeySelection();
        }

        public void Redo() {
            SwitchCurveKeySelection();
        }

        void SwitchCurveKeySelection() {
            int tempCurveFormIndex = curveLines.GetCurveFormIndex();
            bool tempFirstCurveSelected = curveLines.GetFirstCurveSelected();
            int tempSelectedIndex = curveLines.GetSelectedIndex();
            curveLines.SelectCurveForm(prevCurveFormIndex);
            curveLines.SelectFirstCurve(prevCurveSelected);
            curveLines.SelectKey(prevSelectedIndex);
            prevCurveFormIndex = tempCurveFormIndex;
            prevCurveSelected = tempFirstCurveSelected;
            prevSelectedIndex = tempSelectedIndex;
        }

    }
}
