using m039.Common;
using m039.Common.Blackboard;
using m039.Common.DependencyInjection;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Game.BehaviourTreeSample
{
    public class GameController : CoreGameController
    {
        static readonly Collider2D[] s_Buffer = new Collider2D[16];

        #region Inspector

        [SerializeField]
        MinMaxFloat _SpawnFoodTimer = new(2, 3);

        [SerializeField]
        MinMaxInt _SpawnFoodAmount = new(1, 3);

        [SerializeField]
        MinMaxInt _SpawnBotAmount = new(1, 3);

        #endregion

        [Inject]
        readonly IGameEntityFactory _entityFactory;

        readonly Blackboard _blackboard = new();

        Coroutine _spawner;

        private void Start()
        {
            EventBus.Logger.SetEnabled(false);

            var botClasses = System.Enum.GetValues(typeof(BotClass)).Cast<BotClass>().ToList();

            var count = _SpawnBotAmount.Random();
            for (int i = 0; i < count; i++)
            {
                _blackboard.Clear();
                _blackboard.SetValue(BlackboardKeys.Position, CameraUtils.RandomPositionOnScreen());
                _blackboard.SetValue(BlackboardKeys.TypeClass, botClasses[Random.Range(0, botClasses.Count)]);
                _entityFactory.Create(GameEntityType.Bot, _blackboard);
            }
        }

        private void Update()
        {
            ProcessInput();
            ProcessSpawner();
        }

        void ProcessSpawner()
        {
            if (!_entityFactory.GetEnteties(GameEntityType.Food).Any() && _spawner == null)
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

            if (Input.GetMouseButtonDown(0))
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

        void SpawnFoodAtRandomLocation()
        {
            _blackboard.Clear();
            _blackboard.SetValue(BlackboardKeys.Position, CameraUtils.RandomPositionOnScreen());
            _entityFactory.Create(GameEntityType.Food, _blackboard);
        }
    }
}
