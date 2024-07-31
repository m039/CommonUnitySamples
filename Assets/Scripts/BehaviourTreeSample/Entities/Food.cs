using m039.Common.Blackboard;
using UnityEngine;

namespace Game.BehaviourTreeSample
{
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

        bool _created;

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
    }
}
