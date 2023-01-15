using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DevionGames
{
    public class BezierCurve : MonoBehaviour
    {
        [SerializeField]
        private float m_Scale = 1f;
        public float scale {
            get { return m_Scale; }
        }

        /// <summary>
        /// Points of the bezier curve
        /// </summary>
        [SerializeField]
        private BezierPoint[] points = new BezierPoint[0];

        /// <summary>
        /// Point at index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public BezierPoint this[int index]
        {
            get { return points[index]; }
        }

        /// <summary>
        /// Amount of points
        /// </summary>
        public int count
        {
            get { return points.Length; }
        }

        /*[SerializeField]
        private int m_Resolution = 300;
        public int resolution {
            get { return m_Resolution; }
        }*/

        /// <summary>
        /// Approximate length of the bezier curve based on resolution
        /// </summary>
        /*public float length
        {
            get
            {
                float m_Length = 0f;
                for (int i = 0; i < points.Length - 1; i++)
                {
                    m_Length += ApproximateLength(points[i], points[i + 1], this.m_Resolution);
                }
                return m_Length;
            }
        }*/

        /// <summary>
        /// Adds a new point at the end of curve
        /// </summary>
        /// <param name="point"></param>
        public void Add(BezierPoint point)
        {
            List<BezierPoint> tempArray = new List<BezierPoint>(points);
            tempArray.Add(point);
            points = tempArray.ToArray();
        }

        /// <summary>
        /// Adds a new point at the end of curve
        /// </summary>
        /// <param name="point"></param>
        public void Add(Vector3 position)
        {
            Insert(count, position);
        }

        /// <summary>
        /// Adds a new point at index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public BezierPoint Insert(int index, Vector3 position)
        {
            BezierPoint newPoint = new BezierPoint(this);
            newPoint.localPosition = position;
            newPoint.handle1 = position;
            newPoint.handle2 = position;

            List<BezierPoint> tempArray = new List<BezierPoint>(points);
            tempArray.Insert(index, newPoint);
            points = tempArray.ToArray();
            return newPoint;
        }

        /// <summary>
        /// Removes the given point from curve
        /// </summary>
        /// <param name="point"></param>
        public void Remove(BezierPoint point)
        {
            List<BezierPoint> tempArray = new List<BezierPoint>(points);
            tempArray.Remove(point);
            points = tempArray.ToArray();
        }

        public void Clear() {
            points = new BezierPoint[0];
        }

        public Vector3 GetPoint(float t, int resolution)
        {

            if (t <= 0f) return points[0].localPosition;
            else if (t >= 1f) return points[points.Length - 1].localPosition;

            float totalPercent = 0;
            float curvePercent = 0;

            BezierPoint p1 = null;
            BezierPoint p2 = null;
            float length = ApproximateLength(resolution);
            for (int i = 0; i < points.Length - 1; i++)
            {
                curvePercent = ApproximateLength(points[i], points[i + 1], 30) / length;
                if (totalPercent + curvePercent > t)
                {
                    p1 = points[i];
                    p2 = points[i + 1];
                    break;
                }

                else totalPercent += curvePercent;
            }

            t -= totalPercent;

            return GetPoint(p1, p2, t / curvePercent);
        }

        public Vector3 GetPointAtDistance(float distance, int resolution)
        {

            Vector3[] verts = GetVertices(resolution);
            float current = 0f;
            for (int i = 0; i < verts.Length - 1; i++)
            {
                Vector3 p1 = verts[i];
                Vector3 p2 = verts[i + 1];
                current += Vector3.Distance(p1, p2);
                if (current >= distance)
                {
                    return p1;
                }
            }

            return Vector3.zero;
        }

        private Vector3[] GetVertices(int resolution)
        {
            int limit = resolution + 1;
            float _res = resolution;
            List<Vector3> vertices = new List<Vector3>();
            if (count > 1)
            {
                for (int index = 0; index < count - 1; index++)
                {
                    BezierPoint p1 = this[index];
                    BezierPoint p2 = this[index + 1];

                    for (int i = 1; i < limit; i++)
                    {
                        vertices.Add(BezierCurve.GetPoint(p1, p2, i / _res));
                    }
                }
            }
            return vertices.ToArray();
        }

        public float ApproximateLength(int resolution) {
            float m_Length = 0f;
            for (int i = 0; i < points.Length - 1; i++)
            {
                m_Length += ApproximateLength(points[i], points[i + 1], resolution);
            }
            return m_Length;
        }

        /// <summary>
        /// Approximate length between two points
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="resolution"></param>
        /// <returns></returns>
        public static float ApproximateLength(BezierPoint p1, BezierPoint p2, int resolution = 30)
        {
            float _res = resolution;
            float total = 0;
            Vector3 lastPosition = p1.localPosition;
            Vector3 currentPosition;

            for (int i = 0; i < resolution + 1; i++)
            {
                currentPosition = GetPoint(p1, p2, i / _res);
                total += (currentPosition - lastPosition).magnitude;
                lastPosition = currentPosition;
            }

            return total;
        }

        

        public static Vector3 GetPoint(BezierPoint p1, BezierPoint p2, float t)
        {
            if (p1.handle2 != Vector3.zero)
            {
                if (p2.handle1 != Vector3.zero)
                {
                    return GetCubicCurvePoint(p1.localPosition, p1.handle2, p2.handle1, p2.localPosition, t);
                }
                else
                {
                    return GetQuadraticCurvePoint(p1.localPosition, p1.handle2, p2.localPosition, t);
                }
            }
            else
            {
                if (p2.handle1 != Vector3.zero)
                {
                    return GetQuadraticCurvePoint(p1.localPosition, p2.handle1, p2.localPosition, t);
                }
                else
                {
                    return GetLinearPoint(p1.localPosition, p2.localPosition, t);
                }
            }
        }

        public static Vector3 GetCubicCurvePoint(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, float t)
        {
            t = Mathf.Clamp01(t);

            Vector3 part1 = Mathf.Pow(1 - t, 3) * p1;
            Vector3 part2 = 3 * Mathf.Pow(1 - t, 2) * t * p2;
            Vector3 part3 = 3 * (1 - t) * Mathf.Pow(t, 2) * p3;
            Vector3 part4 = Mathf.Pow(t, 3) * p4;

            return part1 + part2 + part3 + part4;
        }

        public static Vector3 GetQuadraticCurvePoint(Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            t = Mathf.Clamp01(t);

            Vector3 part1 = Mathf.Pow(1 - t, 2) * p1;
            Vector3 part2 = 2 * (1 - t) * t * p2;
            Vector3 part3 = Mathf.Pow(t, 2) * p3;

            return part1 + part2 + part3;
        }

        public static Vector3 GetLinearPoint(Vector3 p1, Vector3 p2, float t)
        {
            return p1 + ((p2 - p1) * t);
        }
    }
}