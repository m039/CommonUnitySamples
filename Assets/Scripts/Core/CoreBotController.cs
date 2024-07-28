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

        [SerializeField]
        CoreBotBrain _Brain;

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
    }
}
