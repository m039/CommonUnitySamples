using m039.Common.BehaviourTrees;
using UnityEngine;

namespace Game.BehaviourTreeSample
{
    public class AwayFromStartPositionBotNode : CoreBotNode
    {
        protected override Status OnProcess()
        {
            if (!botController.Blackboard.TryGetValue(BlackboardKeys.StartPosition, out Vector3 startPosition))
            {
                return Status.Failure;
            }

            if (Vector2.Distance(botController.transform.position, startPosition) < 0.1f)
            {
                return Status.Failure;
            }

            return Status.Success;
        }
    }

}
