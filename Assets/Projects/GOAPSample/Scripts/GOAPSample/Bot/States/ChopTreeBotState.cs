using Game.BehaviourTreeSample;
using Game.StateMachineSample;
using m039.Common;
using UnityEngine;

namespace Game.GOAPSample
{
    public class ChopTreeBotState : CoreBotState
    {
        #region Inspector

        [SerializeField]
        float _ChopDuration = 5;

        [SerializeField]
        SpriteMask _ProgressBarMask;

        #endregion

        CountdownTimer _timer;

        IGameEntity _tree;

        protected override void OnInit(CoreBotController botController)
        {
            base.OnInit(botController);

            _timer = new CountdownTimer(_ChopDuration);
            _timer.onStop += OnTimerStop;

            _ProgressBarMask.alphaCutoff = 0;
        }

        void OnTimerStop()
        {
            botController.Blackboard.SetValue(BlackboardKeys.HasWood, true);
            botController.Blackboard.Remove(BlackboardKeys.IsChoping);
            _tree.Destroy();
        }

        public override void OnEnter()
        {
            base.OnEnter();

            _timer.Start();
            _ProgressBarMask.alphaCutoff = 0;

            if (botController.ServiceLocator.TryGet(out Animator animator))
            {
                animator.Play(AnimationKeys.Idle);
            }

            if (botController.Blackboard.TryGetValue(BlackboardKeys.Target, out var target) &&
                target.type == GameEntityType.Tree)
            {
                _tree = target;
            } else
            {
                botController.Blackboard.Remove(BlackboardKeys.IsChoping);
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            _ProgressBarMask.alphaCutoff = 0;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            _timer.Tick(Time.deltaTime);
            _ProgressBarMask.alphaCutoff = _timer.progress;
        }
    }
}
