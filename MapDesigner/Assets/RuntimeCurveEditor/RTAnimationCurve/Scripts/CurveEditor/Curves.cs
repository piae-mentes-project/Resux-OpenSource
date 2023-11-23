using UnityEngine;
using System.Collections.Generic;

namespace RuntimeCurveEditor
{
    public struct Knot
    {
        public Vector2 point;
        public bool visible;

        public Knot(Vector2 point, bool visible) {
            this.point = point;
            this.visible = visible;
        }
    }

    public static class Curves
    {
        public static Camera camera;
        public static Material lineMaterial;

        public static Dictionary<AnimationCurve, List<ContextMenu>> dictCurvesContextMenus;

        public static List<Knot> activeCurveKnots = new List<Knot>();

        public static float margin;

        static Dictionary<AnimationCurve, Mesh> meshCurves = new Dictionary<AnimationCurve, Mesh>();
        static Dictionary<AnimationCurve, Mesh> meshCurvesKnots = new Dictionary<AnimationCurve, Mesh>();
        static Dictionary<AnimationCurve, bool> meshCurvesUpdate = new Dictionary<AnimationCurve, bool>();
        static Mesh meshTangents = new Mesh();
        static Dictionary<AnimationCurve, Mesh> meshCurvePaths = new Dictionary<AnimationCurve, Mesh>();
        static Dictionary<AnimationCurve, Mesh> meshBasicCurves = new Dictionary<AnimationCurve, Mesh>();
        static Dictionary<AnimationCurve, Mesh> meshBasicCurvesQuads = new Dictionary<AnimationCurve, Mesh>();
        static Dictionary<AnimationCurve, bool> meshBasicCurvesUpdate = new Dictionary<AnimationCurve, bool>();

        static Color LIGHT_GRAY = new Color(0.85f, 0.85f, 0.85f);

        public const float Z_DIFF = 0.01f;
        const float Z_POS = -Z_DIFF;
        const float Z_POS_TANG = Z_POS - Z_DIFF;
        public const float Z_POS_KNOTS = Z_POS_TANG - Z_DIFF;
        const float Z_POS_PATH = Z_POS * 0.5f;
        
        public static Vector2 SampleBezier(float t, Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4) {
            return (1 - t) * (1 - t) * (1 - t) * p1 + 3.0F * (1 - t) * (1 - t) * t * p2 + 3.0F * (1 - t) * t * t * p3 + t * t * t * p4;
        }

        public static void AddBezier(List<Vector3> vertices, AnimationCurve curve, Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, Rect clipRect, float clip = 1.0f) {
            Vector2 v1m = Vector2.zero;
            Vector2 v2m = Vector2.zero;
            int samples = (int)(0.5f * (p4.x - p1.x));
            float invSamples = 1f / samples;
            float t = 0;
            v1m = SampleBezier(t, p1, p2, p3, p4);
            bool on = true;

            List<Vector3> bezierVertices = new List<Vector3>();
            do {
                t += invSamples;
                if (t > clip) {
                    on = false;
                    t = clip;
                }
                v2m = SampleBezier(t, p1, p2, p3, p4);
                if (clipRect.Contains(v1m) && clipRect.Contains(v2m)) {
                    if ((bezierVertices.Count == 0) && (vertices.Count == 0)) {
                        bezierVertices.Add(camera.ScreenToWorldPoint(new Vector3(v1m.x, v1m.y, 0)));
                    }
                    bezierVertices.Add(camera.ScreenToWorldPoint(new Vector3(v2m.x, v2m.y, 0)));
                }
                v1m = v2m;
            } while (on);
            vertices.AddRange(bezierVertices);
        }

        static void AddConstantLine(List<Vector3> vertices, Vector2 p1, Vector2 p2, Rect clipRect) {
            Vector2 p = p1;
            p.x = p2.x;
            Vector2 pp = p;
            if (Utils.CohenSutherlandLineClip(clipRect, ref p1, ref p)) {
                AddConstantLine(vertices, p1, p, true);
            }
            if (Utils.CohenSutherlandLineClip(clipRect, ref p2, ref pp)) {
                AddConstantLine(vertices, p2, pp, true);
            }
        }

