using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public interface ISpatialGridItem
    {
        Vector2 position { get; }
        Vector2Int gridIndex { get; set; }
    }

    class SpatialGrid<T> where T : class, ISpatialGridItem
    {
        static readonly Vector2Int[] Directions = new Vector2Int[]
        {
                new Vector2Int(-1, 1),
                new Vector2Int(0, 1),
                new Vector2Int(1, 1),
                new Vector2Int(-1, 0),
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(-1, -1),
                new Vector2Int(0, -1),
                new Vector2Int(1, -1)
        };

        static readonly Queue<T> s_Buffer = new();

        readonly HashSet<T>[,] _items;

        readonly Rect _rect;

        public readonly Rect screenRect;

        public readonly float neighbourRadius;

        readonly Vector2 _size;

        bool _inited = false;

        public SpatialGrid(Rect screenRect, float neighbourRadius)
        {
            this.screenRect = screenRect;
            this.neighbourRadius = neighbourRadius;
            const float padding = 0.1f;
            _rect = new Rect(
                screenRect.position - screenRect.size * padding,
                (1 + 2 * padding) * screenRect.size
            );
            var length = neighbourRadius * 2;
            var width = (int)(_rect.width / length) + 1;
            var height = (int)(_rect.height / length) + 1;
            _size = new Vector2(_rect.width / width, _rect.height / height);
            _items = new HashSet<T>[width, height];
            _inited = false;
        }

        public Queue<T> GetNeighbours(T agent)
        {
            s_Buffer.Clear();
            if (agent.gridIndex.x < 0 || agent.gridIndex.y < 0)
            {
                return s_Buffer;
            }

            foreach (var direction in Directions)
            {
                var newX = agent.gridIndex.x + direction.x;
                var newY = agent.gridIndex.y + direction.y;
                if (newX < 0 || newX >= _items.GetLength(0) ||
                    newY < 0 || newY >= _items.GetLength(1))
                    continue;

                var agents = _items[newX, newY];
                if (agents == null)
                    continue;

                foreach (var a in agents)
                {
                    if (Vector2.Distance(agent.position, a.position) < neighbourRadius && agent != a)
                    {
                        s_Buffer.Enqueue(a);
                    }
                }
            }

            return s_Buffer;
        }

        public void Update(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                var startPosition = item.position - _rect.position;
                var x = (int)(startPosition.x / _size.x);
                var y = (int)(startPosition.y / _size.y);

                if (item.gridIndex.x == x && item.gridIndex.y == y && _inited)
                    continue;

                if (item.gridIndex.x >= 0 && item.gridIndex.x < _items.GetLength(0) &&
                    item.gridIndex.y >= 0 && item.gridIndex.y < _items.GetLength(1) &&
                    _items[item.gridIndex.x, item.gridIndex.y] != null &&
                    _inited)
                {
                    _items[item.gridIndex.x, item.gridIndex.y].Remove(item);
                }

                item.gridIndex = new Vector2Int(-1, -1);

                if (x >= 0 && x < _items.GetLength(0) &&
                    y >= 0 && y < _items.GetLength(1))
                {
                    item.gridIndex = new Vector2Int(x, y);

                    if (_items[x, y] == null)
                    {
                        _items[x, y] = new HashSet<T>();
                    }
                    _items[item.gridIndex.x, item.gridIndex.y].Add(item);
                }
            }

            _inited = true;
        }

        public void DrawGizmos()
        {
            for (int x = 0; x < _items.GetLength(0); x++)
            {
                for (int y = 0; y < _items.GetLength(1); y++)
                {
                    var rect = new Rect(_rect.position + new Vector2(x * _size.x, y * _size.y), _size);

                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireCube(rect.center, rect.size);

                    if (_items[x, y] != null)
                    {
                        foreach (var a in _items[x, y])
                        {
                            Gizmos.color = Color.red;
                            Gizmos.DrawLine(rect.center, a.position);
                        }
                    }
                }
            }
        }

        public void DrawDebug(DebugLinePool debugLinePool)
        {
            for (int x = 0; x < _items.GetLength(0); x++)
            {
                for (int y = 0; y < _items.GetLength(1); y++)
                {
                    var rect = new Rect(_rect.position + new Vector2(x * _size.x, y * _size.y), _size);

                    var line1 = debugLinePool.GetLine();
                    line1.positionCount = 5;
                    line1.SetPosition(0, rect.position);
                    line1.SetPosition(1, rect.position + new Vector2(rect.width, 0));
                    line1.SetPosition(2, rect.position + new Vector2(rect.width, rect.height));
                    line1.SetPosition(3, rect.position + new Vector2(0, rect.height));
                    line1.SetPosition(4, rect.position);
                    line1.startColor = line1.endColor = Color.yellow;

                    if (_items[x, y] != null)
                    {
                        foreach (var a in _items[x, y])
                        {
                            var line2 = debugLinePool.GetLine();
                            line2.positionCount = 2;
                            line2.SetPosition(0, rect.center);
                            line2.SetPosition(1, a.position);
                            line2.startColor = line2.endColor = Color.red;
                        }
                    }
                }
            }
        }
    }
}
