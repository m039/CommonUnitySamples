using m039.Common;
using m039.Common.Blackboard;
using m039.Common.Events;
using UnityEngine;

namespace Game.BehaviourTreeSample
{
    public interface IFoodEater
    {
        void Eat(IGameEntity food);
    }

    public class Food : MonoBehaviour, IGameEntity
    {
        public int id { get; private set; } = 0;
        public Vector2 position {
            get
            {
                return transform.position;
            }

            set
            {
                var p = transform.position;
                p.x = value.x;
                p.y = value.y;
                transform.position = p;
            }
        }

        public GameEntityType type => GameEntityType.Food;

        public bool IsAlive { get; set; }

        public ServiceLocator locator => _serviceLocator;

        bool _created;

        readonly ServiceLocator _serviceLocator = new();

        public void OnCreate(Blackboard blackboard)
        {
            if (blackboard.TryGetValue(BlackboardKeys.Id, out int _id))
            {
                id = _id;
            }

            if (blackboard.TryGetValue(BlackboardKeys.Position, out Vector2 _position))
            {
                position = _position;
            }

            _created = true;
        }

        void Start()
        {
            if (!_created)
            {
                CoreGameController.Instance.ServiceLocator.Get<IGameEntityFactory>()?.CreateManually(this);
            }
        }

        void IGameEntity.OnDestroy()
        {
        }

        void OnTriggerEnter2D(Collider2D collider)
        {
            var gameEntity = collider.GetComponentInParent<IGameEntity>();
            if (gameEntity != null && gameEntity.locator.TryGet(out IFoodEater foodEater))
            {
                foodEater.Eat(this);
                this.Destroy();
            }
        }
    }
}