        static void AddConstantLine(List<Vector3> vertices, Vector2 p1, Vector2 p2, bool oneLine = false) {
            if (oneLine || (p1.y == p2.y)) {
                if (vertices.Count == 0)
                {
                    vertices.Add(camera.ScreenToWorldPoint(new Vector3(p1.x, p1.y, 0)));
                }
                vertices.Add(camera.ScreenToWorldPoint(new Vector3(p2.x, p2.y, 0)));
            } else {
                if (vertices.Count == 0)
                {
                    vertices.Add(camera.ScreenToWorldPoint(new Vector3(p1.x, p1.y, 0)));
                }
                vertices.Add(camera.ScreenToWorldPoint(new Vector3(p2.x, p1.y, 0)));
                vertices.Add(camera.ScreenToWorldPoint(new Vector3(p2.x, p2.y, 0)));
            }
        }

        static Vector2 GetTangLength(Vector2 p1, Vector2 p2) {
            Vector2 tangLength = Vector2.zero;
            tangLength.x = Mathf.Abs(p1.x - p2.x) * 0.333333f;
            tangLength.y = tangLength.x;
            return tangLength;
        }

        public static void GetControlPoints(Vector2 p1, Vector2 p2, float tangOut, float tangIn, out Vector2 c1, out Vector2 c2) {
            Vector2 tangLength = GetTangLength(p1, p2);
            c1 = p1;
            c2 = p2;
            c1.x += tangLength.x;
            c1.y += tangLength.y * tangOut;
            c2.x -= tangLength.x;
            c2.y -= tangLength.y * tangIn;
        }

        public static void GetTangents(Vector2 p1, Vector2 p2, Vector2 c1, Vector2 c2, out float tangOut, out float tangIn) {
            Vector2 tangLength = GetTangLength(p1, p2);
            tangOut = (c1.y - p1.y) / tangLength.y;
            tangIn = (c2.y - p2.y) / tangLength.y;
        }

        public static void TriggerUpdateCurves()
        {
            List<AnimationCurve> curves = new List<AnimationCurve>(meshCurvesUpdate.Keys);
            foreach (AnimationCurve curve in curves)
            {
                meshCurvesUpdate[curve] = true;
            }
        }

        public static void TriggerUpdateCurve(AnimationCurve curve)
        {
            if (curve != null) {
                meshCurvesUpdate[curve] = true;
            }
        }

        public static void TriggerUpdateBasicCurves()
        {
            List<AnimationCurve> curves = new List<AnimationCurve>(meshBasicCurvesUpdate.Keys);
            foreach (AnimationCurve curve in curves)
            {
                meshBasicCurvesUpdate[curve] = true;
            }
        }

        public static bool BasicCurvesUpdate(AnimationCurve curve)
        {
            return (meshBasicCurvesUpdate.Count == 0) || meshBasicCurvesUpdate[curve];
        }

        static void AddLine(List<Vector3> vertices, float x1, float y1, float x2, float y2) {
            if (vertices.Count == 0)
            {
                vertices.Add(camera.ScreenToWorldPoint(new Vector3(x1, y1, 0)));
            }
            vertices.Add(camera.ScreenToWorldPoint(new Vector3(x2, y2, 0)));
        }

