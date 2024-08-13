using Game.StateMachineSample;
using UnityEngine;

namespace Game.BehaviourTreeSample
{
    public class MoveBotState : CoreBotState
    {
        #region Inspector

        [SerializeField]
        float _MoveSpeed = 1f;

        [SerializeField]
        bool _SyncMoveSpeedInAnimator = true;

        #endregion

        public override void OnEnter()
        {
            base.OnEnter();

            if (botController.ServiceLocator.TryGet(out Animator animator))
            {
                animator.Play(AnimationKeys.Move);
                if (_SyncMoveSpeedInAnimator)
                {
                    animator.SetFloat(AnimationKeys.MoveSpeed, _MoveSpeed);
                }
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
            if (Vector2.Distance(p, destination) < botController.Blackboard.GetValue(BlackboardKeys.DestinationThreshold, 0.1f))
            {
                botController.Blackboard.Remove(BlackboardKeys.Destination);
            } else
            {
                var direction = ((Vector2)destination - p).normalized;
                botController.Blackboard.SetValue(BlackboardKeys.IsFacingLeft, direction.x < 0);

                gameEntity.position += _MoveSpeed * Time.deltaTime * direction;

                botController.Blackboard.UpdateValue(BlackboardKeys.Tiredness, x => x + Time.deltaTime);
            }
        }
    }
}
