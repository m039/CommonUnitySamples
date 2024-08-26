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

        QuadTree _quadTree;

        GridLookUp _gridLookUp;

        FlockingAgent _cachedNeighboursAgent;

        void Update()
        {
            if (neighboursMode == NeighboursMode.QuadTree)
            {
                _quadTree = new QuadTree(this);
            } else if (neighboursMode == NeighboursMode.GridLookUp)
            {
                if (_gridLookUp == null || !_gridLookUp.IsValid || neighbourRadius != _previousNeighborRadius)
                {
                    _gridLookUp = new GridLookUp(this);
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
                _gridLookUp.Update();
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
                return _quadTree.GetNeighbours(agent);
            }
            else if (mode == NeighboursMode.GridLookUp)
            {
                return _gridLookUp.GetNeighbours(agent);
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

            _quadTree = new QuadTree(this);
            _gridLookUp = new GridLookUp(this);
            _gridLookUp.Update();

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

        class GridLookUp
        {
            readonly FlockingManager _manager;

            readonly QuadNode<FlockingAgent> _root;

            readonly HashSet<FlockingAgent>[,] _agents;

            Rect _rect;

            Rect _screenRect;

            Vector2 _size;

            public bool IsValid => _screenRect.Equals(CameraUtils.ScreenRect);

            public GridLookUp(FlockingManager manager)
            {
                _manager = manager;
                _screenRect = CameraUtils.ScreenRect;
                const float padding = 0.1f;
                _rect = new Rect(
                    _screenRect.position - _screenRect.size * padding,
                    (1 + 2 * padding) * _screenRect.size
                );
                var length = _manager.neighbourRadius * 2;
                var width = (int)(_rect.width / length) + 1;
                var height = (int)(_rect.height / length) + 1;
                _size = new Vector2(_rect.width / width, _rect.height / height);
                _agents = new HashSet<FlockingAgent>[width, height];

                foreach (var a in _manager._agents)
                {
                    a.gridX = -1;
                    a.gridY = -1;
                }
            }

            public Queue<FlockingAgent> GetNeighbours(FlockingAgent agent)
            {
                s_Neighbours.Clear();
                if (agent.gridX < 0 || agent.gridY < 0)
                {
                    return s_Neighbours;
                }

                foreach (var direction in Directions)
                {
                    var newX = agent.gridX + direction.x;
                    var newY = agent.gridY + direction.y;
                    if (newX < 0 || newX >= _agents.GetLength(0) ||
                        newY < 0 || newY >= _agents.GetLength(1))
                        continue;

                    var agents = _agents[newX, newY];
                    if (agents == null)
                        continue;

                    foreach (var a in agents)
                    {
                        if (Vector2.Distance(agent.position, a.position) < _manager.neighbourRadius && agent != a)
                        {
                            s_Neighbours.Enqueue(a);
                        }
                    }
                }

                return s_Neighbours;
            }

            public void Update()
            {
                foreach (var a in _manager._agents)
                {
                    var startPosition = a.position - _rect.position;
                    var x = (int)(startPosition.x / _size.x);
                    var y = (int)(startPosition.y / _size.y);

                    if (a.gridX == x && a.gridY == y)
                        continue;

                    if (a.gridX >= 0 && a.gridX < _agents.GetLength(0) &&
                        a.gridY >= 0 && a.gridY < _agents.GetLength(1) &&
                        _agents[a.gridX, a.gridY] != null)
                    {
                        _agents[a.gridX, a.gridY].Remove(a);
                    }

                    a.gridX = -1;
                    a.gridY = -1;

                    if (x >= 0 && x < _agents.GetLength(0) &&
                        y >= 0 && y < _agents.GetLength(1))
                    {
                        a.gridX = x;
                        a.gridY = y;

                        if (_agents[x, y] == null)
                        {
                            _agents[x, y] = new HashSet<FlockingAgent>();
                        }
                        _agents[a.gridX, a.gridY].Add(a);
                    }
                }
            }

            public void DrawGizmos()
            {
                for (int x = 0; x < _agents.GetLength(0); x++)
                {
                    for (int y = 0; y < _agents.GetLength(1); y++)
                    {
                        var rect = new Rect(_rect.position + new Vector2(x * _size.x, y * _size.y), _size);

                        Gizmos.color = Color.yellow;
                        Gizmos.DrawWireCube(rect.center, rect.size);

                        if (_agents[x, y] != null)
                        {
                            foreach (var a in _agents[x, y])
                            {
                                Gizmos.color = Color.red;
                                Gizmos.DrawLine(rect.center, a.position);
                            }
                        }
                    }
                }
            }
        }

        class QuadTree
        {
            int _deepestLevel;

            readonly FlockingManager _manager;

            QuadNode<FlockingAgent> _root;

            public QuadTree(FlockingManager manager)
            {
                _manager = manager;
                Init();
            }

            void Init()
            {
                var screenRect = CameraUtils.ScreenRect;
                const float padding = 0.1f;
                var rect = new Rect(
                    screenRect.position - new Vector2(screenRect.width, screenRect.height) * padding,
                    screenRect.size * (1 + 2 * padding)
                );

                var size = Mathf.Min(rect.width, rect.height);
                _deepestLevel = 1;
                while (true)
                {
                    if (size / 2f > _manager.neighbourRadius * 2)
                    {
                        size = size / 2f;
                        _deepestLevel++;
                        continue;
                    }
                    break;
                }

                _root = new QuadNode<FlockingAgent>
                {
                    depth = 1,
                    rect = rect
                };

                foreach (var a in _manager._agents)
                {
                    InsertAgent(_root, a);
                }
            }

            bool InsertAgent(QuadNode<FlockingAgent> node, FlockingAgent agent)
            {
                if (!node.rect.Contains(agent.position))
                    return false;

                if (node.depth == _deepestLevel)
                {
                    if (node.data == null)
                    {
                        node.data = new();
                    }

                    node.data.Add(agent);
                    return true;
                }

                var rect = node.rect;
                var size = new Vector2(rect.width / 2f, rect.height / 2f);
                var depth = node.depth + 1;

                if (node.one != null)
                {
                    if (InsertAgent(node.one, agent))
                        return true;
                }
                else
                {
                    var rectOne = new Rect(rect.position, size);
                    var one = new QuadNode<FlockingAgent>
                    {
                        rect = rectOne,
                        depth = depth
                    };
                    node.one = one;
                    if (InsertAgent(one, agent))
                        return true;
                }

                if (node.two != null)
                {
                    if (InsertAgent(node.two, agent))
                        return true;
                }
                else
                {
                    var rectTwo = new Rect(rect.position + new Vector2(size.x, 0), size);
                    var two = new QuadNode<FlockingAgent>
                    {
                        rect = rectTwo,
                        depth = depth
                    };
                    node.two = two;
                    if (InsertAgent(two, agent))
                        return true;
                }

                if (node.three != null)
                {
                    if (InsertAgent(node.three, agent))
                        return true;
                }
                else
                {
                    var rectThree = new Rect(rect.position + new Vector2(0, size.y), size);
                    var three = new QuadNode<FlockingAgent>
                    {
                        rect = rectThree,
                        depth = depth
                    };
                    node.three = three;
                    if (InsertAgent(three, agent))
                        return true;
                }

                if (node.four != null)
                {
                    if (InsertAgent(node.four, agent))
                        return true;
                }
                else
                {
                    var rectFour = new Rect(rect.position + new Vector2(size.x, size.y), size);
                    var four = new QuadNode<FlockingAgent>
                    {
                        rect = rectFour,
                        depth = depth
                    };
                    node.four = four;
                    if (InsertAgent(four, agent))
                        return true;
                }

                return false;
            }

            public Queue<FlockingAgent> GetNeighbours(FlockingAgent agent)
            {
                var rect = CameraUtils.ScreenRect;
                var size = Vector2.one * _manager.neighbourRadius;
                s_Neighbours.Clear();

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
                        if (Vector2.Distance(agent.position, a.position) < _manager.neighbourRadius && a != agent)
                        {
                            s_Neighbours.Enqueue(a);
                        }
                    }
                }

                return s_Neighbours;
            }

            QuadNode<FlockingAgent> GetDeepestNode(QuadNode<FlockingAgent> node, Vector2 position)
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
                void drawNode(QuadNode<FlockingAgent> n)
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

        class QuadNode<T> where T : FlockingAgent
        {
            public QuadNode<T> one;
            public QuadNode<T> two;
            public QuadNode<T> three;
            public QuadNode<T> four;
            public HashSet<T> data;
            public int depth;
            public Rect rect;
        }
    }
}
