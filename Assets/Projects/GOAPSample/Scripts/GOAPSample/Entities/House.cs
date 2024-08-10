using Game.BehaviourTreeSample;
using m039.Common.Blackboard;
using System.Collections;
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

        protected override void OnCreateEntity(BlackboardBase blackboard)
        {
            base.OnCreateEntity(blackboard);

            IEnumerator createBonfire()
            {
                yield return new WaitForFixedUpdate();

                var factory = CoreGameController.Instance.ServiceLocator.Get<IGameEntityFactory>();
                var bonfireSpawnRadius = factory.GetPrefab(GameEntityType.Bonfire).spawnRadius;

                for (int i = 0; i < 100; i++)
                {
                    var point = position + UnityEngine.Random.insideUnitCircle * this.spawnRadius;
                    var rect = new Rect(point.x - bonfireSpawnRadius, point.y - bonfireSpawnRadius, bonfireSpawnRadius * 2, bonfireSpawnRadius * 2);

                    if (_Rigidbody.OverlapPoint(rect.min) || _Rigidbody.OverlapPoint(rect.max))
                        continue;

                    var b = new Blackboard();
                    b.SetValue(BlackboardKeys.Position, point);
                    _bonfire = factory.Create(GameEntityType.Bonfire, b);
                    break;
                }
            }

            StartCoroutine(createBonfire());
        }

        protected override void OnDestroyEntity()
        {
            base.OnDestroyEntity();

            _bonfire.Destroy();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, _SpawnRadius);
        }
    }
}
