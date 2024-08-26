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
            SpatialGrid = 0, Physics = 1, QuadTree = 2, Bruteforce = 3
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

        public NeighboursMode neighboursMode = NeighboursMode.SpatialGrid;

        [SerializeField]
        bool _Debug = true;

        public Color[] colorsByNeighbour;

        #endregion

        [NonSerialized]
        public float aligmentCoeff = 1f;

        [NonSerialized]
        public float separationCoeff = 1f;

        [NonSerialized]
        public float cohesionCoeff = 1f;

        [NonSerialized]
        public bool debugNeighbours = false;

        [NonSerialized]
        public bool debugGrids = false;

        [NonSerialized]
        public bool colorByNeighbours = true;

        [NonSerialized]
        public bool colorize = true;

        [Provide]
        FlockingManager GetFlockingManager() => this;

        readonly List<FlockingAgent> _agents = new();

        float _previousNeighborRadius;

        QuadTree<FlockingAgent> _quadTree;

        SpatialGrid<FlockingAgent> _spatialGrid;

        FlockingAgent _cachedNeighboursAgent;

        Queue<FlockingAgent> _cachedNeighbours;

        DebugLinePool _debugLinePool;

        void Awake()
        {
            Init();
        }

        void Init()
        {
            var template = transform.Find("LineTemplate").GetComponent<LineRenderer>();
            _debugLinePool = new DebugLinePool(template);
        }

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
            // Recreate grids if necessary.
            if (neighboursMode == NeighboursMode.QuadTree)
            {
                if (_quadTree == null ||
                    _quadTree.neighbourRadius != neighbourRadius ||
                    !_quadTree.screenRect.Equals(CameraUtils.ScreenRect))
                {
                    _quadTree = new QuadTree<FlockingAgent>(CameraUtils.ScreenRect, neighbourRadius);
                }
            } else if (neighboursMode == NeighboursMode.SpatialGrid)
            {
                if (_spatialGrid == null ||
                    _spatialGrid.neighbourRadius != neighbourRadius ||
                    !_spatialGrid.screenRect.Equals(CameraUtils.ScreenRect))
                {
                    _spatialGrid = new SpatialGrid<FlockingAgent>(CameraUtils.ScreenRect, neighbourRadius);
                }
            }

            _cachedNeighboursAgent = null;
            _cachedNeighbours = null;

            if (debugNeighbours || debugGrids)
            {
                _debugLinePool.PreUpdate();
            } else
            {
                _debugLinePool.DestroyLines();
            }

            // Main logic: calculate move and update the agents.
            foreach (var agent in _agents)
            {
                if (debugNeighbours)
                {
                    DebugNeighbours(agent);
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

            // Update grids and draw debug if needed.
            if (neighboursMode == NeighboursMode.SpatialGrid)
            {
                _spatialGrid.Update(_agents);

                if (debugGrids)
                {
                    _spatialGrid.DrawDebug(_debugLinePool);
                }

            } else if (neighboursMode == NeighboursMode.QuadTree)
            {
                _quadTree.Update(_agents);

                if (debugGrids)
                {
                    _quadTree.DrawDebug(_debugLinePool);
                }
            }

            _previousNeighborRadius = neighbourRadius;
        }

        void SetAgentColor(FlockingAgent agent)
        {
            if (!colorize)
            {
                agent.color = Color.white;
                return;
            }

            if (!colorByNeighbours) {
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

        void DebugNeighbours(FlockingAgent agent)
        {
            foreach (var neighbour in GetNeighbours(agent))
            {
                LineRenderer line = _debugLinePool.GetLine();

                line.positionCount = 2;
                line.SetPosition(0, agent.position);
                line.SetPosition(1, neighbour.position);
                line.startColor = line.endColor = Color.magenta;
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
            else if (mode == NeighboursMode.SpatialGrid)
            {
                return _spatialGrid.GetNeighbours(agent);
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
                return _cachedNeighbours;
            }

            _cachedNeighboursAgent = agent;
            return _cachedNeighbours = GetNeighbours(agent, neighboursMode);
        }

        [ContextMenu("Validate Neighbours")]
        void ValidateNeighbours()
        {
            if (!Application.isPlaying)
                return;

            _quadTree = new QuadTree<FlockingAgent>(CameraUtils.ScreenRect, neighbourRadius);
            _spatialGrid = new SpatialGrid<FlockingAgent>(CameraUtils.ScreenRect, neighbourRadius);
            _spatialGrid.Update(_agents);
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
                } else if (neighboursMode == NeighboursMode.SpatialGrid && _spatialGrid != null)
                {
                    _spatialGrid.DrawGizmos();
                }
            }
        }
    }
}
