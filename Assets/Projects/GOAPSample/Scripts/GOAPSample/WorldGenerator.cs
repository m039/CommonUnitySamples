using Game.BehaviourTreeSample;
using m039.Common;
using m039.Common.Blackboard;
using m039.Common.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;

namespace Game.GOAPSample
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

        [SerializeField]
        MinMaxInt _ObstaclesCount = new(10, 20);

        #endregion

        [Inject]
        readonly IGameEntityFactory _entityFactory;

        [Inject]
        readonly GameUI _ui;

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

            void spawn(GameEntityType type, int count, System.Type typeClass, List< GameEntityType> checkTypes)
            {
                // Data needed for spawning entities with different type classes.
                List<(float, int)> spawnData; 
                if (typeClass == null)
                {
                    spawnData = new()
                    {
                        (_entityFactory.GetPrefab(type).spawnRadius, 0)
                    };
                } else
                {
                    spawnData = new();
                    foreach (var e in System.Enum.GetValues(typeClass))
                    {
                        var typeClassInt = (int)e;
                        spawnData.Add((_entityFactory.GetPrefab(type, typeClassInt).spawnRadius, typeClassInt));
                    }
                }

                for (int i = 0; i < count; i++)
                {
                    var position = Vector2.zero;
                    var validPosition = false;
                    var data = spawnData[Random.Range(0, spawnData.Count)];

                    for (int j = 0; j < 100; j++)
                    {
                        position = CameraUtils.RandomPositionOnScreen(data.Item1);
                        if (isOccupied(position, data.Item1, checkTypes))
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
                    _blackboard.SetValue(BlackboardKeys.TypeClass, data.Item2);
                    _entityFactory.Create(type, _blackboard);
                }
            }

            bool generate(List<Template> templates)
            {
                // Destroy previous entities.

                foreach (var template in templates)
                {
                    foreach (var entity in _entityFactory.GetEnteties(template.type).ToArray())
                    {
                        _entityFactory.Destroy(entity);
                    }
                }

                // Spawn new entities.

                var checkTypes = new List<GameEntityType>();

                foreach (var template in templates)
                {
                    if (template.checkCollisions)
                    {
                        checkTypes.Add(template.type);
                        spawn(template.type, template.count, template.typeClass, checkTypes);
                    }
                    else
                    {
                        spawnDefault(template.type, template.count);
                    }
                }

                // Check if all entities have been succesfully spawned.

                foreach (var template in templates)
                {
                    if (_entityFactory.GetEnteties(template.type).Count <= 0)
                    {
                        return false;
                    }
                }

                return true;
            }

            var templates = new List<Template>
            {
                new (GameEntityType.House, _HousesCount.Random()),
                new (GameEntityType.Forest, _ForestCount.Random()),
                new (GameEntityType.Glade, _GladesCount.Random()),
                new (GameEntityType.Troll, _TrollsCount.Random(), checkCollisions: false),
                new (GameEntityType.Obstacle, _ObstaclesCount.Random(), typeClass: typeof(ObstalceType))
            };

            int tries = 100;

            while (true)
            {
                if (generate(templates))
                {
                    return;
                }

                // Find a template with the highest count parameter.

                var highCountIndex = -1;
                Template template;

                for (int i = 0; i < templates.Count; i++)
                {
                    template = templates[i];
                    if ((template.count > highCountIndex || highCountIndex == -1) && template.count > 1 && template.checkCollisions)
                    {
                        highCountIndex = i;
                    }
                }

                if (highCountIndex == -1)
                {
                    tries--;
                    if (tries <= 0)
                    {
                        break;
                    } else
                    {
                        continue;
                    }
                }

                // Decrease count to make generation possible.

                template = templates[highCountIndex];
                template.count--;
                templates[highCountIndex] = template;
            }

            _ui.SetWarningNotification("Can't successfully generate the world");
        }

        struct Template
        {
            public GameEntityType type;
            public int count;
            public bool checkCollisions;
            public System.Type typeClass;

            public Template(GameEntityType type, int count, bool checkCollisions = true, System.Type typeClass = null)
            {
                this.type = type;
                this.count = count;
                this.checkCollisions = checkCollisions;
                this.typeClass = typeClass;
            }
        }
    }
}
