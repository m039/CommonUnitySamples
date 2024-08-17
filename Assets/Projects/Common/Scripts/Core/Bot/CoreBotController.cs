using Game.BehaviourTreeSample;
using m039.Common;
using m039.Common.Blackboard;
using m039.Common.Events;
using UnityEngine;

namespace Game
{
    public class CoreBotController : MonoBehaviour
    {
        BlackboardBase _blackboard;

        public BlackboardBase Blackboard {
            get {
                if (_blackboard == null)
                {
                    _blackboard = new GameBlackboard();
                }

                return _blackboard;
            }
        }

        EventBusByInterface _eventBus;

        public EventBusByInterface EventBus
        {
            get
            {
                if (_eventBus == null)
                {
                    _eventBus = new();
                }

                return _eventBus;
            }
        }

        ServiceLocator _serviceLocator;

        public ServiceLocator ServiceLocator
        {
            get
            {
                if (_serviceLocator == null)
                {
                    _serviceLocator = new();
                }
                return _serviceLocator;
            }
        }

        [SerializeField]
        CoreBotBrain _Brain;

        CoreBotSystem[] _systems;

        protected CoreBotBrain Brain => _Brain;

        protected virtual void Awake()
        {
            _systems = GetComponentsInChildren<CoreBotSystem>();
            foreach (var botSystem in _systems)
            {
                botSystem.Init(this);
            }
        }

        protected virtual void OnDestroy()
        {
        }

        protected virtual void Start()
        {
            if (_Brain == null)
            {
                _Brain = GetComponent<CoreBotBrain>();
            }

            if (_Brain != null)
            {
                _Brain.Init(this);
            }
        }

        protected virtual void Update()
        {
            foreach (var botSystem in _systems)
            {
                botSystem.Process(Time.deltaTime);
            }

            if (_Brain != null)
            {
                _Brain.Think();
            }
        }

        protected virtual void FixedUpdate()
        {
            if (_Brain != null)
            {
                _Brain.FixedThink();
            }
        }
    }
}
