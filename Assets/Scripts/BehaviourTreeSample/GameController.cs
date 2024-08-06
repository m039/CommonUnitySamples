using m039.Common;
using m039.Common.AI;
using m039.Common.Blackboard;
using m039.Common.DependencyInjection;
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

        #endregion

        [Inject]
        readonly IGameEntityFactory _entityFactory;

        readonly BlackboardBase _blackboard = new GameBlackboard();

        readonly Arbiter _arbiter = new();

        Coroutine _spawner;

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
        }

        void Update()
        {
            ProcessInput();
            ProcessSpawner();
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
                var count = Physics2D.OverlapCircleNonAlloc(position, 0, s_Buffer);
                Food food = null;

                for (int i = 0; i < count; i++)
                {
                    if (s_Buffer[i].GetComponentInParent<Food>() is Food ge)
                    {
                        food = ge;
                        break;
                    }
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
        }
    }
}
