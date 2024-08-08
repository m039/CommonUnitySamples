using m039.Common;
using m039.Common.AI;
using m039.Common.BehaviourTrees;
using m039.Common.Blackboard;
using m039.Common.DependencyInjection;
using m039.Common.StateMachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.BehaviourTreeSample
{
    public class GameController : CoreGameController, IFoodEatenEvent
    {
        static readonly Collider2D[] s_Buffer = new Collider2D[16];

        #region Inspector

        [SerializeField]
        MinMaxFloat _SpawnFoodTimer = new(2, 3);

        [SerializeField]
        MinMaxInt _SpawnFoodAmount = new(1, 3);

        [SerializeField]
        MinMaxInt _SpawnBotAmount = new(1, 3);

        [SerializeField]
        TMPro.TMP_Text _GroupInfo;

        [SerializeField]
        TMPro.TMP_Text _FPSCounter;

        [SerializeField]
        TMPro.TMP_Text _BotInfo;

        #endregion

        [Inject]
        readonly IGameEntityFactory _entityFactory;

        readonly BlackboardBase _blackboard = new GameBlackboard();

        readonly Arbiter _arbiter = new();

        Coroutine _spawner;

        float _fpsTimer = 0;

        IGameEntity _selectedBot;

        protected override void DoAwake()
        {
            base.DoAwake();

            ServiceLocator.Register(_arbiter);
            EventBus.Subscribe(this);
        }

        protected override void DoDestroy()
        {
            base.DoDestroy();

            EventBus.Unsubscribe(this);
        }

        readonly Dictionary<BotClass, BlackboardBase> _groupBlackboards = new();

        void Start()
        {
            EventBus.Logger.SetEnabled(false);

            var botClasses = System.Enum.GetValues(typeof(BotClass)).Cast<BotClass>().ToList();
            for (int i = 0; i < botClasses.Count; i++)
            {
                _groupBlackboards.Add(botClasses[i], new GameBlackboard());
            }

            var count = _SpawnBotAmount.Random();
            for (int i = 0; i < count; i++)
            {
                var botClass = botClasses[Random.Range(0, botClasses.Count)];

                _blackboard.Clear();
                _blackboard.SetValue(BlackboardKeys.Position, CameraUtils.RandomPositionOnScreen());
                _blackboard.SetValue(BlackboardKeys.GroupBlackboard, _groupBlackboards[botClass]);
                _blackboard.SetValue(BlackboardKeys.TypeClass, (int) botClass);
                _entityFactory.Create(GameEntityType.Bot, _blackboard);
            }

            UpdateGroupInfo();
            _FPSCounter.gameObject.SetActive(false);
        }

        void Update()
        {
            ProcessInput();
            ProcessSpawner();
            ProcessDebug();
            ProcessBotInfo();
        }

        private void LateUpdate()
        {
            _arbiter.Iteration();
        }

        void ProcessSpawner()
        {
            if (_entityFactory.GetEnteties(GameEntityType.Food).Count <= 0 && _spawner == null)
            {
                _spawner = StartCoroutine(StartSpawnerCoroutine());
            }
        }

        void ProcessInput()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                SpawnFoodAtRandomLocation();
                return;
            }

            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                Vector2 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var count = Physics2D.OverlapCircleNonAlloc(position, 0.1f, s_Buffer);
                IGameEntity food = null;
                IGameEntity bot = null;

                for (int i = 0; i < count; i++)
                {
                    if (s_Buffer[i].GetComponentInParent<IGameEntity>() is IGameEntity gameEntity)
                    {
                        if (food == null && gameEntity.type == GameEntityType.Food)
                        {
                            food = gameEntity;
                        }

                        if (bot == null && gameEntity.type == GameEntityType.Bot)
                        {
                            bot = gameEntity;
                        }

                        if (bot != null && food != null)
                        {
                            break;
                        }
                    }
                }

                void unselectCurrentBot()
                {
                    _selectedBot?.locator.Get<BlackboardBase>().Remove(BlackboardKeys.Selection);
                    _selectedBot = null;
                }

                if (_selectedBot != null && _selectedBot == bot)
                {
                    unselectCurrentBot();
                    return;
                } else if (bot != null)
                {
                    unselectCurrentBot();
                    _selectedBot = bot;
                    _selectedBot.locator.Get<BlackboardBase>().SetValue(BlackboardKeys.Selection, true);
                    return;
                }

                if (food == null)
                {
                    _blackboard.Clear();
                    _blackboard.SetValue(BlackboardKeys.Position, position);
                    _entityFactory.Create(GameEntityType.Food, _blackboard);
                } else
                {
                    _entityFactory.Destroy(food);
                }
            }
        }

        void ProcessDebug()
        {
            if (Blackboard.GetValue(BlackboardKeys.DebugMode))
            {
                _fpsTimer -= Time.deltaTime;
                if (_fpsTimer < 0)
                {
                    _FPSCounter.text = string.Format("FPS: {0,3:f2}", 1 / Time.deltaTime);
                    _fpsTimer = 0.1f;
                }
            }
        }

        void ProcessBotInfo()
        {
            if (_selectedBot == null)
            {
                _BotInfo.gameObject.SetActive(false);
                return;
            }
            _BotInfo.gameObject.SetActive(true);

            object getValue(BlackboardEntry value)
            {
                var v = value.GetValue();
                if (v is ICollection collection)
                {
                    return $"{v.GetType().Name}[{collection.Count}]";
                } else if (v is IGameEntity gameEntity)
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
            if (_selectedBot.locator.TryGet(out BehaviourTree behaviourTree)) {
                sb.AppendLine();
                sb.AppendLine("BehaviourTree:");
                behaviourTree.PrintTree(2, sb);
            }
            _BotInfo.text = sb.ToString();
        }

        IEnumerator StartSpawnerCoroutine()
        {
            yield return new WaitForSeconds(_SpawnFoodTimer.Random());

            var count = _SpawnFoodAmount.Random();
            for (int i = 0; i < count; i++)
            {
                SpawnFoodAtRandomLocation();
            }
            _spawner = null;
        }

        void UpdateGroupInfo()
        {
            var sb = new StringBuilder();

            sb.AppendLine("Eaten food:");
            foreach (var (botClass, foodEaten) in _groupBlackboards
                .Select(x => (x.Key, x.Value.GetValue(BlackboardKeys.EatenFood, 0)))
                .OrderByDescending(x => x.Item2))
            {
                sb.AppendLine("  " + botClass + " = " + foodEaten);
            }

            _GroupInfo.text = sb.ToString();
        }

        void SpawnFoodAtRandomLocation()
        {
            _blackboard.Clear();
            _blackboard.SetValue(BlackboardKeys.Position, CameraUtils.RandomPositionOnScreen());
            _entityFactory.Create(GameEntityType.Food, _blackboard);
        }

        public void FoodEaten(IGameEntity eater, IGameEntity food)
        {
            UpdateGroupInfo();
        }

        public void OnDebugModeChanged(bool debugMode)
        {
            Blackboard.UpdateValue(BlackboardKeys.DebugMode, x => !x);
            _FPSCounter.gameObject.SetActive(debugMode);
        }
    }
}
