using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuntimeCurveEditor
{
    public interface Operation
    {
        void Undo();

        void Redo();
    }
}
