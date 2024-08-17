using Game.StateMachineSample;
using m039.Common;
using m039.Common.GOAP;
using m039.Common.StateMachine;
using UnityEngine;

namespace Game.GOAPSample
{
    public class TrollBrains : CoreBotBrain, IOnDestoyEntityEvent
    {
        #region Inspector

        [Header("States")]
        [SerializeField]
        CoreBotState _IdleState;

        [SerializeField]
        CoreBotState _MoveState;

        [SerializeField]
        MinMaxFloat _IdleDuration = new(10, 30);

        #endregion

        StateMachine _stateMachine { get; } = new();

        CoreBotState[] _botStates;

        Timer _timer;

        public override void Init(CoreBotController botController)
        {
            base.Init(botController);

            botController.EventBus.Subscribe(this);

            botController.ServiceLocator.Register(_stateMachine);

            // State Machine.

            _botStates = GetComponentsInChildren<CoreBotState>();

            foreach (var botState in _botStates)
            {
                botState.Init(botController);
            }

            _stateMachine.AddTransition(_IdleState, _MoveState, () =>
            {
                return botController.Blackboard.ContainsKey(BlackboardKeys.Destination);
            });

            _stateMachine.AddTransition(_MoveState, _IdleState, () =>
            {
                return !botController.Blackboard.ContainsKey(BlackboardKeys.Destination);
            });

            _stateMachine.SetState(_IdleState);
        }

        public override void Think()
        {
            _stateMachine.Update();

            if (botController.Blackboard.ContainsKey(BlackboardKeys.Destination))
                return;

            if (_timer != null)
            {
                _timer.Tick(Time.deltaTime);
            } else
            {
                _timer = new CountdownTimer(_IdleDuration.Random());
                _timer.onStop += () =>
                {
                    FindNewDestination();
                    _timer = null;
                };
                _timer.Start();
            }
        }

        void FindNewDestination()
        {
            botController.Blackboard.SetValue(BlackboardKeys.Destination, CameraUtils.RandomPositionOnScreen());
        }

        public void OnDestroyEntity()
        {
            _timer = null;
        }
    }
}
