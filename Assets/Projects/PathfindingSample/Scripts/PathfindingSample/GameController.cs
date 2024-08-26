using m039.Common.DependencyInjection;
using m039.Common.Pathfindig;
using System.Collections;
using UnityEngine;

namespace Game.PathfindingSample
{
    public class GameController : CoreGameController
    {
        #region Inspector

        [SerializeField]
        GraphController _graphController;

        #endregion

        [Inject]
        readonly WorldGenerator _worldGenerator;

        [Inject]
        readonly DraggablePathController _pathController;

        [Inject]
        readonly ModularPanel _modularPanel;

        Vector2 _previousSize;

        void Start()
        {
            UpdateGraphController();
            Generate();
            CreatePanel();
        }

        void CreatePanel()
        {
            if (_modularPanel == null)
                return;

            var builder = _modularPanel.CreateBuilder();

            var enablePathSmootherItem = new ModularPanel.ToggleItem(false, "Enable Path Smoother");
            enablePathSmootherItem.onValueChanged += (v) =>
            {
                _graphController.GetComponent<PathSmoother>().enabled = v;
                _pathController.Refresh();
            };
            builder.AddItem(enablePathSmootherItem);

            var enableRaycastModifierItem = new ModularPanel.ToggleItem(true, "Enable Raycast Modifier");
            enableRaycastModifierItem.onValueChanged += (v) =>
            {
                _graphController.GetComponent<RaycastModifier>().enabled = v;
                _pathController.Refresh();
            };
            builder.AddItem(enableRaycastModifierItem);

            var debugGraphControllerItem = new ModularPanel.ToggleItem(false, "Debug Graph Controller");
            debugGraphControllerItem.onValueChanged += (v) =>
            {
                _graphController.GetComponent<GraphControllerDebugger>().enabled = v;
            };
            builder.AddItem(debugGraphControllerItem);

            var resetItem = new ModularPanel.ButtonItem("Reset");
            resetItem.onClick += OnResetClicked;
            builder.AddItem(resetItem);

            var regenerateItem = new ModularPanel.ButtonItem("Regenerate");
            regenerateItem.onClick += OnRegenerateClicked;
            builder.AddItem(regenerateItem);

            builder.Build();
        }

        void UpdateGraphController()
        {
            var height = Camera.main.orthographicSize * 2;
            var width = height * Camera.main.aspect;

            var size = new Vector2(width, height);
            if (size != _previousSize)
            {
                _previousSize = size;

                _graphController.width = width;
                _graphController.height = height;

                var rows = _graphController.rows;
                var columns = (int)(rows * Camera.main.aspect);
                _graphController.columns = columns;

                _graphController.Refresh();
            }
        }

        void Update()
        {
            UpdateGraphController();
        }

        public void OnResetClicked()
        {
            _pathController.ResetState();
        }

        public void OnRegenerateClicked()
        {
            Generate();
        }

        void Generate()
        {
            IEnumerator command()
            {
                _worldGenerator.Generate();
                yield return new WaitForEndOfFrame();
                _graphController.Refresh();
                _pathController.ResetState();
            }

            StartCoroutine(command());
        }
    }
}
