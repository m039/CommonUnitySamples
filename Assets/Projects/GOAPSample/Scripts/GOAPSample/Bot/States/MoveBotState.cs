using Game.StateMachineSample;
using m039.Common.Blackboard;
using UnityEngine;

namespace Game.GOAPSample
{
    public class MoveBotState : CoreBotState
    {
        public override void OnEnter()
        {
            base.OnEnter();

            if (botController.ServiceLocator.TryGet(out Animator animator))
            {
                animator.Play(AnimationKeys.Move);
                if (botController.ServiceLocator.TryGet(out BlackboardBase blackboard))
                {
                    animator.SetFloat(AnimationKeys.MoveSpeed, blackboard.GetValue(BlackboardKeys.MoveSpeed, 1f));
                }
            }
        }
    }
}
