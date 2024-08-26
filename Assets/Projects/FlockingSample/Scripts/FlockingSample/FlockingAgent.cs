using System;
using UnityEngine;

namespace Game.FlockingSample
{
    public class FlockingAgent :
        MonoBehaviour,
        FlockingManager.IQuadTreeItem,
        FlockingManager.IGridLookUpItem
    {
        #region Inspector

        [SerializeField]
        SpriteRenderer _Renderer;

        public float speed = 1f;

        public float bodyRadius = 1f;

        #endregion

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

        public Color normalColor { get; private set; }

        public Color color {
            set
            {
                _Renderer.color = value;
            }
        }

        Vector2Int FlockingManager.IGridLookUpItem.gridIndex { get; set; }

        object FlockingManager.IQuadTreeItem.node { get; set; }

        public void Initialize(FlockingManager manager)
        {
            _manager = manager;
            normalColor = manager.colorsByNeighbour[UnityEngine.Random.Range(0, manager.colorsByNeighbour.Length)];
            color = normalColor;
        }

        public void Move(Vector2 move)
        {
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
        }
    }
}
