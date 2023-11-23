using UnityEngine;

namespace RuntimeCurveEditor
{
    /// <summary>
    /// keeps the calculation related to point to bezier distance.
    /// </summary>
    static class PointBezier
    {

        /// <summary>
        /// Simulates the multiplication between two complex numbers(each number being represented by a Vector2).
        /// </summary>
        /// <returns>
        /// The result.
        /// </returns>
        static Vector2 ComplexMultiplier(Vector2 complex1, Vector2 complex2) {
            return new Vector2(complex1.x * complex2.x - complex1.y * complex2.y, complex1.y * complex2.x + complex1.x * complex2.y);
        }

        /// <summary>
        /// Sqr distance(as it is used for comparison) of a point to a bezier curve(given by the two end points and the two control points). 
        /// </summary>
        /// <returns>
        /// Return the square of the distance, and also the closest point on bezier, and the t value coresponding to that closest point.
        /// </returns>	
        public static float SqrDistPointToBezier(Vector2 point, Vector2 p1, Vector2 c1, Vector2 c2, Vector2 p2, out Vector2 closestPoint, out float closestPointTValue) {
            //we're going to project the point on the bezier curve, so to know how close it is 
            closestPoint = Vector2.zero;
            closestPointTValue = 0;

            Vector2 p1Norm = p1;
            Vector2 c1Norm = c1;
            Vector2 c2Norm = c2;
            Vector2 p2Norm = p2;

            //move the whole bezier curve so that the mouse location si translated to the origin
            p1Norm -= point;
            c1Norm -= point;
            c2Norm -= point;
            p2Norm -= point;

            //here are the factors of B(t)
            Vector2 A = -p1Norm + 3.0F * c1Norm - 3.0F * c2Norm + p2Norm;
            Vector2 B = 3.0F * p1Norm - 6.0F * c1Norm + 3.0F * c2Norm;
            Vector2 C = -3.0F * p1Norm + 3.0F * c1Norm;
            Vector2 D = p1Norm;

            //calculates the factor of B(t)*B'(t)..this is a 5th degree polynom
            const int max_degree = 5;
            int degree = max_degree;
            //finds the real degree
            float[] Q = new float[max_degree + 1];
            Q[0] = 3.0F * Vector2.Dot(A, A);
            Q[1] = 5.0F * Vector2.Dot(A, B);
            Q[2] = 4.0F * Vector2.Dot(A, C) + 2.0F * Vector2.Dot(B, B);
            Q[3] = 3.0F * Vector2.Dot(B, C) + 3.0F * Vector2.Dot(A, D);
            Q[4] = Vector2.Dot(C, C) + 2.0F * Vector2.Dot(B, D);
            Q[5] = Vector2.Dot(C, D);

            const float error = 0.00001f;//accepted error
            for (int j = 0; j < max_degree; ++j) {
                //we should consider nil, the very small values
                if (System.Math.Abs(Q[j]) < error) {
                    degree -= 1;
                } else {
                    for (int k = j + 1; k < max_degree + 1; ++k) {
                        Q[k] /= Q[j];
                    }
                    break;
                }
            }

            //get now the 't' value which is real and between 0.0F and 1.0F(if more usefull 't' values, calculate for each one the coresponding point, so to find wich is the closest)
            //use Durand-Kerner method 		
            Vector2 complex = new Vector2(0.4f, 0.9f);//randomly chosen value
            Vector2[] t = new Vector2[degree];

            for (int j = 0; j < degree; ++j) {
                t[j] = pow(complex, j);
            }

            Vector2[] t_prev = new Vector2[degree];
            const int MAX_ITERS = 15;
            int count = 0;
            bool condition = false;
            do {
                for (int j = 0; j < degree; ++j) {
                    t_prev[j] = t[j];
                    Vector2 denom = Vector2.right;
                    for (int k = 0; k < degree; ++k) {
                        if (j != k) {
                            denom = ComplexMultiplier(denom, t[j] - t[k]);
                        }
                    }
                    Vector2 revDenom = new Vector2(denom.x, -denom.y) / denom.sqrMagnitude;
                    Vector2 fValue = f(t[j], Q, max_degree - degree);
                    t[j] = t[j] - ComplexMultiplier(fValue, revDenom);
                }

                count += 1;
                for (int j = 0; j < degree; ++j) {
                    condition = condition || Mathf.Abs(t[j].y - t_prev[j].y) >= error || Mathf.Abs(t[j].x - t_prev[j].x) >= error;
                }
            } while ((count < MAX_ITERS) && condition);

            float sqrDistance = -1.0F;
            for (int j = 0; j < degree; ++j) {
                if ((Mathf.Abs(t[j].y) < error) && (t[j].x >= 0.0F) && (t[j].x <= 1.0F)) {
                    Vector2 samplePoint = Curves.SampleBezier(t[j].x, p1, c1, c2, p2);
                    float sqrDist = Vector2.SqrMagnitude(samplePoint - point);
                    if ((sqrDistance < 0.0F) || (sqrDist < sqrDistance)) {
                        sqrDistance = sqrDist;
                        //get the closest point that is on the curve (if any ..)
                        closestPoint = samplePoint;
                        closestPointTValue = t[j].x;
                    }
                }
            }
            if (sqrDistance < 0) {
                return Mathf.Infinity;
            }
            return sqrDistance;//!! this value is not to be used, as it's in the normalized space
        }

        /// <summary>
        /// Pow for complex value(complex value emulated with a Vector2).
        /// </param>
        static Vector2 pow(Vector2 val, int pow) {
            Vector2 result = Vector2.right;
            if (pow == 0) return result;
            for (int i = 0; i < pow; ++i) {
                result = ComplexMultiplier(result, val);
            }
            return result;
        }

        /// <summary>
        /// Helper method used in Durnar-Kerner algorithm.
        /// </summary>
        /// <param name="val">Complex value</param>
        /// <param name="Q">Polynom terms</param>
        /// <param name="offset">Difference between the initial degree of the polynom and the real degree.</param>
        /// <returns>A complex value</returns>
        static Vector2 f(Vector2 val, float[] Q, int offset) {
            int power = Q.Length - 1 - offset;
            Vector2 result = pow(val, power);
            int i;
            for (i = offset + 1; i < Q.Length - 1; ++i) {
                result += pow(val, Q.Length - i - 1) * Q[i];
            }
            result.x += Q[i];
            return result;
        }

    }
}
