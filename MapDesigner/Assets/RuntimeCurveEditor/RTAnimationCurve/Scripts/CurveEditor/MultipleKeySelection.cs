using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace RuntimeCurveEditor
{
    public enum ResizePart
    {
        None,
        Left,
        Right,
        Top,
        Bottom,
    }

    public class MultipleKeySelection : MonoBehaviour
    {
        public Material lineMaterial;
        public bool MultipleKeySelectionOn { get; private set; }//true, just while the user select an area 
                                                                //defines the selected area
        bool triggerUpdate;

        Vector2 startPoint;
        Vector2 endPoint;

        List<int> selectedKnots = new List<int>();
        //defines a rectangle that includes all selected knots
        Vector2 leftBottom;
        Vector2 rightTop;
        Vector2 leftPadBottom;
        Vector2 rightPadTop;

        static Color GRAY_TRANSP = new Color(0.63f, 0.63f, 0.63f, 0.35f);
        static Color MIDNIGHT_BLUE = new Color(0.098f, 0.098f, 0.44f, 1f);

        static Vector2 PAD = new Vector2(10, 10);//e.g. useful, when the seleted knots are on the same line
        static Vector2 HALF_PAD = PAD * 0.5f;//e.g. useful, when the seleted knots are on the same line
        static Vector2 QUARTER_PAD = HALF_PAD * 0.5f;//e.g. useful, when the seleted knots are on the same line
        
        Camera currentCamera;

        Mesh meshQuad;
        Mesh meshLines;
        Mesh meshQuadSelection;
        Mesh meshLinesSelectionResize;

        const int QUAD_VERTICES_COUNT = 4;
        const int LINES_SELECTION_VERTICES_COUNT = 2 * QUAD_VERTICES_COUNT;

        const float Z_POS = Curves.Z_POS_KNOTS - Curves.Z_DIFF;

        float resizeLineLeft;
        float resizeLineRight;
        float resizeLineTop;
        float resizeLineBottom;
               
        // Use this for initialization
        void Start()
        {
            Color[] quadColors = new Color[QUAD_VERTICES_COUNT];
            int[] quadIndices = new int[QUAD_VERTICES_COUNT];
            Color[] linesSelectionColors = new Color[LINES_SELECTION_VERTICES_COUNT];
            int[] linesSelectionIndices = new int[LINES_SELECTION_VERTICES_COUNT];

            meshQuad = new Mesh();
            meshLines = new Mesh();
            meshQuadSelection = new Mesh();
            meshLinesSelectionResize = new Mesh();

            for (int i = 0; i < QUAD_VERTICES_COUNT; ++i)
            {
                quadColors[i] = GRAY_TRANSP;
                quadIndices[i] = i;
            }
            meshQuad.SetVertexBufferParams(QUAD_VERTICES_COUNT, new[]{new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3)});
            meshQuad.SetIndices(quadIndices, MeshTopology.Quads, 0);
            meshQuad.SetColors(quadColors);
            meshQuadSelection.SetVertexBufferParams(QUAD_VERTICES_COUNT, new[] { new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3) });
            meshQuadSelection.SetIndices(quadIndices, MeshTopology.Quads, 0);
            meshQuadSelection.SetColors(quadColors);

            for (int i = 0; i < LINES_SELECTION_VERTICES_COUNT; ++i)
            {
                linesSelectionColors[i] = MIDNIGHT_BLUE;
                linesSelectionIndices[i] = i;
            }
            meshLinesSelectionResize.SetVertexBufferParams(LINES_SELECTION_VERTICES_COUNT, new[] { new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3) });
            meshLinesSelectionResize.SetIndices(linesSelectionIndices, MeshTopology.Lines, 0);
            meshLinesSelectionResize.SetColors(linesSelectionColors);

            currentCamera = GetComponent<Camera>();
        }

        // Update is called once per frame
        void Update() {
            if (MultipleKeySelectionOn) {
                endPoint = Input.mousePosition;

                SetSelectedKnots();

                if (Input.GetMouseButtonUp(0)) {
                    MultipleKeySelectionOn = false;
                }
            }

            DrawSelection();
        }

        public void UpdateSelectedKnots(Rect gridRect) {
            if (selectedKnots.Count > 1)
            {
                bool first = true;
                foreach (int i in selectedKnots)
                {
                    Knot knot = Curves.activeCurveKnots[i];
                    SetRectLimits(knot, first);
                    if (first)
                    {
                        first = false;
                    }
                }                

                if (leftBottom.x < gridRect.xMin)
                {
                    leftBottom.x = gridRect.xMin;
                }
                if (gridRect.xMax < rightTop.x)
                {
                    rightTop.x = gridRect.xMax;
                }
                if (gridRect.yMax < rightTop.y)
                {
                    rightTop.y = gridRect.yMax;
                }
                if (leftBottom.y < gridRect.yMin)
                {
                    leftBottom.y = gridRect.yMin;
                }

                triggerUpdate = true;
            }
        }

        void SetSelectedKnots() {
            selectedKnots.Clear();
            int i = 0;
            bool first = true;
            foreach (Knot knot in Curves.activeCurveKnots) {
                if (knot.visible && Contains(knot.point, startPoint, endPoint)) {
                    selectedKnots.Add(i);
                    SetRectLimits(knot, first);
                    if (first) {
                        first = false;
                    }
                }
                i += 1;
            }
        }

        void SetRectLimits(Knot knot, bool first) {
            if (first) {
                leftBottom = knot.point;
                rightTop = knot.point;
            } else {
                if (knot.point.x < leftBottom.x) {
                    leftBottom.x = knot.point.x;
                }
                if (rightTop.x < knot.point.x) {
                    rightTop.x = knot.point.x;
                }
                if (knot.point.y < leftBottom.y) {
                    leftBottom.y = knot.point.y;
                }
                if (rightTop.y < knot.point.y) {
                    rightTop.y = knot.point.y;
                }

                PadRect();
            }
        }

        void DrawSelection() {
            if (MultipleKeySelectionOn) {
                List<Vector3> verticesLines = new List<Vector3>();
                verticesLines.Add(currentCamera.ScreenToWorldPoint(new Vector3(startPoint.x, startPoint.y, 0)));
                verticesLines.Add(currentCamera.ScreenToWorldPoint(new Vector3(startPoint.x, endPoint.y, 0)));
                verticesLines.Add(currentCamera.ScreenToWorldPoint(new Vector3(endPoint.x, endPoint.y, 0)));
                verticesLines.Add(currentCamera.ScreenToWorldPoint(new Vector3(endPoint.x, startPoint.y, 0)));
                verticesLines.Add(currentCamera.ScreenToWorldPoint(new Vector3(startPoint.x, startPoint.y, 0)));
                meshLines.SetVertices(verticesLines);
                int[] indices = new int[verticesLines.Count];
                Color[] colors = new Color[verticesLines.Count];
                for (int i = 0; i < verticesLines.Count; ++i)
                {
                    indices[i] = i;
                    colors[i] = Color.gray;
                }
                meshLines.SetIndices(indices, MeshTopology.LineStrip, 0);
                meshLines.SetColors(colors);
                Graphics.DrawMesh(meshLines, new Vector3(0, 0, Z_POS), Quaternion.identity, lineMaterial, 5);

                List<Vector3> verticesQuads = new List<Vector3>();
                verticesQuads.Add(currentCamera.ScreenToWorldPoint(new Vector3(startPoint.x, startPoint.y, 0)));
                verticesQuads.Add(currentCamera.ScreenToWorldPoint(new Vector3(startPoint.x, endPoint.y, 0)));
                verticesQuads.Add(currentCamera.ScreenToWorldPoint(new Vector3(endPoint.x, endPoint.y, 0)));
                verticesQuads.Add(currentCamera.ScreenToWorldPoint(new Vector3(endPoint.x, startPoint.y, 0)));
                meshQuad.SetVertices(verticesQuads);
                Graphics.DrawMesh(meshQuad, new Vector3(0, 0, Z_POS), Quaternion.identity, lineMaterial, 5);

                triggerUpdate = true;
            }

            if (selectedKnots.Count > 1) {
                if (triggerUpdate)
                {
                    triggerUpdate = false;
                    
                    List<Vector3> verticesQuads = new List<Vector3>();
                    verticesQuads.Add(currentCamera.ScreenToWorldPoint(new Vector3(leftPadBottom.x, leftPadBottom.y, 0)));
                    verticesQuads.Add(currentCamera.ScreenToWorldPoint(new Vector3(leftPadBottom.x, rightPadTop.y, 0)));
                    verticesQuads.Add(currentCamera.ScreenToWorldPoint(new Vector3(rightPadTop.x, rightPadTop.y, 0)));
                    verticesQuads.Add(currentCamera.ScreenToWorldPoint(new Vector3(rightPadTop.x, leftPadBottom.y, 0)));
                    meshQuadSelection.SetVertices(verticesQuads);

                    resizeLineLeft = leftPadBottom.x - HALF_PAD.x;
                    resizeLineRight = rightPadTop.x + HALF_PAD.x;
                    resizeLineTop = rightPadTop.y + HALF_PAD.y;
                    resizeLineBottom = leftPadBottom.y - HALF_PAD.x;
                    List<Vector3> verticesLinesSelectionResize = new List<Vector3>();
                    verticesLinesSelectionResize.Add(currentCamera.ScreenToWorldPoint(new Vector3(leftPadBottom.x + HALF_PAD.x, resizeLineBottom, 0)));
                    verticesLinesSelectionResize.Add(currentCamera.ScreenToWorldPoint(new Vector3(rightPadTop.x - HALF_PAD.x, resizeLineBottom, 0)));
                    verticesLinesSelectionResize.Add(currentCamera.ScreenToWorldPoint(new Vector3(leftPadBottom.x + HALF_PAD.x, resizeLineTop, 0)));
                    verticesLinesSelectionResize.Add(currentCamera.ScreenToWorldPoint(new Vector3(rightPadTop.x - HALF_PAD.x, resizeLineTop, 0)));
                    verticesLinesSelectionResize.Add(currentCamera.ScreenToWorldPoint(new Vector3(resizeLineLeft, leftPadBottom.y + HALF_PAD.y, 0)));
                    verticesLinesSelectionResize.Add(currentCamera.ScreenToWorldPoint(new Vector3(resizeLineLeft, rightPadTop.y - HALF_PAD.y, 0)));
                    verticesLinesSelectionResize.Add(currentCamera.ScreenToWorldPoint(new Vector3(resizeLineRight, leftPadBottom.y + HALF_PAD.y, 0)));
                    verticesLinesSelectionResize.Add(currentCamera.ScreenToWorldPoint(new Vector3(resizeLineRight, rightPadTop.y - HALF_PAD.y, 0)));
                    meshLinesSelectionResize.SetVertices(verticesLinesSelectionResize);
                }
                Graphics.DrawMesh(meshQuadSelection, new Vector3(0, 0, Z_POS), Quaternion.identity, lineMaterial, 5);
                Graphics.DrawMesh(meshLinesSelectionResize, new Vector3(0, 0, Z_POS), Quaternion.identity, lineMaterial, 5);
            }
        }

        public void StartMultipleKeySelection() {
            startPoint = Input.mousePosition;
            endPoint = startPoint;
            MultipleKeySelectionOn = true;
        }

        public bool MultipleKeysAreSelected() {
            return selectedKnots.Count > 1;
        }

        public List<int> SelectedKeyIndices() {
            return selectedKnots;
        }

        public void ClearMultipleKeySelection() {
            selectedKnots.Clear();
        }

        public bool InsideSelectedKeys() {
            Vector2 mousePos = Input.mousePosition;
            return Contains(mousePos, leftPadBottom, rightPadTop);
        }

        public ResizePart OnResizingLines()
        {
            Vector2 mousePos = Input.mousePosition;
            ResizePart resizePart = ResizePart.None;

            if (OnResizingLinesLeft(mousePos))
            {
                resizePart = ResizePart.Left;
            } else if (OnResizingLinesRight(mousePos)){
                resizePart = ResizePart.Right;
            } else if (OnResizingLinesTop(mousePos)){
                resizePart = ResizePart.Top;
            } else if (OnResizingLinesBottom(mousePos)){
                resizePart = ResizePart.Bottom;
            }
            
            return resizePart;
        }

        bool OnResizingLinesLeft(Vector2 mousePos)
        {            
            return (resizeLineLeft - QUARTER_PAD.x < mousePos.x) && (mousePos.x < resizeLineLeft + QUARTER_PAD.x) && (leftPadBottom.y + HALF_PAD.y < mousePos.y) && (mousePos.y < rightPadTop.y - HALF_PAD.y);
        }

        bool OnResizingLinesRight(Vector2 mousePos)
        {
            return (resizeLineRight - QUARTER_PAD.x < mousePos.x) && (mousePos.x < resizeLineRight + QUARTER_PAD.x) && (leftPadBottom.y + HALF_PAD.y < mousePos.y) && (mousePos.y < rightPadTop.y - HALF_PAD.y);
        }

        bool OnResizingLinesTop(Vector2 mousePos)
        {
            return (leftPadBottom.x + HALF_PAD.x < mousePos.x) && (mousePos.x < rightPadTop.x - HALF_PAD.x) && (resizeLineTop - QUARTER_PAD.y < mousePos.y) && (mousePos.y < resizeLineTop + QUARTER_PAD.y);
        }

        bool OnResizingLinesBottom(Vector2 mousePos)
        {
            return (leftPadBottom.x + HALF_PAD.x < mousePos.x) && (mousePos.x < rightPadTop.x - HALF_PAD.x) && (resizeLineBottom - QUARTER_PAD.y < mousePos.y) && (mousePos.y < resizeLineBottom + QUARTER_PAD.y);
        }

        bool Contains(Vector2 point, Vector2 startPoint, Vector2 endPoint) {
            return (((startPoint.x < endPoint.x) && (startPoint.x <= point.x) && (point.x <= endPoint.x)) || ((startPoint.x > endPoint.x) && (startPoint.x >= point.x) && (point.x >= endPoint.x))) &&
                 (((startPoint.y < endPoint.y) && (startPoint.y <= point.y) && (point.y <= endPoint.y)) || ((startPoint.y > endPoint.y) && (startPoint.y >= point.y) && (point.y >= endPoint.y)));
        }

        void PadRect() {
            if (selectedKnots.Count > 1) {
                leftPadBottom = leftBottom - PAD;
                rightPadTop = rightTop + PAD;
            }
        }

    }
}
