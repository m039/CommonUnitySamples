using m039.Common;
using m039.Common.Blackboard;
using m039.Common.Events;
using UnityEngine;

namespace Game
{
    public class CoreBotController : MonoBehaviour
    {
        Blackboard _blackboard;

        public Blackboard Blackboard {
            get {
                if (_blackboard == null)
                {
                    _blackboard = new();
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

        CoreBotFeature[] _features;

        protected virtual void Awake()
        {
            _features = GetComponents<CoreBotFeature>();
            foreach (var feature in _features)
            {
                feature.Init(this);
            }
        }

        protected virtual void OnDestroy()
        {
            foreach (var feature in _features)
            {
                feature.Deinit(this);
            }
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