        static void UpdateCurve(Color color, AnimationCurve curve, bool activeCurve, int selectedKey, Rect entireGridRect, Rect gridClipRect, Rect gradRect, bool isIcon = false, float clip = 1.0f) {
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> verticesKnots = new List<Vector3>();
            List<Color> colorsKnots = new List<Color>();
            List<Vector3> verticesTangents = new List<Vector3>();

            float ratio = entireGridRect.height * gradRect.width / (entireGridRect.width * gradRect.height);

            if (activeCurve)
            {
                activeCurveKnots.Clear();
            }

            for (int i = 0; i < curve.length; ++i)
            {
                Vector2 knot = new Vector2(curve[i].time, curve[i].value);
                knot = Utils.Convert(knot, entireGridRect, gradRect);
                bool knotIn = !isIcon && gridClipRect.Contains(knot);

                //outside of the interval, just draw straigt lines outside from the 1st and last key respectively
                if (i == 0)
                {
                    if (knotIn)
                    {
                        AddLine(vertices, gridClipRect.xMin, knot.y, knot.x, knot.y);
                    }
                    else if (!isIcon && (gridClipRect.yMin <= knot.y) && (knot.y <= gridClipRect.yMax) && (gridClipRect.xMax <= knot.x))
                    {
                        AddLine(vertices, gridClipRect.xMin, knot.y, gridClipRect.xMax, knot.y);
                    }
                }

                if (i == curve.length - 1)
                {
                    if (knotIn)
                    {
                        AddLine(vertices, knot.x, knot.y, gridClipRect.xMax, knot.y);
                    }
                    else if (!isIcon && (gridClipRect.yMin <= knot.y) && (knot.y <= gridClipRect.yMax) && (knot.x <= gridClipRect.xMin))
                    {
                        AddLine(vertices, gridClipRect.xMin, knot.y, gridClipRect.xMax, knot.y);
                    }
                }

                if (isIcon && (curve.length == 1))
                {
                    AddLine(vertices, gridClipRect.xMin, knot.y, gridClipRect.xMax, knot.y);
                }

                if (curve.length > i + 1)
                {//draw bezier between consecutive keys
                    Vector2 knot2 = new Vector2(curve[i + 1].time, curve[i + 1].value);
                    knot2 = Utils.Convert(knot2, entireGridRect, gradRect);
                    bool knot2In = gridClipRect.Contains(knot2);

                    Vector2 c1 = Vector2.zero;
                    Vector2 c2 = Vector2.zero;
                    float tangOut = curve[i].outTangent;
                    float tangIn = curve[i + 1].inTangent;

                    //TODO it would be nice to have all these tang scaled values calculated only when needed(when ratio or tangs of the point changes)
                    float tangOutScaled = Mathf.Atan(tangOut * ratio);
                    float tangInScaled = Mathf.Atan(tangIn * ratio);

                    if ((tangOut != float.PositiveInfinity) && (tangIn != float.PositiveInfinity))
                    {
                        GetControlPoints(knot, knot2, tangOut * ratio, tangIn * ratio, out c1, out c2);
                        AddBezier(vertices, curve, knot, c1, c2, knot2, gridClipRect, clip);
                    }
                    else
                    {
                        if (knotIn && knot2In)
                        {
                            AddConstantLine(vertices, knot, knot2);
                        }
                        else
                        {
                            AddConstantLine(vertices, knot, knot2, gridClipRect);
                        }
                    }

                    if (activeCurve)
                    {

                        if (knotIn && (selectedKey == i))
                        {
                            ContextMenu contextMenu = dictCurvesContextMenus[curve][selectedKey];
                            if (!contextMenu.broken || contextMenu.rightTangent.free)
                            {
                                Vector2 tangPeak = new Vector2(knot.x + CurveLines.tangFloat * Mathf.Cos(tangOutScaled), knot.y + CurveLines.tangFloat * Mathf.Sin(tangOutScaled));

                                verticesTangents.Add(camera.ScreenToWorldPoint(new Vector3(knot.x, knot.y, 0)));
                                verticesTangents.Add(camera.ScreenToWorldPoint(new Vector3(tangPeak.x, tangPeak.y, 0)));

                                AddQuad(verticesKnots, colorsKnots, Color.gray, tangPeak, margin);
                            }
                        }

                        if (knot2In && (selectedKey == i + 1))
                        {
                            ContextMenu contextMenu = dictCurvesContextMenus[curve][selectedKey];
                            if (!contextMenu.broken || contextMenu.leftTangent.free)
                            {
                                Vector2 tangPeak = new Vector2(knot2.x - CurveLines.tangFloat * Mathf.Cos(tangInScaled), knot2.y - CurveLines.tangFloat * Mathf.Sin(tangInScaled));

                                verticesTangents.Add(camera.ScreenToWorldPoint(new Vector3(knot2.x, knot2.y, 0)));
                                verticesTangents.Add(camera.ScreenToWorldPoint(new Vector3(tangPeak.x, tangPeak.y, 0)));

                                AddQuad(verticesKnots, colorsKnots, Color.gray, tangPeak, margin);
                            }
                        }
                    }
                }

                if (activeCurve)
                {
                    activeCurveKnots.Add(new Knot(knot, knotIn));
                }

                if (knotIn)
                {
                    if (activeCurve)
                    {
                        if (selectedKey == i)
                        {
                            AddQuad(verticesKnots, colorsKnots, LIGHT_GRAY, knot, 1.33333f * margin);
                        }
                    }
                    if (!isIcon)
                    {
                        AddQuad(verticesKnots, colorsKnots, color, knot, margin);
                    }
                }
            }

            Mesh meshCurve = (isIcon ? meshBasicCurves : meshCurves)[curve];
            //setup curve mesh
            meshCurve.Clear();
            meshCurve.SetVertices(vertices);
            Color[] colors = new Color[vertices.Count];
            for (int i = 0; i < vertices.Count; ++i)
            {
                colors[i] = color;
            }
            meshCurve.SetColors(colors);
            int[] indices = new int[vertices.Count];
            for (int i = 0; i < vertices.Count; ++i)
            {
                indices[i] = i;
            }
            meshCurve.SetIndices(indices, MeshTopology.LineStrip, 0);

            if (!isIcon) {                
                Mesh meshCurveKnots = meshCurvesKnots[curve];

                //setup curve knots
                meshCurveKnots.Clear();
                meshCurveKnots.SetVertices(verticesKnots);
                meshCurveKnots.SetColors(colorsKnots);
                int[] indicesKnots = new int[verticesKnots.Count];
                for (int i = 0; i < verticesKnots.Count; ++i)
                {
                    indicesKnots[i] = i;
                }
                meshCurveKnots.SetIndices(indicesKnots, MeshTopology.Quads, 0);

                //setup tangent
                meshTangents.Clear();
                if (verticesTangents.Count > 0)
                {
                    int[] indicesTangents = new int[verticesTangents.Count];
                    Color[] colorTangents = new Color[verticesTangents.Count];
                    for (int i = 0; i < verticesTangents.Count; ++i)
                    {
                        indicesTangents[i] = i;
                        colorTangents[i] = Color.gray;
                    }

                    meshTangents.SetVertices(verticesTangents);
                    meshTangents.SetColors(colorTangents);
                    meshTangents.SetIndices(indicesTangents, MeshTopology.Lines, 0);
                }
            }
        }

