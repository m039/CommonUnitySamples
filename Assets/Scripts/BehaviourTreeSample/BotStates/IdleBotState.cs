using Game.StateMachineSample;
using m039.Common;
using UnityEngine;

namespace Game.BehaviourTreeSample
{
    public class IdleBotState : CoreBotState
    {
        public override void OnEnter()
        {
            base.OnEnter();

            if (botController.ServiceLocator.TryGet(out Animator animator))
            {
                animator.Play(AnimationKeys.Idle);
            }
        }
    }
}
