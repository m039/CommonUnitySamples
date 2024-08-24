using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public struct BezierCubicCurve
    {
        public Vector3 p0;
        public Vector3 p1;
        public Vector3 p2;
        public Vector3 p3;

        public Vector3 startPosition => p0;

        public Vector3 endPostion => p3;

        public BezierCubicCurve(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            this.p0 = p0;
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
        }

        public Vector3[] GetSegments(int subdivisions)
        {
            var segments = new Vector3[subdivisions];
            for (int i = 0; i < subdivisions; i++)
            {
                var t = Mathf.InverseLerp(0, subdivisions, i);
                segments[i] = Bezier.GetCubic(p0, p1, p2, p3, t);
            }
            return segments;
        }
    }
}
