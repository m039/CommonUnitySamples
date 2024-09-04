using m039.Common;
using m039.Common.DependencyInjection;
using m039.Common.Pathfindig;
using UnityEngine;
using UnityEngine.Assertions;

namespace Game
{
    public class GraphControllerDebugger : MonoBehaviour, IDependencyProvider
    {
        #region Inspector

        [SerializeField]
        GraphController _GraphController;

        #endregion

        [Provide]
        GraphControllerDebugger GetGraphControllerDebugger()
        {
            return this;
        }

        Transform _parent;
        
        void Start()
        {
            Assert.IsNotNull(_GraphController, "GraphController is not set.");
            _GraphController.onGraphChanged += OnGraphChanged;
            UpdateDebugGraph();
        }

        void OnEnable()
        {
            UpdateDebugGraph();
        }

        void OnDisable()
        {
            UpdateDebugGraph();
        }

        void OnGraphChanged()
        {
            UpdateDebugGraph();
        }

        void UpdateDebugGraph()
        {
            if (enabled)
            {
                CreateDebugGraph();
            }
            else
            {
                if (_parent != null)
                {
                    Destroy(_parent.gameObject);
                    _parent = null;
                }
            }
        }

        void CreateDebugGraph()
        {
            if (_parent != null)
            {
                Destroy(_parent.gameObject);
                _parent = null;
            }

            if (!gameObject.activeSelf)
                return;

            _parent = new GameObject("< Cells >").transform;
            _parent.SetParent(transform, false);

            var template = CreateTemplate(_parent);

            var width = _GraphController.Graph.Width;
            var height = _GraphController.Graph.Height;
            var borderSize =  _GraphController.borderSize;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var cell = new Cell(template);
                    
                    cell.SetPosition(_GraphController.GetNodePosition(x, y));

                    cell.SetSize(
                        _GraphController.GetCellWidth() - borderSize,
                        _GraphController.GetCellHeight() - borderSize
                    );

                    var node = _GraphController.Graph.GetNode(x, y);
                    if (node.type == NodeType.Blocked)
                    {
                        cell.SetColor(Color.red.With(a: 0.2f));
                    } else
                    {
                        cell.SetColor(Color.white.With(a: 0.05f));
                    }
                }
            }

            Destroy(template.gameObject);
        }

        Transform CreateTemplate(Transform parent)
        {
            var transform = new GameObject("Cell Template").transform;
            transform.SetParent(parent, false);

            var renderer = transform.AddComponent<SpriteRenderer>();
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 1f);
            renderer.sprite = sprite;

            return transform;
        }

        class Cell
        {
            readonly Transform _transform;

            readonly SpriteRenderer _renderer;

            public Cell(Transform template)
            {
                _transform = Instantiate(template, template.parent);
                _renderer = _transform.GetComponent<SpriteRenderer>();
            }

            public void SetPosition(Vector3 position)
            {
                _transform.position = position;
            }

            public void SetColor(Color color)
            {
                _renderer.color = color;
            }

            public void SetSize(float width, float height)
            {
                _transform.localScale = new Vector3(width, height, 0);
            }
        }
    }
}
