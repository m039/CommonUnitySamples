using Game.BehaviourTreeSample;
using Game.StateMachineSample;
using m039.Common;
using m039.Common.GOAP;
using m039.Common.Pathfindig;
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

        Path _path;

        int _pathIndex;

        IGameEntity _gameEntity;

        Vector2 _destination;

        public override void Init(CoreBotController botController)
        {
            base.Init(botController);

            _gameEntity = botController.ServiceLocator.Get<IGameEntity>();

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

            if (_path != null)
            {
                if (Vector2.Distance(_gameEntity.position, _path.vectorPath[_pathIndex]) < 0.01f)
                {
                    _pathIndex++;

                    if (_pathIndex < _path.vectorPath.Count)
                    {
                        botController.Blackboard.SetValue(BlackboardKeys.Destination, _path.vectorPath[_pathIndex]);
                    }
                    else
                    {
                        botController.Blackboard.SetValue(BlackboardKeys.Destination, _destination);
                        _path = null;
                    }
                }
            }

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
                    CalcNewPath();
                    _timer = null;
                };
                _timer.Start();
            }
        }

        void CalcNewPath()
        {
            var seeker = CoreGameController.Instance.ServiceLocator.Get<Seeker>();
            _destination = CameraUtils.RandomPositionOnScreen();

            _path = seeker.Search(_gameEntity.position, _destination);
            _pathIndex = 0;

            botController.ServiceLocator.Get<DebugBotSystem>().DebugPath(_path);

            if (_path != null)
            {
                botController.Blackboard.SetValue(BlackboardKeys.Destination, _path.vectorPath[_pathIndex]);
                botController.Blackboard.SetValue(BlackboardKeys.DestinationThreshold, 0.01f);
            }
            else
            {
                botController.Blackboard.SetValue(BlackboardKeys.Destination, _destination);
                botController.Blackboard.SetValue(BlackboardKeys.DestinationThreshold, 0.01f);
            }
        }

        public void OnDestroyEntity()
        {
            _timer = null;
        }
    }
}
