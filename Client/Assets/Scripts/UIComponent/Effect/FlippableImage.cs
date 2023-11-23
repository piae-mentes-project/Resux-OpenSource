using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Resux.UI
{
    [RequireComponent(typeof(RectTransform), typeof(Graphic)), DisallowMultipleComponent]
    [AddComponentMenu("UI/Effects/Extensions/FlippableImage")]
    public class FlippableImage : MonoBehaviour, IMeshModifier
    {

        [SerializeField] private bool _horizontal = false;
        [SerializeField] private bool _veritical = false;

        public bool horizontal
        {
            get { return _horizontal; }
            set { _horizontal = value; OnValidate(); }
        }

        public bool vertical
        {
            get { return _veritical; }
            set { _veritical = value; OnValidate(); }
        }

        protected void OnValidate()
        {
            GetComponent<Graphic>().SetVerticesDirty();
        }

        public void ModifyMesh(VertexHelper verts)
        {
            RectTransform rt = transform as RectTransform;

            for (int i = 0; i < verts.currentVertCount; ++i)
            {
                UIVertex uiVertex = new UIVertex();
                verts.PopulateUIVertex(ref uiVertex, i);

                // Modify positions
                uiVertex.position = new Vector3(
                    (_horizontal ? (uiVertex.position.x + (rt.rect.center.x - uiVertex.position.x) * 2) : uiVertex.position.x),
                    (_veritical ? (uiVertex.position.y + (rt.rect.center.y - uiVertex.position.y) * 2) : uiVertex.position.y),
                    uiVertex.position.z
                );

                // Apply
                verts.SetUIVertex(uiVertex, i);
            }
        }

        public void ModifyMesh(Mesh mesh)
        {
            //Obsolete member implementation
        }

    }
}
