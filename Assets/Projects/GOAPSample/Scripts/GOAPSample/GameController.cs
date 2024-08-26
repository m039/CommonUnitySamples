using Game.BehaviourTreeSample;
using m039.Common.Blackboard;
using m039.Common.DependencyInjection;
using m039.Common.GOAP;
using m039.Common.Pathfindig;
using m039.Common.StateMachine;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.GOAPSample
{
    public class GameController : CoreGameController
    {
        static readonly Collider2D[] s_Buffer = new Collider2D[16];

        #region Inspector

        [SerializeField]
        GraphController _GraphController;

        #endregion

        [Inject]
        readonly GameUI _ui;

        [Inject]
        readonly FPSCounter _fpsCounter;

        [Inject]
        readonly WorldGenerator _worldGenerator;

        [Inject]
        readonly ModularPanel _modularPanel;

        IGameEntity _selectedBot;

        Vector2 _previousScreenSize;

        void Start()
        {
            ServiceLocator.Register(_GraphController.GetComponent<Seeker>());
            ServiceLocator.Register<IGraphController>(_GraphController);
            ServiceLocator.Register(GetComponentInChildren<PathSmoother>());

            GenerateWorld();
            CreatePanel();
        }

        void CreatePanel()
        {
            if (_modularPanel == null)
                return;

            var builder = _modularPanel.CreateBuilder();

            var enablePathSmootherItem = new ModularPanel.ToggleItem(true, "Enable Path Smoother");
            enablePathSmootherItem.onValueChanged += (v) => _GraphController.GetComponent<PathSmoother>().enabled = v;
            builder.AddItem(enablePathSmootherItem);

            var debugGraphControllerItem = new ModularPanel.ToggleItem(false, "Debug Graph Controller");
            debugGraphControllerItem.onValueChanged += (v) => _GraphController.GetComponent<GraphControllerDebugger>().enabled = v;
            builder.AddItem(debugGraphControllerItem);

            var debugPathfindingItem = new ModularPanel.ToggleItem(false, "Debug Pathfinding");
            debugPathfindingItem.onValueChanged += (v) => OnDebugPathfindingChanged(v);
            builder.AddItem(debugPathfindingItem);

            var debugModeItem = new ModularPanel.ToggleItem(true, "Debug Mode");
            debugModeItem.onValueChanged += (v) => OnDebugModeChanged(v);
            builder.AddItem(debugModeItem);

            var regenerateItem = new ModularPanel.ButtonItem("Regenerate");
            regenerateItem.onClick += GenerateWorld;
            builder.AddItem(regenerateItem);

            builder.Build();
        }

        void Update()
        {
            ProcessInput();
            ProcessBotInfo();
            UpdateGraphController();
        }

        void ProcessInput()
        {
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                Vector2 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var count = Physics2D.OverlapCircleNonAlloc(position, 0.1f, s_Buffer);
                IGameEntity bot = null;

                for (int i = 0; i < count; i++)
                {
                    if (s_Buffer[i].GetComponentInParent<IGameEntity>() is IGameEntity gameEntity)
                    {
                        if (bot == null && gameEntity.type == GameEntityType.Bot)
                        {
                            bot = gameEntity;
                            break;
                        }
                    }
                }

                if (_selectedBot != null && _selectedBot == bot)
                {
                    ForgetSelectedBot();
                }
                else if (bot != null)
                {
                    ForgetSelectedBot();
                    _selectedBot = bot;
                    _selectedBot.locator.Get<BlackboardBase>().SetValue(BlackboardKeys.Selection, true);
                }
            }
        }

        void ForgetSelectedBot()
        {
            _selectedBot?.locator.Get<BlackboardBase>().Remove(BlackboardKeys.Selection);
            _selectedBot = null;
        }

        void ProcessBotInfo()
        {
            if (_selectedBot == null)
            {
                _ui.botInfo.gameObject.SetActive(false);
                return;
            }
            _ui.botInfo.gameObject.SetActive(true);

            object getValue(BlackboardEntry value)
            {
                var v = value.GetValue();
                if (v is ICollection collection)
                {
                    return $"{v.GetType().Name}[{collection.Count}]";
                }
                else if (v is IGameEntity gameEntity)
                {
                    return gameEntity.name;
                }
                return v;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"<b>{_selectedBot.name}</b>");
            if (_selectedBot.locator.TryGet(out BlackboardBase blackboard) && blackboard is GameBlackboard gameBlackboard)
            {
                sb.AppendLine("Blackboard:");
                foreach (var entities in gameBlackboard)
                {
                    sb.AppendLine($"  {entities.Key}: {getValue(entities.Value)}");
                }
            }
            if (_selectedBot.locator.TryGet(out StateMachine stateMachine))
            {
                sb.AppendLine();
                sb.AppendLine("StateMachine:");
                string currentStateName;
                if (stateMachine.CurrentState is MonoBehaviour monoBahaviour)
                {
                    currentStateName = monoBahaviour.name;
                }
                else
                {
                    currentStateName = stateMachine.CurrentState?.ToString();
                }
                sb.AppendLine($"  CurrentState: {currentStateName}");
            }
            if (_selectedBot.locator.TryGet(out Agent agent))
            {
                sb.AppendLine();
                sb.AppendLine("GOAP:");
                agent.Print(2, sb);
            }
            _ui.botInfo.text = sb.ToString();
        }

        void OnDebugModeChanged(bool value)
        {
            Blackboard.SetValue(BlackboardKeys.DebugMode, value);
            _fpsCounter.gameObject.SetActive(value);
        }

        void OnDebugPathfindingChanged(bool value)
        {
            Blackboard.SetValue(BlackboardKeys.DebugPathfinding, value);
        }

        void GenerateWorld()
        {
            IEnumerator command()
            {
                _ui.ClearWarningNotification();
                ForgetSelectedBot();
                _worldGenerator.GenerateWorld();
                yield return new WaitForFixedUpdate();
                _GraphController.Refresh();
            }

            StartCoroutine(command());
        }

        void UpdateGraphController()
        {
            var height = Camera.main.orthographicSize * 2;
            var width = height * Camera.main.aspect;

            var size = new Vector2(width, height);
            if (size != _previousScreenSize)
            {
                _previousScreenSize = size;

                _GraphController.width = width;
                _GraphController.height = height;

                var rows = _GraphController.rows;
                var columns = (int)(rows * Camera.main.aspect);
                _GraphController.columns = columns;

                _GraphController.Refresh();
            }
        }
    }
}
