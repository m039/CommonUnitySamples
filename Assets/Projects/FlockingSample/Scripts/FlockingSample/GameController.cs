using m039.Common.DependencyInjection;
using UnityEngine;

namespace Game.FlockingSample
{
    public class GameController : CoreGameController
    {
        #region Inspector

        [SerializeField]
        TMPro.TMP_Text _FPSCounter;

        #endregion

        [Inject]
        readonly FlockingManager _flocking;

        [Inject]
        readonly ModularPanel _modularPanel;

        float _fpsTimer = 0;

        void Start()
        {
            _flocking.CreateAgents();
            CreatePanel();
        }

        void CreatePanel()
        {
            if (_modularPanel == null)
                return;

            var builder = _modularPanel.CreateBuilder();

            // Neigbour Radius.
            var neghbourRadiusItem = new ModularPanel.SliderItem(_flocking.neighbourRadius, 0.1f, 2f);
            neghbourRadiusItem.label = "Neigbour Radius";
            neghbourRadiusItem.onValueChanged += (v) => _flocking.neighbourRadius = v;
            builder.AddItem(neghbourRadiusItem);

            // Aligment Coeff.
            var aligmentItem = new ModularPanel.SliderItem(_flocking.aligmentCoeff, 0f, 1f);
            aligmentItem.label = "Aligment Coeff";
            aligmentItem.onValueChanged += (v) => _flocking.aligmentCoeff = v;
            builder.AddItem(aligmentItem);

            // Aligment Coeff.
            var cohesionItem = new ModularPanel.SliderItem(_flocking.cohesionCoeff, 0f, 1f);
            cohesionItem.label = "Cohesion Coeff";
            cohesionItem.onValueChanged += (v) => _flocking.cohesionCoeff = v;
            builder.AddItem(cohesionItem);

            // Separation Coeff.
            var separationItem = new ModularPanel.SliderItem(_flocking.separationCoeff, 0f, 1f);
            separationItem.label = "Separation Coeff";
            separationItem.onValueChanged += (v) => _flocking.separationCoeff = v;
            builder.AddItem(separationItem);

            // Agent Speed.
            var agentSpeedItem = new ModularPanel.SliderItem(_flocking.movementSpeedMultiplier, 0.1f, 10f);
            agentSpeedItem.label = "Agent Speed";
            agentSpeedItem.onValueChanged += (v) => _flocking.movementSpeedMultiplier = v;
            builder.AddItem(agentSpeedItem);

            // Neighbours Mode
            var neighboursModeItem = new ModularPanel.DropdownEnumItem(typeof(FlockingManager.NeighboursMode));
            neighboursModeItem.value = (int)_flocking.neighboursMode;
            neighboursModeItem.onValueChanged += (v) => _flocking.neighboursMode = (FlockingManager.NeighboursMode)v;
            builder.AddItem(neighboursModeItem);

            // Debug Neighbours.
            var debugNeighboursItem = new ModularPanel.ToggleItem(false, "Debug Neighbours");
            debugNeighboursItem.onValueChanged += (v) => _flocking.SetDebugNeighbours(v);
            builder.AddItem(debugNeighboursItem);

            // Color By Neighbours
            var colorByNeighbours = new ModularPanel.ToggleItem(true, "Color By Neighbours");
            colorByNeighbours.onValueChanged += (v) => _flocking.SetColorByNeighbours(v);
            builder.AddItem(colorByNeighbours);

            // No Colorize.
            var noColorize = new ModularPanel.ToggleItem(false, "No Colorize");
            noColorize.onValueChanged += (v) =>
            {
                _flocking.SetColorize(!v);
                colorByNeighbours.visible = !v;
            };
            builder.AddItem(noColorize);

            builder.Build();
        }

        void Update()
        {
            ProcessDebug();
        }

        void ProcessDebug()
        {
            _fpsTimer -= Time.deltaTime;
            if (_fpsTimer < 0)
            {
                _FPSCounter.text = string.Format("FPS: {0,3:f2}", 1 / Time.deltaTime);
                _fpsTimer = 0.1f;
            }
        }
    }
}
