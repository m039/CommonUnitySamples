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

        protected override void OnInit(CoreBotController botController)
        {
            base.OnInit(botController);

            if (!_SyncMoveSpeedInAnimator)
            {
                botController.Blackboard.Subscribe(BlackboardKeys.MoveAnimationSpeed, UpdateMoveAnimationSpeed);
                UpdateMoveAnimationSpeed();
            }
        }

        void UpdateMoveAnimationSpeed()
        {
            if (!botController.ServiceLocator.TryGet(out Animator animator))
            {
                return;
            }

            if (botController.Blackboard.TryGetValue(BlackboardKeys.MoveAnimationSpeed, out var animationSpeed))
            {
                animator.SetFloat(AnimationKeys.MoveSpeed, animationSpeed);
            }
        }

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

                var moveSpeedMultiplier = botController.Blackboard.GetValue(BlackboardKeys.MoveSpeedMultiplier, 1);

                gameEntity.position += _MoveSpeed * moveSpeedMultiplier * Time.deltaTime * direction;

                botController.Blackboard.UpdateValue(BlackboardKeys.Tiredness, x => x + Time.deltaTime);
            }
        }
    }
}
