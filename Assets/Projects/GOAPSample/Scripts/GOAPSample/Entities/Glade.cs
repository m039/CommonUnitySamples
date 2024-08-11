using Game.BehaviourTreeSample;
using m039.Common.Blackboard;
using m039.Common;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GOAPSample
{
    public class Glade : GameEntity
    {
        #region Inspector

        [SerializeField]
        float _SpawnRadius = 1f;

        [SerializeField]
        MinMaxInt _MushroomsToSpawn = new(4, 6);

        #endregion

        public override GameEntityType type => GameEntityType.Glade;

        public override float spawnRadius => _SpawnRadius;

        readonly List<IGameEntity> _mushrooms = new();

        protected override void OnCreateEntity(BlackboardBase blackboard)
        {
            base.OnCreateEntity(blackboard);

            var count = _MushroomsToSpawn.Random();
            var factory = CoreGameController.Instance.ServiceLocator.Get<IGameEntityFactory>();
            var b = new Blackboard();
            for (int i = 0; i < count; i++)
            {
                var point = position + Random.insideUnitCircle * spawnRadius;
                b.Clear();
                b.SetValue(BlackboardKeys.Position, point);
                _mushrooms.Add(factory.Create(GameEntityType.Mushroom, b));
            }
        }

        protected override void OnDestroyEntity()
        {
            base.OnDestroyEntity();

            foreach (var t in _mushrooms)
            {
                t.Destroy();
            }

            _mushrooms.Clear();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, _SpawnRadius);
        }
    }
}