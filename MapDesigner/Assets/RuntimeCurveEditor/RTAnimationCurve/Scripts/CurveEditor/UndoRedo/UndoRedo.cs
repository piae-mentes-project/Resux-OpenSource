using System.Collections.Generic;

namespace RuntimeCurveEditor
{
    public class UndoRedo
    {

        int undoIndex;

        List<Operation> operations = new List<Operation>();

        const int MAX_OPERATIONS = 20;

        public void Undo() {
            if (operations.Count > undoIndex) {
                undoIndex += 1;//TODO when undo limit reached, an event could be triggered, to notice the UI layer
                Operation undoneOperation = operations[operations.Count - undoIndex];
                undoneOperation.Undo();
                //TODO show redo icon, if not visible
            }
        }

        public void Redo() {
            if (undoIndex > 0) {
                operations[operations.Count - undoIndex].Redo();
                undoIndex -= 1;//TODO when undo index gets 0, an event could be triggered, to notice the UI layer
                               //TODO show undo icon, if not visible
            }
        }

        public void AddOperation(Operation operation) {
            while (undoIndex > 0) {//once a new operation takes place the undone operations are lost forever
                undoIndex -= 1;
                Operation lastOperation = operations[operations.Count - 1];
                operations.RemoveAt(operations.Count - 1);
            }
            operations.Add(operation);
            undoIndex = 0;
            if (operations.Count > MAX_OPERATIONS) {
                operations.RemoveAt(0);
            }
            //TODO show undo icon, if not visible
        }

        public void ClearStack() {
            operations.Clear();//TODO hide/disable undo/redo icons
        }

    }
}