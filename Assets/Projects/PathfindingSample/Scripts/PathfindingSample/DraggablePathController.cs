using m039.Common.DependencyInjection;
using m039.Common.Pathfindig;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.PathfindingSample
{
    public class DraggablePathController : MonoBehaviour, IDependencyProvider
    {
        #region Inspector

        [SerializeField]
        Transform _Handle1;

        [SerializeField]
        Transform _Handle2;

        [SerializeField]
        Color _GoodPathColor = Color.green;

        [SerializeField]
        Color _BadPathColor = Color.red;

        [SerializeField]
        LineRenderer _LineRenderer;

        [SerializeField]
        Seeker _Seeker;

        [SerializeField]
        PathSmoother _Smoother;

        #endregion

        Handle _handle1;

        Handle _handle2;

        Handle _currentHandle;

        [Provide]
        DraggablePathController GetDraggablePathController()
        {
            return this;
        }

        public void ResetState()
        {
            _handle1.Reset();
            _handle2.Reset();
            Refresh();
        }

        void Awake()
        {
            Init();
        }

        void Init()
        {
            _handle1 = new Handle(_Handle1);
            _handle2 = new Handle(_Handle2);
            Refresh();
        }

        void Update()
        {
            ProcessInput();
        }

        void ProcessInput()
        {
            var position = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (_currentHandle == null)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (_handle1.collider.OverlapPoint(position))
                    {
                        _currentHandle = _handle1;
                    } else if (_handle2.collider.OverlapPoint(position))
                    {
                        _currentHandle = _handle2;
                    }

                    if (_currentHandle != null)
                    {
                        _currentHandle.renderer.color = Color.gray;
                    }
                }
            } else
            {
                if (Input.GetMouseButton(0))
                {
                    var p = _currentHandle.transform.position;
                    p.x = position.x;
                    p.y = position.y;
                    _currentHandle.transform.position = p;
                    Refresh();
                } else if (Input.GetMouseButtonUp(0))
                {
                    _currentHandle.renderer.color = Color.white;
                    _currentHandle = null;
                }
            }
        }

        const float Z = 10f;

        public void Refresh()
        {
            var path = _Seeker.Search(_handle1.position, _handle2.position);
            IList<Vector3> vectorPath = null;
            if (path != null) {
                vectorPath = _Smoother.GetSmoothPath(path.vectorPath);
            }

            if (path == null || vectorPath == null || vectorPath.Count <= 0)
            {
                _LineRenderer.positionCount = 2;
                _LineRenderer.SetPosition(0, _handle1.position);
                _LineRenderer.SetPosition(1, _handle2.position);
                _LineRenderer.startColor = _LineRenderer.endColor = _BadPathColor;
            } else
            {
                _LineRenderer.positionCount = vectorPath.Count + 2;
                _LineRenderer.SetPosition(0, _handle1.position);
                for (int i = 0; i < vectorPath.Count; i++)
                {
                    _LineRenderer.SetPosition(i + 1, CreatePosition(vectorPath[i], Z));
                }
                _LineRenderer.SetPosition(_LineRenderer.positionCount - 1, _handle2.position);
                _LineRenderer.startColor = _LineRenderer.endColor = _GoodPathColor;
            }
        }

        static Vector3 CreatePosition(Vector3 position, float z)
        {
            position.z = z;
            return position;
        }

        class Handle
        {
            Vector3 _defaultPosition;

            public Handle(Transform transform)
            {
                this.transform = transform;
                _defaultPosition = transform.position;
                collider = transform.GetComponentInChildren<Collider2D>();
                renderer = transform.GetComponentInChildren<SpriteRenderer>();
            }

            public readonly Transform transform;

            public readonly Collider2D collider;

            public readonly SpriteRenderer renderer;
            
            public Vector3 position => CreatePosition(transform.position, Z);

            public void Reset()
            {
                transform.position = _defaultPosition;
            }
        }
    }
}
