using Game.BehaviourTreeSample;
using m039.Common;
using m039.Common.Blackboard;
using UnityEngine;

namespace Game.GOAPSample
{
    public abstract class GameEntity : MonoBehaviour, IGameEntity
    {
        public int id { get; private set; }

        public virtual float spawnRadius => 0;

        public virtual Vector2 position
        {
            get
            {
                return transform.position;
            }

            set
            {
                var p = transform.position;
                p.x = value.x;
                p.y = value.y;
                p.z = Mathf.InverseLerp(-100, 100, value.y) * 100;
                transform.position = p;
            }
        }

        public abstract GameEntityType type { get; }

        public virtual int typeClass => 0;

        public ServiceLocator locator
        {
            get
            {
                if (_serviceLocator == null)
                {
                    _serviceLocator = new();
                    OnCreateServiceLocator(_serviceLocator);
                }

                return _serviceLocator;
            }
        }
        public bool IsAlive { get; set; } = false;

        ServiceLocator _serviceLocator;

        protected readonly BlackboardBase Blackboard = new GameBlackboard();

        protected virtual void OnCreateServiceLocator(ServiceLocator serviceLocator)
        {
            _serviceLocator.Register(Blackboard);
        }

        string IGameEntity.name => $"{type}#{id}";

        void IGameEntity.OnCreate(BlackboardBase blackboard)
        {
            if (blackboard.TryGetValue(BlackboardKeys.Id, out var id))
            {
                this.id = id;
            }

            if (blackboard.TryGetValue(BlackboardKeys.Position, out var position))
            {
                this.position = position;
            }

            OnCreateEntity(blackboard);
        }

        protected virtual void OnCreateEntity(BlackboardBase blackboard)
        {
        }

        void IGameEntity.OnDestroy()
        {
            OnDestroyEntity();
        }

        protected virtual void OnDestroyEntity()
        {
        }
    }
}
