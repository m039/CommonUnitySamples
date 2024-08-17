using Game.BehaviourTreeSample;
using m039.Common;
using m039.Common.Blackboard;
using m039.Common.DependencyInjection;
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

        [SerializeField]
        MinMaxInt _TrollsCount = new(5, 10);

        #endregion

        [Inject]
        readonly IGameEntityFactory _entityFactory;

        [Provide]
        WorldGenerator GetWorldGenerator()
        {
            return this;
        }

        readonly BlackboardBase _blackboard = new GameBlackboard();

        public void GenerateWorld()
        {
             GenerateWorldInternal();
        }

        static readonly Collider2D[] s_Buffer = new Collider2D[32];

        void GenerateWorldInternal()
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

            void spawnDefault(GameEntityType type, int count)
            {
                for (int i = 0; i < count; i++)
                {
                    var position = CameraUtils.RandomPositionOnScreen();

                    _blackboard.Clear();
                    _blackboard.SetValue(BlackboardKeys.Position, position);
                    _entityFactory.Create(type, _blackboard);
                }
            }

            void spawn(GameEntityType type, int count, List<GameEntityType> checkTypes)
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
                }
            }

            bool generate()
            {
                var templates = new List<(GameEntityType, int, bool)>
                {
                    (GameEntityType.House, _HousesCount.Random(), true),
                    (GameEntityType.Forest, _ForestCount.Random(), true),
                    (GameEntityType.Glade, _GladesCount.Random(), true),
                    (GameEntityType.Troll, _TrollsCount.Random(), false)
                };

                // Destroy previous entities.

                foreach (var (type, _, _) in templates)
                {
                    foreach (var entity in _entityFactory.GetEnteties(type).ToArray())
                    {
                        _entityFactory.Destroy(entity);
                    }
                }

                // Spawn new entities.

                var checkTypes = new List<GameEntityType>();

                foreach (var (type, count, checkCollisions) in templates)
                {
                    if (checkCollisions)
                    {
                        checkTypes.Add(type);
                        spawn(type, count, checkTypes);
                    }
                    else
                    {
                        spawnDefault(type, count);
                    }
                }

                // Check if all entities have been succesfully spawned.

                foreach (var (type, _, _) in templates)
                {
                    if (_entityFactory.GetEnteties(type).Count <= 0)
                    {
                        return false;
                    }
                }

                return true;
            }

            for (int i = 0; i < 10; i++)
            {
                if (generate())
                {
                    return;
                }
            }

            Debug.LogError("Can't successfully generate the world. Adjust the generation parameters.");
        }
    }
}
