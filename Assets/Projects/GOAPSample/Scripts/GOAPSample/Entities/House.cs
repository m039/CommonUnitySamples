using Game.BehaviourTreeSample;
using m039.Common;
using m039.Common.Blackboard;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GOAPSample
{
    public class House : GameEntity
    {
        #region Inspector

        [SerializeField]
        float _SpawnRadius = 1f;

        [SerializeField]
        Transform _Pivot;

        [SerializeField]
        Rigidbody2D _Rigidbody;

        [SerializeField]
        MinMaxInt _BotsCount = new(1, 2);

        #endregion

        public override Vector2 position
        {
            set
            {
                var p = transform.position;
                p.x = value.x;
                p.y = value.y;
                transform.position = p;

                var y = _Pivot.position.y;
                p.z = Mathf.InverseLerp(-100, 100, y) * 100;
                transform.position = p;
            }
        }

        public override float spawnRadius => _SpawnRadius;

        public override GameEntityType type => GameEntityType.House;

        IGameEntity _bonfire;

        readonly List<IGameEntity> _bots = new();

        Coroutine _createCoroutine;

        protected override void OnCreateEntity(BlackboardBase blackboard)
        {
            base.OnCreateEntity(blackboard);

            IEnumerator createEntities()
            {
                yield return new WaitForFixedUpdate();

                // Create bonfire.

                var factory = CoreGameController.Instance.ServiceLocator.Get<IGameEntityFactory>();
                var bonfireSpawnRadius = factory.GetPrefab(GameEntityType.Bonfire).spawnRadius;
                var b = new Blackboard();

                for (int i = 0; i < 100; i++)
                {
                    var point = position + UnityEngine.Random.insideUnitCircle * this.spawnRadius;
                    var rect = new Rect(point.x - bonfireSpawnRadius, point.y - bonfireSpawnRadius, bonfireSpawnRadius * 2, bonfireSpawnRadius * 2);

                    if (_Rigidbody.OverlapPoint(rect.min) || _Rigidbody.OverlapPoint(rect.max))
                        continue;

                    b.Clear();
                    b.SetValue(BlackboardKeys.Position, point);
                    _bonfire = factory.Create(GameEntityType.Bonfire, b);
                    break;
                }

                // Create bots.

                var botsCount = _BotsCount.Random();

                for (int i = 0; i < botsCount; i++)
                {
                    for (int j = 0; j < 100; j++)
                    {
                        var point = position + UnityEngine.Random.insideUnitCircle * this.spawnRadius;

                        if (_Rigidbody.OverlapPoint(point))
                            continue;

                        b.Clear();
                        b.SetValue(BlackboardKeys.Position, point);
                        _bots.Add(factory.Create(GameEntityType.Bot, b));
                        break;
                    }
                }
            }

            _createCoroutine = StartCoroutine(createEntities());
        }

        protected override void OnDestroyEntity()
        {
            base.OnDestroyEntity();

            if (_createCoroutine != null)
            {
                StopCoroutine(_createCoroutine);
                _createCoroutine = null;
            }

            _bonfire?.Destroy();

            foreach (var bot in _bots)
            {
                bot.Destroy();
            }

            _bots.Clear();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, _SpawnRadius);
        }
    }
}
