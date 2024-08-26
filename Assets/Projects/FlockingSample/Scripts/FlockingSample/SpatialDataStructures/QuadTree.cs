using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.FlockingSample
{
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

    public class QuadTree<T> where T : class, IQuadTreeItem
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

        readonly int _deepestLevel;

        readonly QuadNode<T> _root;

        public readonly float neighbourRadius;

        public readonly Rect screenRect;

        bool _inited = false;

        public QuadTree(Rect screenRect, float neighbourRadius)
        {
            this.screenRect = screenRect;
            this.neighbourRadius = neighbourRadius;
            _inited = false;
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
                if (item.node == null || !_inited)
                {
                    InserItem(_root, item);
                }
                else
                {
                    var node = (QuadNode<T>)item.node;
                    if (!node.rect.Contains(item.position))
                    {
                        RemoveItem(item);
                        InserItem(_root, item);
                    }
                }
            }

            _inited = true;
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
                if (rectOne.Contains(item.position))
                {
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
            }

            if (node.two != null)
            {
                if (InserItem(node.two, item))
                    return true;
            }
            else
            {
                var rectTwo = new Rect(rect.position + new Vector2(size.x, 0), size);
                if (rectTwo.Contains(item.position))
                {
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
            }

            if (node.three != null)
            {
                if (InserItem(node.three, item))
                    return true;
            }
            else
            {
                var rectThree = new Rect(rect.position + new Vector2(0, size.y), size);
                if (rectThree.Contains(item.position)) { 
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
            }

            if (node.four != null)
            {
                if (InserItem(node.four, item))
                    return true;
            }
            else
            {
                var rectFour = new Rect(rect.position + new Vector2(size.x, size.y), size);
                if (rectFour.Contains(item.position))
                {
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

        public void DrawDebug(DebugLinePool debugLinePool)
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
                    var line1 = debugLinePool.GetLine();
                    line1.positionCount = 5;
                    line1.SetPosition(0, rect.position);
                    line1.SetPosition(1, rect.position + new Vector2(rect.width, 0));
                    line1.SetPosition(2, rect.position + new Vector2(rect.width, rect.height));
                    line1.SetPosition(3, rect.position + new Vector2(0, rect.height));
                    line1.SetPosition(4, rect.position);
                    line1.startColor = line1.endColor = Color.yellow;

                    foreach (var a in n.data)
                    {
                        var line2 = debugLinePool.GetLine();
                        line2.positionCount = 2;
                        line2.SetPosition(0, rect.center);
                        line2.SetPosition(1, a.position);
                        line2.startColor = line2.endColor = Color.red;
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
}
