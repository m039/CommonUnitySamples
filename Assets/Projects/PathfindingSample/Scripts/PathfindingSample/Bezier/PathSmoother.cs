using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class PathSmoother : MonoBehaviour
    {
        #region Inspector

        [SerializeField]
        float _SmoothingLength = 2f;

        [SerializeField]
        int _SmoothingSections = 10;

        #endregion

        private void OnEnable()
        {
            // noop
        }

        public IList<Vector3> GetSmoothPath(IList<Vector3> path)
        {
            if (!enabled)
                return path;

            if (path == null)
                return path;

            if (path.Count >= 3)
            {
                var length = path.Count - 1;
                var positions = new Vector3[length * _SmoothingSections];
                var curves = new BezierCubicCurve[length];

                Gizmos.color = Color.blue;

                for (int i = 0; i < length; i++)
                {
                    var position = path[i];
                    var lastPosition = i == 0 ? path[0] : path[i - 1];
                    var nextPosition = path[i + 1];
                    var lastDirection = (position - lastPosition).normalized;
                    var nextDirection = (nextPosition - position).normalized;
                    var starTangent = (lastDirection + nextDirection) * _SmoothingLength;
                    var endTangent = starTangent * -1;
                    curves[i] = new BezierCubicCurve(position, position + starTangent, nextPosition + endTangent, nextPosition);
                }

                {
                    var nextDirection = (curves[1].endPostion - curves[1].startPosition).normalized;
                    var lastDirection = (curves[0].endPostion - curves[0].startPosition).normalized;
                    curves[0].p2 = curves[0].p3 + (nextDirection + lastDirection) * -1 * _SmoothingLength;
                }

                for (int i = 0; i < length; i++)
                {
                    var segments = curves[i].GetSegments(_SmoothingSections);
                    Array.Copy(segments, 0, positions, i * _SmoothingSections, _SmoothingSections);
                }

                return positions;
            }

            return path;
        }
    }
}
