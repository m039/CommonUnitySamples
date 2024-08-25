#if UNITY_EDITOR
//#define SHOW_NEIGHBOURS
//using System.Collections.Generic;
#endif
using System;
using UnityEngine;

namespace Game.FlockingSample
{
    public class FlockingAgent : MonoBehaviour
    {
        #region Inspector

        public float speed = 1f;

        public float bodyRadius = 1f;

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

        FlockingManager _manager;

#if SHOW_NEIGHBOURS
        readonly List<FlockingAgent> _neighboars = new();
#endif

        public void Initialize(FlockingManager manager)
        {
            _manager = manager;
        }

        public void Move(Vector2 move)
        {
#if SHOW_NEIGHBOURS
            _neighboars.Clear();
            _neighboars.AddRange(_manager.GetNeighbours(this));
#endif

            up = move;
            var p = Vector2.MoveTowards(position, position + move, move.magnitude * speed * Time.deltaTime);
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
            foreach (var a in _neighboars)
            {
                Gizmos.DrawLine(position, a.position);
            }
#endif
        }
    }
}
