using Game.BehaviourTreeSample;
using m039.Common;
using m039.Common.Blackboard;
using m039.Common.DependencyInjection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game
{
    public class WorldGenerator : MonoBehaviour, IDependencyProvider
    {
        #region Inspector

        [SerializeField]
        MinMaxInt _HousesCount = new(5, 10);

        [SerializeField]
        MinMaxInt _ForestCount = new(5, 10);

        [SerializeField]
        MinMaxInt _GladesCount = new(5, 10);

        #endregion

        [Inject]
        readonly IGameEntityFactory _entityFactory;

        [Provide]
        WorldGenerator GetWorldGenerator()
        {
            return this;
        }

        readonly BlackboardBase _blackboard = new GameBlackboard();

        Coroutine _generateWorldCoroutine;

        public void GenerateWorld()
        {
            if (_generateWorldCoroutine != null)
            {
                StopCoroutine(_generateWorldCoroutine);
                _generateWorldCoroutine = null;
            }

            _generateWorldCoroutine = StartCoroutine(GenerateWorldCoroutine());
        }

        static readonly Collider2D[] s_Buffer = new Collider2D[32];

        IEnumerator GenerateWorldCoroutine()
        {
            bool isOccupied(Vector2 position, float spawnRadius, List<GameEntityType> checkTypes)
            {
                foreach (var type in checkTypes)
                {
                    foreach (var entity in _entityFactory.GetEnteties(type))
                    {
                        if (PhysicUtils.OverlapCircles(position, spawnRadius, entity.position, entity.spawnRadius))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            IEnumerator spawn(GameEntityType type, int count, List<GameEntityType> checkTypes)
            {
                var spawnRadius = _entityFactory.GetPrefab(type).spawnRadius;

                for (int i = 0; i < count; i++)
                {
                    var position = Vector2.zero;
                    var validPosition = false;

                    for (int j = 0; j < 100; j++)
                    {
                        position = CameraUtils.RandomPositionOnScreen(spawnRadius);
                        if (isOccupied(position, spawnRadius, checkTypes))
                        {
                            continue;
                        }
                        validPosition = true;
                        break;
                    }

                    if (!validPosition)
                        continue;

                    _blackboard.Clear();
                    _blackboard.SetValue(BlackboardKeys.Position, position);
                    _entityFactory.Create(type, _blackboard);

                    yield return null;
                }
            }

            var templates = new List<(GameEntityType, int)>
            {
                (GameEntityType.House, _HousesCount.Random()),
                (GameEntityType.Forest, _ForestCount.Random()),
                (GameEntityType.Glade, _GladesCount.Random())
            };

            // Destroy previous entities.

            foreach (var (type, _) in templates)
            {
                foreach (var entity in _entityFactory.GetEnteties(type).ToArray())
                {
                    _entityFactory.Destroy(entity);
                }
            }

            // Spawn new entities.

            var checkTypes = new List<GameEntityType>();

            foreach (var (type, count) in templates)
            {
                checkTypes.Add(type);
                yield return spawn(type, count, checkTypes);
            }
        }
    }
}
