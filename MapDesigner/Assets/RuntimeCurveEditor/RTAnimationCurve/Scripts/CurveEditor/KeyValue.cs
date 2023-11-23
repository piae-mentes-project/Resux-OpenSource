using UnityEngine;
using UnityEngine.UI;

namespace RuntimeCurveEditor
{
    public class KeyValue : MonoBehaviour
    {

        public Text label;

        public RectTransform panel;

        public InputField timeInput;

        public InputField valueInput;

        public RectTransform panelEdit;

        Vector2 halfSize;

        Vector2 editHalfSize;

        InterfaceKeyEditListener interfaceKeyEditListener;

        RectTransform timeInputRectTransform;

        RectTransform valueInputRectTransform;

        // Use this for initialization
        void Start() {
            timeInputRectTransform = timeInput.GetComponent<RectTransform>();
            valueInputRectTransform = valueInput.GetComponent<RectTransform>();

            halfSize = panel.sizeDelta * 0.5f;
            editHalfSize.x = -timeInput.GetComponent<RectTransform>().sizeDelta.x * 0.25f;
            editHalfSize.y = -timeInput.GetComponent<RectTransform>().sizeDelta.y * 1.5f;
        }

        // Update is called once per frame
        void Update() {

        }

        public void SetLabelEnabled(bool enable, InterfaceKeyEditListener interfaceKeyEditListener = null) {
            panel.gameObject.SetActive(enable);
            gameObject.SetActive(enable);
            this.interfaceKeyEditListener = interfaceKeyEditListener;
        }

        public void SetKeyEditEnabled(bool enable, InterfaceKeyEditListener interfaceKeyEditListener = null) {
            panelEdit.gameObject.SetActive(enable);
            gameObject.SetActive(enable);
            this.interfaceKeyEditListener = interfaceKeyEditListener;
        }

        public void SetTimeValueText(float time, float value) {
            label.text = string.Format("{0:0.0000}", time) + ", " + string.Format("{0:0.0000}", value);
        }

        public void SetLabelPos(Vector2 pos) {
            panel.position = pos + halfSize;
        }

        public void SetTimeValueEditFields(float time, float value) {
            timeInput.text = string.Format("{0:0.#######}", time);
            valueInput.text = string.Format("{0:0.#######}", value);
        }

        public void SetPanelEditPos(Vector2 pos) {
            panelEdit.position = pos + editHalfSize;
        }

        public void OnKeyTimeChange(string strTime) {
            float time = 0;
            if (float.TryParse(strTime, out time)) {
                interfaceKeyEditListener.ChangeKeyTime(time);
            }
        }

        public void OnKeyValueChange(string strValue) {
            float value = 0;
            if (float.TryParse(strValue, out value)) {
                interfaceKeyEditListener.ChangeKeyValue(value);
            }
        }

        public bool IsKeyEditVisible() {
            return panelEdit.gameObject.activeSelf;
        }

        public bool FocusOnInputFields() {
            Vector2 pos = Input.mousePosition;
            return timeInputRectTransform.rect.Contains(pos - (Vector2)timeInputRectTransform.position) ||
                   valueInputRectTransform.rect.Contains(pos - (Vector2)valueInputRectTransform.position);
        }

        public void ReadKeyFrameValues(out float time, out float value) {
            float.TryParse(timeInput.text, out time);
            float.TryParse(valueInput.text, out value);
        }

    }
}
