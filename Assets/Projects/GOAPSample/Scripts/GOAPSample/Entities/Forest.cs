using Game.BehaviourTreeSample;
using m039.Common;
using m039.Common.Blackboard;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GOAPSample
{
    public class Forest : GameEntity, IGameEntityDestroyedEvent
    {
        #region Inspector

        [SerializeField]
        float _SpawnRadius = 1f;

        [SerializeField]
        MinMaxInt _TreesToSpawn = new(4, 6);

        #endregion

        public override GameEntityType type => GameEntityType.Forest;

        public override float spawnRadius => _SpawnRadius;

        readonly List<IGameEntity> _trees = new();

        protected override void OnCreateEntity(BlackboardBase blackboard)
        {
            base.OnCreateEntity(blackboard);

            var count = _TreesToSpawn.Random();
            var factory = CoreGameController.Instance.ServiceLocator.Get<IGameEntityFactory>();
            var b = new Blackboard();
            for (int i = 0; i < count; i++)
            {
                var point = position + Random.insideUnitCircle * spawnRadius;
                b.Clear();
                b.SetValue(BlackboardKeys.Position, point);
                _trees.Add(factory.Create(GameEntityType.Tree, b));
            }

            Blackboard.SetValue(BlackboardKeys.MaxChilds, count);
            Blackboard.SetValue(BlackboardKeys.Childs, _trees);

            CoreGameController.Instance.EventBus.Subscribe(this);
        }

        protected override void OnDestroyEntity()
        {
            base.OnDestroyEntity();

            CoreGameController.Instance.EventBus.Unsubscribe(this);

            foreach (var t in _trees)
            {
                t.Destroy();
            }

            _trees.Clear();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, _SpawnRadius);
        }

        public void OnGameEntityDestroyed(IGameEntity gameEntity)
        {
            if (gameEntity.type == GameEntityType.Tree)
            {
                if (_trees.Remove(gameEntity))
                {
                    EventBus.Raise<IOnChildRemoved>(a => a.OnChildRemoved(gameEntity));
                }
            }
        }
    }
}
