using m039.Common.Blackboard;
using m039.Common.StateMachine;
using UnityEngine;

namespace Game.BehaviourTreeSample
{
    public class BotController : CoreBotController, IGameEntity
    {
        public int id { get; private set; }

        public Vector2 position
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
                transform.position = p;
            }
        }

        public GameEntityType type => GameEntityType.Bot;

        public bool IsAlive { get; set; }

        bool _created;

        public void OnCreate(Blackboard blackboard)
        {
            if (blackboard.TryGetValue(BlackboardKeys.Id, out int _id)) {
                id = _id;
            }
            if (blackboard.TryGetValue(BlackboardKeys.Position, out Vector2 _position))
            {
                position = _position;
            }

            _created = true;
        }

        public void SetState(MonoBehaviourState state)
        {
            ServiceLocator.Get<StateMachine>().SetState(state);
        }

        protected override void Start()
        {
            base.Start();

            foreach (var botSystem in GetComponentsInChildren<BotSystem>())
            {
                botSystem.Init(this);
            }

            if (!_created)
            {
                CoreGameController.Instance.ServiceLocator.Get<IGameEntityFactory>()?.CreateManually(this);
            }

            Blackboard.SetValue(BlackboardKeys.StartPosition, transform.position);
        }

        void IGameEntity.OnDestroy()
        {
        }
    }
}
