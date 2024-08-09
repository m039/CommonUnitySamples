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

        public override void OnEnter()
        {
            base.OnEnter();

            if (botController.ServiceLocator.TryGet(out Animator animator))
            {
                animator.Play(AnimationKeys.Move);
                animator.SetFloat(AnimationKeys.MoveSpeed, _MoveSpeed);
            }
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (!botController.Blackboard.TryGetValue(BlackboardKeys.Destination, out var destination))
            {
                return;
            }

            if (!botController.ServiceLocator.TryGet(out IGameEntity gameEntity))
                return;

            var p = gameEntity.position;
            if (Vector2.Distance(p, destination) < 0.1f)
            {
                botController.Blackboard.Remove(BlackboardKeys.Destination);
            } else
            {
                gameEntity.position += _MoveSpeed * Time.deltaTime * ((Vector2)destination - p).normalized;
            }
        }
    }
}
