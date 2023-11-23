using UnityEngine;

namespace RuntimeCurveEditor
{
    public static class Utils
    {

        //use below constants for clipping
        const byte INSIDE = 0; // 0000
        const byte LEFT = 1;   // 0001
        const byte RIGHT = 2;  // 0010
        const byte BOTTOM = 4; // 0100
        const byte TOP = 8;    // 1000

        public static Vector2 closestPoint;//closest point to the line from the mouse selection
        public static float closestPointTValue;//the t value corsesponding to the closest point on the bezier curve ,and it will be used if the user proceed with
                                               //the adding of a new point

        /// <summary>
        /// Convert from 2nd rect coordinates to the 1st rect coordinates.
        /// </param>/
        public static Vector2 Convert(Vector2 val, Rect rect1, Rect rect2) {
            Vector2 convVal = Vector2.zero;
            convVal.x = Mathf.Lerp(rect1.xMin, rect1.xMax, Mathf.InverseLerp(rect2.xMin, rect2.xMax, val.x));
            convVal.y = Mathf.Lerp(rect1.yMin, rect1.yMax, Mathf.InverseLerp(rect2.yMin, rect2.yMax, val.y));
            return convVal;
        }


        /// <summary>
        /// Distance from one point to a given animation curve (in the given gradiations ranges).
        /// </summary>
        /// <returns>
        /// The square of the distance.
        /// </returns>
        public static float PointLineSqrDist(Vector2 point, AnimationCurve animCurve, Rect gridRect, Rect curveGradRect, ContextMenuManager contextMenuManager) {
            float sqrDist = Mathf.Infinity;
            Vector2 keyframeWorld = Convert(new Vector2(animCurve[0].time, animCurve[0].value), gridRect, curveGradRect);
            if (keyframeWorld.x > point.x) {
                closestPoint = point;
                float dist = Mathf.Abs(keyframeWorld.y - point.y);
                sqrDist = dist * dist;
            } else {
                keyframeWorld = Convert(new Vector2(animCurve[animCurve.length - 1].time, animCurve[animCurve.length - 1].value), gridRect, curveGradRect);
                if (keyframeWorld.x < point.x) {
                    closestPoint = point;
                    float dist = Mathf.Abs(keyframeWorld.y - point.y);
                    sqrDist = dist * dist;
                } else if (animCurve.length > 1) {
                    for (int i = 0; i < animCurve.length - 1; ++i) {
                        keyframeWorld = Convert(new Vector2(animCurve[i + 1].time, animCurve[i + 1].value), gridRect, curveGradRect);
                        if (point.x < keyframeWorld.x) {
                            Vector2 p1 = new Vector2(animCurve[i].time, animCurve[i].value);
                            p1 = Convert(p1, gridRect, curveGradRect);
                            Vector2 p2 = keyframeWorld;
                            if (contextMenuManager.dictCurvesContextMenus[animCurve][i].rightTangent.constant ||
                               contextMenuManager.dictCurvesContextMenus[animCurve][i + 1].leftTangent.constant ||
                               animCurve[i].outTangent == float.PositiveInfinity || animCurve[i + 1].inTangent == float.PositiveInfinity) {
                                float dist = Mathf.Abs(p1.y - point.y);
                                if ((p1.y < p2.y && p1.y <= point.y && point.y <= p2.y) ||
                                    (p1.y > p2.y && p1.y >= point.y && point.y >= p2.y)) {
                                    float distX = Mathf.Abs(p2.x - point.x);
                                    if (distX < dist) dist = distX;
                                }
                                closestPoint = point;
                                sqrDist = dist * dist;
                                break;
                            } else {
                                Vector2 c1 = Vector2.zero;
                                Vector2 c2 = Vector2.zero;
                                float ratio = gridRect.height * curveGradRect.width / (gridRect.width * curveGradRect.height);
                                Curves.GetControlPoints(p1, p2, animCurve[i].outTangent * ratio, animCurve[i + 1].inTangent * ratio, out c1, out c2);
                                PointBezier.SqrDistPointToBezier(Normalize(point, gridRect),
                                                                Normalize(p1, gridRect),
                                                                Normalize(c1, gridRect),
                                                                Normalize(c2, gridRect),
                                                                Normalize(p2, gridRect),
                                                                out closestPoint, out closestPointTValue);
                                closestPoint = DeNormalize(closestPoint, gridRect);
                                sqrDist = (point - closestPoint).sqrMagnitude;
                                break;
                            }
                        }
                    }
                }
            }
            return sqrDist;
        }

        static Vector2 Normalize(Vector2 point, Rect rect) {//normalize to [0, 1], but do not clamp (so values like -0.5f or 2.0 are accepted)
            return new Vector2((point.x - rect.xMin) / rect.width, (point.y - rect.yMin) / rect.height);
        }

        static Vector2 DeNormalize(Vector2 point, Rect rect) {
            return new Vector2(point.x * rect.width + rect.xMin, point.y * rect.height + rect.yMin);
        }

        static byte ComputeOutcode(Rect clipRect, float x, float y) {
            byte code = INSIDE;
            if (x < clipRect.xMin) {
                code |= LEFT;
            } else if (x > clipRect.xMax) {
                code |= RIGHT;
            }
            if (y < clipRect.yMin) {
                code |= BOTTOM;
            } else if (y > clipRect.yMax) {
                code |= TOP;
            }
            return code;
        }

        public static bool CohenSutherlandLineClip(Rect clipRect, ref Vector2 p1, ref Vector2 p2) {
            byte outcode1 = ComputeOutcode(clipRect, p1.x, p1.y);
            byte outcode2 = ComputeOutcode(clipRect, p2.x, p2.y);
            bool lineDrawn = false;
            while (true) {
                if ((outcode1 | outcode2) == 0) {
                    lineDrawn = true;
                    break;
                } else if ((outcode1 & outcode2) != 0) {
                    break;
                } else {
                    float x = float.NaN;
                    float y = float.NaN;
                    float slope = (p2.y - p1.y) / (p2.x - p1.x);
                    byte outcodeOut = (outcode1 != 0) ? outcode1 : outcode2;
                    bool top = (outcodeOut & TOP) != 0;
                    bool bottom = (outcodeOut & BOTTOM) != 0;
                    if (top || bottom) {
                        y = top ? clipRect.yMax : clipRect.yMin;
                        x = p1.x + (y - p1.y) / slope;
                    } else {
                        bool right = (outcodeOut & RIGHT) != 0;
                        bool left = (outcodeOut & LEFT) != 0;
                        if (right || left) {
                            x = right ? clipRect.xMax : clipRect.xMin;
                            y = p1.y + (x - p1.x) * slope;
                        }
                    }
                    if (outcodeOut == outcode1) {
                        p1.x = x;
                        p1.y = y;
                        outcode1 = ComputeOutcode(clipRect, x, y);
                    } else {
                        p2.x = x;
                        p2.y = y;
                        outcode2 = ComputeOutcode(clipRect, x, y);
                    }
                }
            }
            return lineDrawn;
        }

    }
}
