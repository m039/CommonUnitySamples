using m039.Common.BehaviourTrees;
using m039.Common.Blackboard;
using UnityEngine;
using UnityEngine.Assertions;

namespace Game.BehaviourTreeSample
{
    public class IsTargetValidBotNode : CoreBotNode
    {
        #region Inspector

        [SerializeField]
        string _ResultKey;

        #endregion

        BlackboardKey<Vector3> _resultKey;

        public override void Init(CoreBotController botController)
        {
            base.Init(botController);

            Assert.IsNotNull(_ResultKey);

            _resultKey = new(_ResultKey);
        }

        protected override Status OnProcess()
        {
            if (!botController.Blackboard.TryGetValue(BlackboardKeys.Target, out var target))
            {
                return Status.Failure;
            }

            if (target == null || !target.IsAlive)
            {
                botController.Blackboard.Remove(BlackboardKeys.Target);
                return Status.Failure;
            }

            botController.Blackboard.SetValue(_resultKey, target.position);
            return Status.Success;
        }
    }
}
