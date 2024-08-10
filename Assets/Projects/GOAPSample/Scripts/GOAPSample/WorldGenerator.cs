using Game.BehaviourTreeSample;
using m039.Common;
using m039.Common.Blackboard;
using m039.Common.DependencyInjection;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Game
{
    public class WorldGenerator : MonoBehaviour, IDependencyProvider
    {
        #region Inspector

        [SerializeField]
        MinMaxInt _HousesCount = new(5, 10);

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
            var entities = _entityFactory.GetEnteties(GameEntityType.House);
            foreach (var entity in entities.ToArray())
            {
                _entityFactory.Destroy(entity);
            }

            var count = _HousesCount.Random();
            var spawnRadius = _entityFactory.GetPrefab(GameEntityType.House).spawnRadius;

            static bool isOccupied(Vector2 position, float spawnRadius)
            {
                var count = Physics2D.OverlapCircleNonAlloc(position, spawnRadius, s_Buffer);
                for (int i = 0; i < count; i++)
                {
                    if (s_Buffer[i].GetComponentInParent<IGameEntity>() is IGameEntity gameEntity)
                    {
                        if (gameEntity.spawnRadius > 0)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            for (int i = 0; i < count; i++)
            {
                var position = Vector2.zero;
                var validPosition = false;

                for (int j = 0; j < 100; j++)
                {
                    position = CameraUtils.RandomPositionOnScreen();
                    if (isOccupied(position, spawnRadius))
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
                _entityFactory.Create(GameEntityType.House, _blackboard);

                yield return null;
            }
        }
    }
}