        static void DrawCurve(Color color, AnimationCurve curve, bool activeCurve, int selectedKey, Rect entireGridRect, Rect gridClipRect, Rect gradRect, bool isIcon = false, float clip = 1.0f) {           
            if (isIcon) {//basic shape
                if (!meshBasicCurves.ContainsKey(curve)){
                    meshBasicCurves.Add(curve, new Mesh());
                    meshBasicCurvesUpdate.Add(curve, true);
                }

                if (meshBasicCurvesUpdate[curve])
                {
                    meshBasicCurvesUpdate[curve] = false;
                    UpdateCurve(color, curve, activeCurve, selectedKey, entireGridRect, gridClipRect, gradRect, true, clip);
                }

                Graphics.DrawMesh(meshBasicCurves[curve], new Vector3(0, 0, Z_POS), Quaternion.identity, lineMaterial, 5);
            } else {
                if (!meshCurves.ContainsKey(curve))
                {
                    meshCurves.Add(curve, new Mesh());
                    meshCurvesKnots.Add(curve, new Mesh());
                    meshCurvesUpdate.Add(curve, true);
                }
                Mesh meshCurve = meshCurves[curve];
                Mesh meshCurveKnots = meshCurvesKnots[curve];

                if (meshCurvesUpdate[curve])
                {
                    meshCurvesUpdate[curve] = false;
                    UpdateCurve(color, curve, activeCurve, selectedKey, entireGridRect, gridClipRect, gradRect);
                }

                Graphics.DrawMesh(meshCurve, new Vector3(0, 0, Z_POS), Quaternion.identity, lineMaterial, 5);
                Graphics.DrawMesh(meshCurveKnots, new Vector3(0, 0, Z_POS_KNOTS), Quaternion.identity, lineMaterial, 5);
                Graphics.DrawMesh(meshTangents, new Vector3(0, 0, Z_POS_TANG), Quaternion.identity, lineMaterial, 5);
            }
        }

