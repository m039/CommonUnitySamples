using Game.BehaviourTreeSample;
using m039.Common;
using m039.Common.Blackboard;
using m039.Common.StateMachine;
using UnityEngine;

namespace Game.GOAPSample
{
    public class Bonfire : GameEntity
    {
        #region Inspector

        [SerializeField]
        float _SpawnRadius = 1f;

        [SerializeField]
        float _LitDuration = 10f;

        [SerializeField]
        SpriteMask _ProgressBarMask;

        #endregion

        public override float spawnRadius => _SpawnRadius;

        public override GameEntityType type => GameEntityType.Bonfire;

        readonly StateMachine _stateMachine = new();

        IState _emptyState;

        IState _litState;

        Animator _animator;

        void Awake()
        {
            _animator = GetComponentInChildren<Animator>();

            _emptyState = new EmptyBonfireState(this);
            _litState = new LitBonfireState(this);

            _stateMachine.AddTransition(_emptyState, _litState, () =>
            {
                return Blackboard.GetValue(BlackboardKeys.IsLit);
            });

            _stateMachine.AddTransition(_litState, _emptyState, () =>
            {
                return !Blackboard.GetValue(BlackboardKeys.IsLit);
            });
        }

        protected override void OnCreateEntity(BlackboardBase blackboard)
        {
            base.OnCreateEntity(blackboard);

            _stateMachine.SetState(_emptyState);
        }

        void Update()
        {
            _stateMachine.Update();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, _SpawnRadius);
        }

        abstract class BonfireState : IState
        {
            protected Bonfire bonfire;

            public BonfireState(Bonfire bonfire)
            {
                this.bonfire = bonfire;
            }

            public virtual void OnEnter() { }
            public virtual void OnExit() { }
            public virtual void OnFixedUpdate() { }
            public virtual void OnUpdate() { }
        }

        class EmptyBonfireState : BonfireState
        {
            public EmptyBonfireState(Bonfire bonfire) : base(bonfire)
            {
            }

            public override void OnEnter()
            {
                bonfire._animator.Play(AnimationKeys.Empty);
            }
        }

        class LitBonfireState : BonfireState
        {
            readonly CountdownTimer _timer;

            public LitBonfireState(Bonfire bonfire) : base(bonfire)
            {
                _timer = new CountdownTimer(bonfire._LitDuration);
                _timer.onStop += OnTimerStop;

                bonfire._ProgressBarMask.alphaCutoff = 0;
            }

            void OnTimerStop()
            {
                bonfire.Blackboard.Remove(BlackboardKeys.IsLit);
            }

            public override void OnEnter()
            {
                base.OnEnter();

                _timer.Start();
                bonfire._ProgressBarMask.alphaCutoff = 1;

                bonfire._animator.Play(AnimationKeys.Lit);
            }

            public override void OnExit()
            {
                base.OnExit();

                bonfire._ProgressBarMask.alphaCutoff = 0;
            }

            public override void OnUpdate()
            {
                base.OnUpdate();

                _timer.Tick(Time.deltaTime);
                bonfire._ProgressBarMask.alphaCutoff = 1 - _timer.progress;
            }
        }
    }
}
