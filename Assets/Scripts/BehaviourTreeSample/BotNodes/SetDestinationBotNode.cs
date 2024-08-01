using Game.BehaviourTreeSample;
using m039.Common.BehaviourTrees;
using m039.Common.Blackboard;
using UnityEngine;
using UnityEngine.Assertions;

namespace Game.BehaviourTreeSample
{
    public class SetDestinationBotNode : CoreBotNode
    {
        #region Inspector

        [SerializeField]
        string _ArgumentKey;

        #endregion

        BlackboardKey _argumentKey;

        public override void Init(CoreBotController botController)
        {
            base.Init(botController);

            Assert.IsNotNull(_ArgumentKey);

            _argumentKey = new(_ArgumentKey);
        }

        public override Status Process()
        {
            if (botController.Blackboard.TryGetValue(_argumentKey, out Vector3 position))
            {
                botController.Blackboard.SetValue(BlackboardKeys.Destination, position);
                return Status.Success;
            }

            return Status.Failure;
        }
    }
}
