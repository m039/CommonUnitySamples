using m039.Common;
using m039.Common.Blackboard;
using m039.Common.DependencyInjection;
using UnityEngine;
using UnityEngine.Assertions;

namespace Game
{
    public class CoreGameController : MonoBehaviourSingleton<CoreGameController>
    {
        [Inject]
        public Blackboard Blackboard { get; private set; }

        [Inject]
        public ServiceLocator ServiceLocator { get; private set; }

        void Start()
        {
            Assert.IsNotNull(ServiceLocator);
            Assert.AreEqual(Blackboard, ServiceLocator.Get<Blackboard>());

            var key22 = new BlackboardKey("key22");
            if (Blackboard.TryGetValue(key22, out int key22Value))
            {
                Debug.Log("key22 => " + key22Value);
            }
            else
            {
                Debug.Log("Can't find value.");
            }

            OnStart();
        }

        protected virtual void OnStart()
        {
        }
    }
}
