#if UNITY_EDITOR
#define SHOW_NEIGHBOURS
#endif

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.FlockingSample
{
    public class FlockingAgent : MonoBehaviour
    {
        #region Inspector

        public float speed = 1f;

        public float bodyRadius = 1f;

        public float rotationSpeed = 1f;

        #endregion

        [NonSerialized]
        internal int gridX = -1;

        [NonSerialized]
        internal int gridY = -1;

        public Vector2 up
        {
            get
            {
                return transform.up;
            }

            set
            {
                var p = transform.up;
                p.x = value.x;
                p.y = value.y;
                transform.up = p;
            }
        }

        public Vector2 position
        {
            get
            {
                return transform.position;
            }

            set
            {
                var p = transform.position;
                p.x = value.x;
                p.y = value.y;
                transform.position = p;
            }
        }

#if SHOW_NEIGHBOURS
        readonly List<FlockingAgent> _neighbours = new();
#endif

        Vector2 Separation(List<FlockingAgent> neighbours)
        {
            Vector2 point = Vector2.zero;
            var count = 0;

            foreach (var a in neighbours)
            {
                var v = a.position - position;
                if (v.magnitude < bodyRadius)
                {
                    point += -v.normalized;
                    count++;
                }
            }

            if (count == 0)
            {
                return point;
            }

            return (point / count).normalized;
        }

        Vector2 Alignment(List<FlockingAgent> neighbours)
        {
            if (neighbours.Count <= 0)
                return Vector2.zero;

            Vector2 direction = Vector2.zero;

            foreach (var a in neighbours)
            {
                direction += a.up;
            }

            return (direction / neighbours.Count).normalized;
        }

        Vector2 Cohesion(List<FlockingAgent> neighbours)
        {
            if (neighbours.Count <= 0)
                return Vector2.zero;

            var averagePosition = Vector2.zero;

            foreach (var a in neighbours)
            {
                averagePosition += a.position;
            }

            averagePosition /= neighbours.Count;

            return (averagePosition - position).normalized;
        }

        Vector2 ScreenBoundAvoidance(FlockingManager manager)
        {
            var screenRect = CameraUtils.ScreenRect;
            var direction = Vector2.zero;

            if (position.x < screenRect.xMin + manager.NeighbourRadius) {
                direction.x = 1;
            }

            if (position.x > screenRect.xMax - manager.NeighbourRadius)
            {
                direction.x = -1;
            }

            if (position.y < screenRect.yMin + manager.NeighbourRadius)
            {
                direction.y = 1;
            }

            if (position.y > screenRect.yMax - manager.NeighbourRadius)
            {
                direction.y = -1;
            }

            return direction.normalized;
        }

        public void Process(FlockingManager manager)
        {
            var neighbours = manager.GetNeighbours(this);
#if SHOW_NEIGHBOURS
            _neighbours.Clear();
            _neighbours.AddRange(neighbours);
#endif
            var separation = Separation(neighbours) * manager.separationCoeff;
            var alignment = Alignment(neighbours) * manager.alignmentCoeff;
            var cohesion = Cohesion(neighbours) * manager.cohesionCoeff;
            var screenBoundAvoidance = ScreenBoundAvoidance(manager) * manager.screenBoundAvoidanceCoeff;

            var direction = separation + cohesion + alignment + screenBoundAvoidance;

            up = Vector2.MoveTowards(
                up,
                direction.normalized,
                direction.magnitude * manager.rotationSpeedMultiplier * rotationSpeed * Time.deltaTime
            );

            var p = position + speed * Time.deltaTime * up * manager.movementSpeedMultiplier;
            var screenRect = CameraUtils.ScreenRect;

            if (p.x < screenRect.xMin - bodyRadius)
            {
                p.x = screenRect.xMax;
            }

            if (p.x > screenRect.xMax + bodyRadius)
            {
                p.x = screenRect.xMin;
            }

            if (p.y < screenRect.yMin - bodyRadius)
            {
                p.y = screenRect.yMax;
            }

            if (p.y > screenRect.yMax + bodyRadius)
            {
                p.y = screenRect.yMin;
            }

            position = p;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, bodyRadius);

#if SHOW_NEIGHBOURS
            Gizmos.color = Color.magenta;
            foreach (var a in _neighbours)
            {
                Gizmos.DrawLine(position, a.position);
            }
#endif
        }
    }
}
