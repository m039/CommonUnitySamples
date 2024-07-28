using m039.Common;
using m039.Common.Blackboard;
using m039.Common.DependencyInjection;
using m039.Common.Events;

namespace Game
{
    public class CoreGameController : MonoBehaviourSingleton<CoreGameController>
    {
        [Inject]
        public Blackboard Blackboard { get; private set; }

        [Inject]
        public EventBusByInterface EventBus { get; private set; }

        [Inject]
        public ServiceLocator ServiceLocator { get; private set; }
    }
}
