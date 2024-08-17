using m039.Common;
using m039.Common.Blackboard;
using m039.Common.DependencyInjection;
using m039.Common.Events;
using UnityEngine;

namespace Game
{
    public class CoreDependencyProvider : MonoBehaviour, IDependencyProvider
    {
        BlackboardBase _blackboard;

        ServiceLocator _serviceLocator;

        EventBusByInterface _eventBus;

        [Provide]
        protected virtual BlackboardBase GetOrCreateBlackboard()
        {
            if (_blackboard == null)
            {
                var blackboard = new Blackboard();
                _blackboard = blackboard;
            }
            return _blackboard;
        }

        [Provide]
        protected virtual EventBusByInterface GetOrCreateEventBus()
        {
            if (_eventBus == null)
            {
                _eventBus = new();
            }
            return _eventBus;
        }

        [Provide]
        protected virtual ServiceLocator GetOrCreateServiceLocator()
        {
            if (_serviceLocator == null)
            {
                _serviceLocator = new();
                _serviceLocator.Register(GetOrCreateBlackboard());
                _serviceLocator.Register(GetOrCreateEventBus());
            }

            return _serviceLocator;
        }
    }
}