        public static void DrawCurveForm(Color color, AnimationCurve curve1, AnimationCurve curve2, bool activeCurve1, bool activeCurve2, int selectedKey, Rect entireGridRect, Rect gridRect, Rect gradRect, bool isIcon = false, float clip = 1.0f) {
            if (curve2 != null) {
                if (!meshCurvePaths.ContainsKey(curve1))
                {
                    meshCurvePaths.Add(curve1, new Mesh());
                }
                Mesh meshPath = meshCurvePaths[curve1];

                if (!meshCurvesUpdate.ContainsKey(curve1) || meshCurvesUpdate[curve1] || meshCurvesUpdate[curve2]) {
                    int samples = (int)entireGridRect.width;
                    Color colorTransp = color;
                    colorTransp.a *= 0.35f;
                    Vector2 v1 = Vector2.zero;
                    Vector2 v2 = Vector2.zero;
                    Vector2 v1prev;
                    Vector2 v2prev;
                    float invSamples = 1f / samples;
                    float t = 0;
                    bool lineIn = GetValues(out v1, out v2, curve1, curve2, entireGridRect, gridRect, gradRect, t);
                    bool prevLineIn;

                    List<Vector3> vertices = new List<Vector3>();
                    for (int i = 0; i <= samples; ++i) {
                        v1prev = v1;
                        v2prev = v2;
                        prevLineIn = lineIn;
                        lineIn = GetValues(out v1, out v2, curve1, curve2, entireGridRect, gridRect, gradRect, t);
                        if (prevLineIn && lineIn) {
                            vertices.Add(camera.ScreenToWorldPoint(new Vector3(v1prev.x, v1prev.y, 0)));
                            vertices.Add(camera.ScreenToWorldPoint(new Vector3(v2prev.x, v2prev.y, 0)));
                            vertices.Add(camera.ScreenToWorldPoint(new Vector3(v2.x, v2.y, 0)));
                            vertices.Add(camera.ScreenToWorldPoint(new Vector3(v1.x, v1.y, 0)));
                        }
                        t += invSamples;
                    }
                    int[] indices = new int[vertices.Count];
                    Color[] colors = new Color[vertices.Count];
                    for (int i = 0; i < vertices.Count; ++i)
                    {
                        indices[i] = i;
                        colors[i] = colorTransp;
                    }
                    meshPath.Clear();
                    meshPath.SetVertices(vertices);
                    meshPath.SetColors(colors);
                    meshPath.SetIndices(indices, MeshTopology.Quads, 0);
                }

                Graphics.DrawMesh(meshPath, new Vector3(0, 0, Z_POS_PATH), Quaternion.identity, lineMaterial, 5);
                DrawCurve(color, curve2, activeCurve2, selectedKey, entireGridRect, gridRect, gradRect, isIcon, clip);
            }
            DrawCurve(color, curve1, activeCurve1, selectedKey, entireGridRect, gridRect, gradRect, isIcon, clip);
        }

        static bool GetValues(out Vector2 v1, out Vector2 v2, AnimationCurve curve1, AnimationCurve curve2, Rect entireGridRect, Rect clipRect, Rect gradRect, float t) {
            v1.x = gradRect.xMin + t * (gradRect.xMax - gradRect.xMin);
            v2.x = v1.x;
            v1.y = curve1.Evaluate(v1.x);
            v1 = Utils.Convert(v1, entireGridRect, gradRect);
            v2.y = curve2.Evaluate(v2.x);
            v2 = Utils.Convert(v2, entireGridRect, gradRect);
            return Utils.CohenSutherlandLineClip(clipRect, ref v1, ref v2);
        }

        static void AddQuad(List<Vector3> vertices, List<Color> colors, Color color, Vector2 pos, float m) {
            for (int i = 0; i < 4; ++i)
            {
                colors.Add(color);                
            }
            vertices.AddRange(new Vector3[] {camera.ScreenToWorldPoint(new Vector3(pos.x, pos.y - m, 0)),
                                            camera.ScreenToWorldPoint(new Vector3(pos.x + m, pos.y, 0)),
                                            camera.ScreenToWorldPoint(new Vector3(pos.x, pos.y + m, 0)),
                                            camera.ScreenToWorldPoint(new Vector3(pos.x - m, pos.y, 0))});
        }
    }
}
