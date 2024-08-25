using m039.Common;
using m039.Common.DependencyInjection;
using System.Collections.Generic;
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

        enum NeighboursMode
        {
            GridLookUp = 0, QuadTree = 1, Bruteforce = 2
        }

        #region Inspector

        [SerializeField]
        FlockingAgent _AgentPrefab;

        [SerializeField]
        FlockingBehaviour _Behaviour;

        [SerializeField]
        MinMaxInt _NumberOfAgents = new(10, 10);

        [SerializeField]
        float _NeighbourRadius = 1f;

        [Range(1f, 100f)]
        [SerializeField]
        float _MaxSpeed = 5;

        [SerializeField]
        float _MovementSpeedMultiplier = 1f;

        [SerializeField]
        NeighboursMode _NeighboursMode = NeighboursMode.GridLookUp;

        [SerializeField]
        bool _Debug = true;

        #endregion

        [Provide]
        FlockingManager GetFlockingManager() => this;

        readonly List<FlockingAgent> _agents = new();

        float _previousNeighborRadius;

        public float NeighbourRadius => _NeighbourRadius;

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
                instance.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
                instance.Initialize(this);
                _agents.Add(instance);
            }
        }

        QuadTree _quadTree;

        GridLookUp _gridLookUp;

        FlockingAgent _cachedNeighboursAgent;

        void Update()
        {
            if (_NeighboursMode == NeighboursMode.QuadTree)
            {
                _quadTree = new QuadTree(this);
            } else if (_NeighboursMode == NeighboursMode.GridLookUp)
            {
                if (_gridLookUp == null || !_gridLookUp.IsValid || _NeighbourRadius != _previousNeighborRadius)
                {
                    _gridLookUp = new GridLookUp(this);
                }
            }

            _cachedNeighboursAgent = null;

            foreach (var agent in _agents)
            {
                var move = _Behaviour.CalculateMove(this, agent);
                move *= _MovementSpeedMultiplier;
                if (move.magnitude > _MaxSpeed)
                {
                    move = move.normalized * _MaxSpeed;
                }
                agent.Move(move);
            }

            if (_NeighboursMode == NeighboursMode.GridLookUp)
            {
                _gridLookUp.Update();
            }

            _previousNeighborRadius = _NeighbourRadius;
        }

        public Queue<FlockingAgent> GetNeighbours(FlockingAgent agent)
        {
            if (_cachedNeighboursAgent == agent)
            {
                return s_Neighbours;
            }

            _cachedNeighboursAgent = agent;

            if (_NeighboursMode == NeighboursMode.Bruteforce) {
                s_Neighbours.Clear();

                foreach (var a in _agents)
                {
                    if (a == agent)
                        continue;

                    if (Vector2.Distance(agent.position, a.position) < _NeighbourRadius)
                    {
                        s_Neighbours.Enqueue(a);
                    }
                }

                return s_Neighbours;
            } else if (_NeighboursMode == NeighboursMode.QuadTree)
            {
                return _quadTree.GetNeighbours(agent);
            } else if (_NeighboursMode == NeighboursMode.GridLookUp)
            {
                return _gridLookUp.GetNeighbours(agent);
            } else
            {
                throw new System.Exception("Unsupported");
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, _NeighbourRadius);

            if (_Debug)
            {
                if (_NeighboursMode == NeighboursMode.QuadTree && _quadTree != null)
                {
                    _quadTree.DrawGizmos();
                } else if (_NeighboursMode == NeighboursMode.GridLookUp && _gridLookUp != null)
                {
                    _gridLookUp.DrawGizmos();
                }
            }
        }

        class GridLookUp
        {
            readonly FlockingManager _manager;

            QuadNode<FlockingAgent> _root;

            HashSet<FlockingAgent>[,] _agents;

            Rect _rect;

            Vector2 _size;

            public bool IsValid => _rect.Equals(CameraUtils.ScreenRect);

            public GridLookUp(FlockingManager manager)
            {
                _manager = manager;
                var screenRect = CameraUtils.ScreenRect;
                const float padding = 0.1f;
                _rect = new Rect(
                    screenRect.position - screenRect.size * padding,
                    screenRect.size * (1 + 2 * padding)
                );
                _rect = screenRect;
                var length = _manager._NeighbourRadius * 2;
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
                        if (Vector2.Distance(agent.position, a.position) < _manager._NeighbourRadius)
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
                    if (size / 2f > _manager._NeighbourRadius * 2)
                    {
                        size = size / 2f;
                        _deepestLevel++;
                        continue;
                    }
                    break;
                }

                var agents = new List<FlockingAgent>(_manager._agents);
                _root = CreateNode(rect, 1, agents);
            }

            public Queue<FlockingAgent> GetNeighbours(FlockingAgent agent)
            {
                var rect = CameraUtils.ScreenRect;
                var size = rect.size / (_deepestLevel * 2);
                s_Neighbours.Clear();

                foreach (var direction in Directions)
                {
                    var p = agent.position + direction * size;
                    var deepestNode = GetDeepestNode(_root, p);
                    if (deepestNode == null)
                    {
                        continue;
                    }

                    foreach (var a in deepestNode.data)
                    {
                        if (Vector2.Distance(agent.position, a.position) < _manager._NeighbourRadius)
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

            QuadNode<FlockingAgent> CreateNode(Rect rect, int depth, List<FlockingAgent> agents)
            {
                bool contains = false;

                foreach (var a in agents)
                {
                    if (rect.Contains(a.position))
                    {
                        contains = true;
                        break;
                    }
                }

                if (!contains)
                {
                    return null;
                }

                var node = new QuadNode<FlockingAgent>
                {
                    depth = depth,
                    rect = rect
                };

                if (depth == _deepestLevel)
                {
                    node.data = new List<FlockingAgent>();
                    for (int i = agents.Count - 1; i >= 0; i--)
                    {
                        if (rect.Contains(agents[i].position))
                        {
                            node.data.Add(agents[i]);
                            agents.RemoveAt(i);
                        }
                    }
                    return node;
                }

                var size = new Vector2(rect.width / 2f, rect.height / 2f);

                var rectOne = new Rect(rect.position, size);
                var rectTwo = new Rect(rect.position + new Vector2(size.x, 0), size);
                var rectThree = new Rect(rect.position + new Vector2(0, size.y), size);
                var rectFour = new Rect(rect.position + new Vector2(size.x, size.y), size);

                node.one = CreateNode(rectOne, depth + 1, agents);
                node.two = CreateNode(rectTwo, depth + 1, agents);
                node.three = CreateNode(rectThree, depth + 1, agents);
                node.four = CreateNode(rectFour, depth + 1, agents);
     
                return node;
            }

            public void DrawGizmos()
            {
                void drawNode(QuadNode<FlockingAgent> n)
                {
                    if (n == null)
                        return;

                    if (n.depth == _deepestLevel)
                    {
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
            public List<T> data;
            public int depth;
            public Rect rect;
        }
    }
}
