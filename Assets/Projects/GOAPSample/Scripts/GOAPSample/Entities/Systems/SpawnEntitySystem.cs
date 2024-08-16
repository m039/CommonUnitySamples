using Game.BehaviourTreeSample;
using Game.GOAPSample;
using m039.Common;
using m039.Common.Blackboard;
using m039.Common.Events;
using UnityEngine;

namespace Game
{
    public class SpawnEntitySystem : EntitySystem, IOnChildRemoved
    {
        #region Inspector

        [SerializeField]
        GameEntityType _TypeToSpawn;

        [SerializeField]
        MinMaxFloat _Cooldown = new(8f, 16f);

        #endregion

        CountdownTimer _timer;

        protected override void OnInit()
        {
            base.OnInit();

            gameEntity.locator.Get<EventBusByInterface>().Subscribe(this);
        }

        protected override void OnDeinit()
        {
            base.OnDeinit();

            gameEntity.locator.Get<EventBusByInterface>().Unsubscribe(this);
        }

        void SpawnObject()
        {
            var blackboard = gameEntity.GetBlackboard();

            if (!blackboard.TryGetValue(BlackboardKeys.Childs, out var childs)) {
                return;
            }

            if (!blackboard.TryGetValue(BlackboardKeys.MaxChilds, out var maxChilds))
            {
                return;
            }

            if (childs.Count >= maxChilds)
                return;

            var factory = CoreGameController.Instance.ServiceLocator.Get<IGameEntityFactory>();

            var position = gameEntity.position + Random.insideUnitCircle * gameEntity.spawnRadius;

            var b = new Blackboard();
            b.SetValue(BlackboardKeys.Position, position);
            var e = factory.Create(_TypeToSpawn, b);

            childs.Add(e);

            if (childs.Count < maxChilds) {
                ResetTimer();
            } else
            {
                _timer = null;
            }
        }

        public override void Process(float deltaTime)
        {
            base.Process(deltaTime);

            if (_timer != null)
            {
                _timer.Tick(deltaTime);
            }
        }

        public void OnChildRemoved(IGameEntity child)
        {
            if (_timer != null)
                return;

            ResetTimer();
        }

        void ResetTimer()
        {
            _timer = new CountdownTimer(_Cooldown.Random());
            _timer.onStop += () =>
            {
                SpawnObject();
            };
            _timer.Start();
        }
    }
}
