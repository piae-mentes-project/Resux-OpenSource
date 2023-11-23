using UnityEngine;
using UnityEngine.EventSystems;

namespace RuntimeCurveEditor
{
    public class WindowDragging : EventTrigger
    {

        public CurveWindow curveWindow;

        bool dragging;

        Vector2 diff;

        Vector2 mousePos;

        // Update is called once per frame
        void Update() {
            if (dragging) {
                curveWindow.UpdatePosition((Vector2)Input.mousePosition - mousePos);
                mousePos = Input.mousePosition;
            }
        }

        public override void OnPointerDown(PointerEventData eventData) {
            if (curveWindow.NormalCursorType()) {
                dragging = true;
                mousePos = Input.mousePosition;
            }
        }

        public override void OnPointerUp(PointerEventData eventData) {
            dragging = false;
        }

    }
}
