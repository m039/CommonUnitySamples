using Game.StateMachineSample;
using UnityEngine;
using m039.Common;

namespace Game.BehaviourTreeSample
{
    public class MoveBotState : CoreBotState
    {
        #region Inspector

        [SerializeField]
        float _MoveSpeed = 1f;

        #endregion

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (!botController.Blackboard.TryGetValue(BlackboardKeys.Destination, out Vector3 destination))
            {
                return;
            }

            var p = botController.transform.position;
            if (Vector3.Distance(p, destination) < 0.1f)
            {
                botController.Blackboard.Remove(BlackboardKeys.Destination);
            } else
            {
                botController.transform.position += _MoveSpeed * Time.deltaTime * (destination - p).With(z:0).normalized;
            }
        }
    }
}
