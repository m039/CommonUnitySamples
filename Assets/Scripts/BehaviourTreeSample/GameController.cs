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
        float _SpawnFoodTimer = 2;

        [SerializeField]
        MinMaxInt _SpawnFoodAmount = new(1, 3);

        #endregion

        [Inject]
        readonly IGameEntityFactory _entityFactory;

        readonly Blackboard _blackboard = new();

        Coroutine _spawner;

        private void Start()
        {
            EventBus.Logger.SetEnabled(false);
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
                    _entityFactory.Create<Food>(_blackboard);
                } else
                {
                    _entityFactory.Destroy(food);
                }
            }
        }

        IEnumerator StartSpawnerCoroutine()
        {
            yield return new WaitForSeconds(_SpawnFoodTimer);

            var count = _SpawnFoodAmount.Random();
            for (int i = 0; i < count; i++)
            {
                SpawnFoodAtRandomLocation();
            }
            _spawner = null;
        }

        void SpawnFoodAtRandomLocation()
        {
            var height = Camera.main.orthographicSize * 2;
            var width = height * Camera.main.aspect;
            Vector2 position = (Vector2)Camera.main.transform.position +
                new Vector2(Random.Range(-width / 2f, width / 2f), Random.Range(-height / 2f, height / 2f));

            _blackboard.Clear();
            _blackboard.SetValue(BlackboardKeys.Position, position);
            _entityFactory.Create<Food>(_blackboard);
        }
    }
}
