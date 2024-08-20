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

        Vector2 _previousSize;

        void Start()
        {
            UpdateGraphController();
            Generate();
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
