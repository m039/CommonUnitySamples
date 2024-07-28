using m039.Common;
using m039.Common.Blackboard;
using m039.Common.DependencyInjection;
using m039.Common.Events;
using UnityEngine;

namespace Game
{
    public class CoreDependencyProvider : MonoBehaviour, IDependencyProvider
    {
        #region Inspector

        [SerializeField]
        BlackboardData _BlackboardData;

        #endregion

        Blackboard _blackboard;

        ServiceLocator _serviceLocator;

        EventBusByInterface _eventBus;

        [Provide]
        protected virtual Blackboard GetOrCreateBlackboard()
        {
            if (_blackboard == null)
            {
                _blackboard = new();
                _blackboard.SetValues(_BlackboardData);
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
