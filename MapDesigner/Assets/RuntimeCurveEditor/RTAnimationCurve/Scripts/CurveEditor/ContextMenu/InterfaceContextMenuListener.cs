
namespace RuntimeCurveEditor
{
    public interface InterfaceContextMenuListener
    {

        void DeleteKey();
        void EditKey();

        void AutoKey(bool addToUndoStack);
        void ClampedAutoKey(bool addToUndoStack);
        void FreeSmoothKey(bool addToUndoStack);
        void FlatKey(bool addToUndoStack);
        void BrokenKey(bool addToUndoStack);

        void Free(TangentPart tangentPart, bool addToUndoStack);
        void Linear(TangentPart tangentPart, bool addToUndoStack);
        void Constant(TangentPart tangentPart, bool addToUndoStack);

        void AddKey();

    }
}
