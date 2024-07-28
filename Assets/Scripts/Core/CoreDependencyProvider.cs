using m039.Common;
using m039.Common.Blackboard;
using m039.Common.DependencyInjection;
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

        [Provide]
        protected virtual Blackboard CreateOrGetBlackboard()
        {
            if (_blackboard == null)
            {
                _blackboard = new Blackboard();
                _blackboard.SetValues(_BlackboardData);
            }
            return _blackboard;
        }

        [Provide]
        protected virtual ServiceLocator CreateOrGetServiceLocator()
        {
            if (_serviceLocator == null)
            {
                _serviceLocator = new ServiceLocator();
                _serviceLocator.Register(CreateOrGetBlackboard());
            }

            return _serviceLocator;
        }
    }
}
