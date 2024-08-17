using m039.Common;
using m039.Common.Blackboard;
using m039.Common.StateMachine;
using UnityEditor;
using UnityEngine;

namespace Game.BehaviourTreeSample
{
    public enum BotClass
    {
        Blue = 0,
        Yellow = 1,
        Green = 2,
        Purple = 3
    }

    public class BotController : CoreBotController, IGameEntity
    {
        #region Inspector

        [SerializeField]
        BotClass _TypeClass;

        #endregion

        public int id { get; private set; }

        public float spawnRadius => 0;

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
                p.z = Mathf.InverseLerp(-100, 100, value.y) * 100;
                transform.position = p;
            }
        }

        public GameEntityType type => GameEntityType.Bot;

        public bool IsAlive { get; set; }

        ServiceLocator IGameEntity.locator => ServiceLocator;

        public int typeClass => (int)_TypeClass;

        string IGameEntity.name => $"Bot_{_TypeClass}#{id}";

        bool _created;

        BotSystem[] _systems;

        public void OnCreate(BlackboardBase blackboard)
        {
            if (blackboard.TryGetValue(BlackboardKeys.Id, out int _id)) {
                id = _id;
            }

            if (blackboard.TryGetValue(BlackboardKeys.Position, out Vector2 _position))
            {
                position = _position;
            }

            if (blackboard.TryGetValue(BlackboardKeys.GroupBlackboard, out BlackboardBase _groupBlackboard))
            {
                Blackboard.SetValue(BlackboardKeys.GroupBlackboard, _groupBlackboard);
            }

            _created = true;
        }

        protected override void Awake()
        {
            base.Awake();

            ServiceLocator.Register(GetComponent<Animator>());
            ServiceLocator.Register<IGameEntity>(this);
            ServiceLocator.Register(Blackboard);
        }

        protected override void Start()
        {
            base.Start();

            _systems = GetComponentsInChildren<BotSystem>();

            foreach (var botSystem in _systems)
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
            _created = false;
        }
    }
}
