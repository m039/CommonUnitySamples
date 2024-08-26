using m039.Common;
using m039.Common.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Game.FlockingSample
{
    public class FlockingManager : MonoBehaviour, IDependencyProvider
    {
        static readonly Queue<FlockingAgent> s_Neighbours = new();

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

        public enum NeighboursMode
        {
            GridLookUp = 0, Physics = 1, QuadTree = 2, Bruteforce = 3
        }

        #region Inspector

        [SerializeField]
        FlockingAgent _AgentPrefab;

        [SerializeField]
        FlockingBehaviour _Behaviour;

        [SerializeField]
        MinMaxInt _NumberOfAgents = new(10, 10);

        public float neighbourRadius = 0.5f;

        [Range(1f, 100f)]
        [SerializeField]
        float _MaxSpeed = 5;

        public float movementSpeedMultiplier = 0.5f;

        public NeighboursMode neighboursMode = NeighboursMode.GridLookUp;

        [SerializeField]
        bool _Debug = true;

        public Color[] colorsByNeighbour;

        [NonSerialized]
        public float aligmentCoeff = 1f;

        [NonSerialized]
        public float separationCoeff = 1f;

        [NonSerialized]
        public float cohesionCoeff = 1f;

        #endregion

        [Provide]
        FlockingManager GetFlockingManager() => this;

        readonly List<FlockingAgent> _agents = new();

        float _previousNeighborRadius;

        LineRenderer _lineTemplate;

        readonly List<LineRenderer> _lines = new();

        bool _debugNeighbours = false;

        bool _colorByNeighbours;

        bool _colorize = true;

        QuadTree<FlockingAgent> _quadTree;

        GridLookUp<FlockingAgent> _gridLookUp;

        FlockingAgent _cachedNeighboursAgent;

        void Awake()
        {
            Init();
        }

        void Init()
        {
            _lineTemplate = transform.Find("LineTemplate").GetComponent<LineRenderer>();
        }

        public void SetDebugNeighbours(bool value) => _debugNeighbours = value;

        public void SetColorByNeighbours(bool value) => _colorByNeighbours = value;

        public void SetColorize(bool value) => _colorize = value;

        public void CreateAgents()
        {
            foreach (var agent in _agents)
            {
                Destroy(agent.gameObject);
            }

            _agents.Clear();

            var count = _NumberOfAgents.Random();
            for (int i = 0; i < count; i++)
            {
                var position = CameraUtils.RandomPositionOnScreen();
                var instance = Instantiate(_AgentPrefab, transform, false);
                instance.transform.position = position;
                instance.transform.rotation = Quaternion.Euler(0, 0, UnityEngine.Random.Range(0, 360));
                instance.Initialize(this);
                _agents.Add(instance);
                instance.gameObject.name = $"Agent {i}";
            }
        }

        void Update()
        {
            if (neighboursMode == NeighboursMode.QuadTree)
            {
                if (_quadTree == null ||
                    _quadTree.neighbourRadius != neighbourRadius ||
                    !_quadTree.screenRect.Equals(CameraUtils.ScreenRect))
                {
                    _quadTree = new QuadTree<FlockingAgent>(CameraUtils.ScreenRect, neighbourRadius);
                }
            } else if (neighboursMode == NeighboursMode.GridLookUp)
            {
                if (_gridLookUp == null ||
                    _gridLookUp.neighbourRadius != neighbourRadius ||
                    !_gridLookUp.screenRect.Equals(CameraUtils.ScreenRect))
                {
                    _gridLookUp = new GridLookUp<FlockingAgent>(CameraUtils.ScreenRect, neighbourRadius);
                }
            }

            _cachedNeighboursAgent = null;

            Queue<LineRenderer> lines = null;
            if (_debugNeighbours)
            {
                lines = new Queue<LineRenderer>(_lines);
                ClearDebugLines();
            } else
            {
                DestroyDebugLines();
            }

            foreach (var agent in _agents)
            {
                if (_debugNeighbours)
                {
                    DebugNeighbours(agent, lines);
                }

                SetAgentColor(agent);

                var move = _Behaviour.CalculateMove(this, agent);
                move *= movementSpeedMultiplier;
                if (move.magnitude > _MaxSpeed)
                {
                    move = move.normalized * _MaxSpeed;
                }
                agent.Move(move);
            }

            if (neighboursMode == NeighboursMode.GridLookUp)
            {
                _gridLookUp.Update(_agents);
            } else if (neighboursMode == NeighboursMode.QuadTree)
            {
                _quadTree.Update(_agents);
            }

            _previousNeighborRadius = neighbourRadius;
        }

        void SetAgentColor(FlockingAgent agent)
        {
            if (!_colorize)
            {
                agent.color = Color.white;
                return;
            }

            if (!_colorByNeighbours) {
                agent.color = agent.normalColor;
                return;
            }

            var neighbours = GetNeighbours(agent);
            if (neighbours.Count <= 0)
            {
                agent.color = agent.normalColor;
                return;
            }

            var color = Color.black;

            foreach (var neighbour in neighbours)
            {
                color += neighbour.normalColor;
            }

            color += agent.normalColor;
            color /= (neighbours.Count + 1);
            agent.color = color.With(a: 1f);
        }

        void ClearDebugLines()
        {
            foreach (var l in _lines)
            {
                l.positionCount = 0;
            }
        }

        void DestroyDebugLines()
        {
            if (_lines.Count <= 0)
                return;

            foreach (var l in _lines)
            {
                Destroy(l.gameObject);
            }
            _lines.Clear();
        }

        void DebugNeighbours(FlockingAgent agent, Queue<LineRenderer> lines)
        {
            foreach (var neighbour in GetNeighbours(agent))
            {
                LineRenderer line;

                if (lines.Count > 0)
                {
                    line = lines.Dequeue();
                } else
                {
                    line = Instantiate(_lineTemplate);
                    line.transform.SetParent(transform);
                    _lines.Add(line);
                }

                line.positionCount = 2;
                line.SetPosition(0, agent.position);
                line.SetPosition(1, neighbour.position);
            }
        }

        Queue<FlockingAgent> GetNeighbours(FlockingAgent agent, NeighboursMode mode)
        {
            if (mode == NeighboursMode.Bruteforce)
            {
                s_Neighbours.Clear();

                foreach (var a in _agents)
                {
                    if (a == agent)
                        continue;

                    if (Vector2.Distance(agent.position, a.position) < neighbourRadius)
                    {
                        s_Neighbours.Enqueue(a);
                    }
                }

                return s_Neighbours;
            }
            else if (mode == NeighboursMode.QuadTree)
            {
                s_Neighbours.Clear();
                foreach (var a in _quadTree.GetNeighbours(agent))
                {
                    s_Neighbours.Enqueue(a);
                }
                return s_Neighbours;
            }
            else if (mode == NeighboursMode.GridLookUp)
            {
                s_Neighbours.Clear();
                foreach (var a in _gridLookUp.GetNeighbours(agent))
                {
                    s_Neighbours.Enqueue(a);
                }
                return s_Neighbours;
            }
            else if (mode == NeighboursMode.Physics)
            {
                s_Neighbours.Clear();
                var colliders = Physics2D.OverlapCircleAll(agent.position, neighbourRadius);

                foreach (var c in colliders)
                {
                    if (c.GetComponentInParent<FlockingAgent>() is FlockingAgent a && a != agent &&
                        Vector2.Distance(agent.position, a.position) < neighbourRadius)
                    {
                        s_Neighbours.Enqueue(a);
                    }
                }

                return s_Neighbours;
            }
            else
            {
                throw new System.Exception("Unsupported");
            }
        }

        public Queue<FlockingAgent> GetNeighbours(FlockingAgent agent)
        {
            if (_cachedNeighboursAgent == agent)
            {
                return s_Neighbours;
            }

            _cachedNeighboursAgent = agent;

            return GetNeighbours(agent, neighboursMode);
        }

        [ContextMenu("Validate Neighbours")]
        void ValidateNeighbours()
        {
            if (!Application.isPlaying)
                return;

            _quadTree = new QuadTree<FlockingAgent>(CameraUtils.ScreenRect, neighbourRadius);
            _gridLookUp = new GridLookUp<FlockingAgent>(CameraUtils.ScreenRect, neighbourRadius);
            _gridLookUp.Update(_agents);
            _quadTree.Update(_agents);

            int count = 0;

            foreach (var a in _agents) {
                count++;
                var results = new Dictionary<NeighboursMode, HashSet<FlockingAgent>>();
                foreach (NeighboursMode mode in Enum.GetValues(typeof(NeighboursMode)))
                {
                    results.Add(mode, new HashSet<FlockingAgent>(GetNeighbours(a, mode)));
                }

                foreach (var p1 in results)
                {
                    foreach (var p2 in results)
                    {
                        if (p1.Key == p2.Key)
                        {
                            continue;
                        }

                        if (!p1.Value.SetEquals(p2.Value))
                        {
                            var sb = new StringBuilder();
                            sb.AppendLine($"Data mistmach in {p1.Key}[{p1.Value.Count}] and {p2.Key}[{p2.Value.Count}] for {a.gameObject.name}.");
                            sb.AppendLine($"Neighbours in {p1.Key}:");
                            foreach (var a2 in p1.Value)
                            {
                                sb.AppendLine(" " + a2.gameObject.name);
                            }
                            sb.AppendLine($"Neighbours in {p2.Key}:");
                            foreach (var a2 in p2.Value)
                            {
                                sb.AppendLine(" " + a2.gameObject.name);
                            }
                            Debug.Log(sb.ToString());
                            Debug.Break();
                            goto exit;
                        }
                    }
                }
            }

            Debug.Log($"Data is valid. Compared {count} agents.");
            return;
        exit:
            return;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, neighbourRadius);

            if (_Debug)
            {
                if (neighboursMode == NeighboursMode.QuadTree && _quadTree != null)
                {
                    _quadTree.DrawGizmos();
                } else if (neighboursMode == NeighboursMode.GridLookUp && _gridLookUp != null)
                {
                    _gridLookUp.DrawGizmos();
                }
            }
        }

        public interface IGridLookUpItem
        {
            Vector2 position { get; }
            Vector2Int gridIndex { get; set; }
        }

        class GridLookUp<T> where T : class, IGridLookUpItem
        {
            static readonly Queue<T> s_Buffer = new();

            readonly HashSet<T>[,] _items;

            readonly Rect _rect;

            public readonly Rect screenRect;

            public readonly float neighbourRadius;

            readonly Vector2 _size;

            bool _inited = false;

            public GridLookUp(Rect screenRect, float neighbourRadius)
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
        }

        class QuadTree<T> where T : class, IQuadTreeItem
        {
            static readonly Queue<T> s_Buffer = new();

            readonly int _deepestLevel;

            readonly QuadNode<T> _root;

            public readonly float neighbourRadius;

            public readonly Rect screenRect;

            bool _initedItems = false;

            public QuadTree(Rect screenRect, float neighbourRadius)
            {
                this.screenRect = screenRect;
                this.neighbourRadius = neighbourRadius;
                _initedItems = false;
                const float padding = 0.1f;
                var rect = new Rect(
                    screenRect.position - new Vector2(screenRect.width, screenRect.height) * padding,
                    screenRect.size * (1 + 2 * padding)
                );

                var size = Mathf.Min(rect.width, rect.height);
                _deepestLevel = 1;
                while (true)
                {
                    if (size / 2f > neighbourRadius * 2)
                    {
                        size = size / 2f;
                        _deepestLevel++;
                        continue;
                    }
                    break;
                }

                _root = new QuadNode<T>
                {
                    depth = 1,
                    rect = rect
                };
            }

            public void Update(IEnumerable<T> items)
            {
                foreach (var item in items)
                {
                    if (item.node == null || !_initedItems)
                    {
                        InserItem(_root, item);
                    } else
                    {
                        var node = (QuadNode<T>)item.node;
                        if (!node.rect.Contains(item.position))
                        {
                            RemoveItem(item);
                            InserItem(_root, item);
                        }
                    }
                }

                _initedItems = true;
            }

            void RemoveItem(T item)
            {
                if (item.node == null)
                    return;

                var node = (QuadNode<T>)item.node;
                item.node = null;

                if (node.data != null)
                {
                    node.data.Remove(item);
                    if (node.data.Count <= 0)
                    {
                        node.data = null;
                    }
                }

                if (node.data != null)
                    return;

                while (node.parent != null)
                {
                    if (node.one == null && node.two == null && node.three == null && node.four == null)
                    {
                        if (node.parent.one == node)
                        {
                            node.parent.one = null;
                            node = node.parent;
                            continue;
                        }

                        if (node.parent.two == node)
                        {
                            node.parent.two = null;
                            node = node.parent;
                            continue;
                        }

                        if (node.parent.three == node)
                        {
                            node.parent.three = null;
                            node = node.parent;
                            continue;
                        }

                        if (node.parent.four == node)
                        {
                            node.parent.four = null;
                            node = node.parent;
                            continue;
                        }
                    }

                    break;
                }
            }

            bool InserItem(QuadNode<T> node, T item)
            {
                if (!node.rect.Contains(item.position))
                    return false;

                if (node.depth == _deepestLevel)
                {
                    if (node.data == null)
                    {
                        node.data = new();
                    }

                    node.data.Add(item);
                    item.node = node;
                    return true;
                }

                var rect = node.rect;
                var size = new Vector2(rect.width / 2f, rect.height / 2f);
                var depth = node.depth + 1;

                if (node.one != null)
                {
                    if (InserItem(node.one, item))
                        return true;
                }
                else
                {
                    var rectOne = new Rect(rect.position, size);
                    var one = new QuadNode<T>
                    {
                        rect = rectOne,
                        depth = depth,
                        parent = node
                    };
                    node.one = one;
                    if (InserItem(one, item))
                        return true;
                }

                if (node.two != null)
                {
                    if (InserItem(node.two, item))
                        return true;
                }
                else
                {
                    var rectTwo = new Rect(rect.position + new Vector2(size.x, 0), size);
                    var two = new QuadNode<T>
                    {
                        rect = rectTwo,
                        depth = depth,
                        parent = node
                    };
                    node.two = two;
                    if (InserItem(two, item))
                        return true;
                }

                if (node.three != null)
                {
                    if (InserItem(node.three, item))
                        return true;
                }
                else
                {
                    var rectThree = new Rect(rect.position + new Vector2(0, size.y), size);
                    var three = new QuadNode<T>
                    {
                        rect = rectThree,
                        depth = depth,
                        parent = node
                    };
                    node.three = three;
                    if (InserItem(three, item))
                        return true;
                }

                if (node.four != null)
                {
                    if (InserItem(node.four, item))
                        return true;
                }
                else
                {
                    var rectFour = new Rect(rect.position + new Vector2(size.x, size.y), size);
                    var four = new QuadNode<T>
                    {
                        rect = rectFour,
                        depth = depth,
                        parent = node
                    };
                    node.four = four;
                    if (InserItem(four, item))
                        return true;
                }

                return false;
            }

            public Queue<T> GetNeighbours(T agent)
            {
                var rect = CameraUtils.ScreenRect;
                var size = Vector2.one * neighbourRadius;
                var neighbours = s_Buffer;
                neighbours.Clear();

                foreach (var direction in Directions)
                {
                    var p = agent.position + direction * size;
                    var deepestNode = GetDeepestNode(_root, p);
                    if (deepestNode == null)
                    {
                        continue;
                    }

                    if (deepestNode.data == null)
                        continue;

                    foreach (var a in deepestNode.data)
                    {
                        if (Vector2.Distance(agent.position, a.position) < neighbourRadius && a != agent)
                        {
                            neighbours.Enqueue(a);
                        }
                    }
                }

                return neighbours;
            }

            QuadNode<T> GetDeepestNode(QuadNode<T> node, Vector2 position)
            {
                if (node == null)
                    return null;

                if (!node.rect.Contains(position))
                    return null;

                if (node.depth == _deepestLevel)
                {
                    return node;
                }

                var r = GetDeepestNode(node.one, position);
                if (r != null)
                    return r;

                r = GetDeepestNode(node.two, position);
                if (r != null)
                    return r;

                r = GetDeepestNode(node.three, position);
                if (r != null)
                    return r;

                return GetDeepestNode(node.four, position);
            }

            public void DrawGizmos()
            {
                void drawNode(QuadNode<T> n)
                {
                    if (n == null)
                        return;

                    if (n.depth == _deepestLevel)
                    {
                        if (n.data == null)
                            return;

                        var rect = n.rect;
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawWireCube(rect.center, rect.size);
                        
                        foreach (var a in n.data)
                        {
                            Gizmos.color = Color.red;
                            Gizmos.DrawLine(rect.center, a.position);
                        }
                        return;
                    }

                    drawNode(n.one);
                    drawNode(n.two);
                    drawNode(n.three);
                    drawNode(n.four);
                }
                drawNode(_root);
            }
        }

        public interface IQuadTreeItem
        {
            Vector2 position { get; }
            object node { get; set; }
        }

        public class QuadNode<T> where T : IQuadTreeItem
        {
            public QuadNode<T> one;
            public QuadNode<T> two;
            public QuadNode<T> three;
            public QuadNode<T> four;
            public HashSet<T> data;
            public int depth;
            public Rect rect;
            public QuadNode<T> parent;
        }
    }
}
