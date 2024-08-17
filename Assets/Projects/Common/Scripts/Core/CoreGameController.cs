using m039.Common;
using m039.Common.Blackboard;
using m039.Common.DependencyInjection;
using m039.Common.Events;

namespace Game
{
    public class CoreGameController : MonoBehaviourSingleton<CoreGameController>
    {
        [Inject]
        public BlackboardBase Blackboard { get; protected set; }

        [Inject]
        public EventBusByInterface EventBus { get; protected set; }

        [Inject]
        public ServiceLocator ServiceLocator { get; protected set; }
    }
}
