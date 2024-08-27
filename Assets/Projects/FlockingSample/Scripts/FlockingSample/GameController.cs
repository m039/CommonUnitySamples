using m039.Common.DependencyInjection;

namespace Game.FlockingSample
{
    public class GameController : CoreGameController
    {
        [Inject]
        readonly FlockingManager _flocking;

        [Inject]
        readonly ModularPanel _modularPanel;

        void Start()
        {
            CreatePanel();
        }

        void CreatePanel()
        {
            if (_modularPanel == null)
                return;

            var builder = _modularPanel.CreateBuilder();

            // Boids Count.
            var boidsCountItem = new ModularPanel.SliderItem(400f, 0f, 1000f);
            boidsCountItem.label = "Boids Count";
            boidsCountItem.valueFormat = "0";
            boidsCountItem.onValueChanged += (v) => _flocking.CreateOrRemoveAgents((int)v);
            builder.AddItem(boidsCountItem);

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
            var neighboursModeItem = new ModularPanel.DropdownEnumItem(typeof(FlockingManager.NeighboursMode), "Find Neighbours By");
            neighboursModeItem.value = (int)_flocking.neighboursMode;
            neighboursModeItem.onValueChanged += (v) => _flocking.neighboursMode = (FlockingManager.NeighboursMode)v;
            builder.AddItem(neighboursModeItem);

            // Debug Neighbours.
            var debugNeighboursItem = new ModularPanel.ToggleItem(_flocking.debugNeighbours, "Debug Neighbours");
            debugNeighboursItem.onValueChanged += (v) => _flocking.debugNeighbours = v;
            builder.AddItem(debugNeighboursItem);

            // Debug Grids.
            var debugGridsItem = new ModularPanel.ToggleItem(_flocking.debugGrids, "Debug Grids");
            debugGridsItem.onValueChanged += (v) => _flocking.debugGrids = v;
            builder.AddItem(debugGridsItem);

            // Color By Neighbours
            var colorByNeighbours = new ModularPanel.ToggleItem(_flocking.colorByNeighbours, "Color By Neighbours");
            colorByNeighbours.onValueChanged += (v) => _flocking.colorByNeighbours = v;
            builder.AddItem(colorByNeighbours);

            // No Colorize.
            var noColorize = new ModularPanel.ToggleItem(!_flocking.colorize, "No Colorize");
            noColorize.onValueChanged += (v) =>
            {
                _flocking.colorize = !v;
                colorByNeighbours.visible = !v;
            };
            builder.AddItem(noColorize);

            builder.Build();
        }
    }
}
