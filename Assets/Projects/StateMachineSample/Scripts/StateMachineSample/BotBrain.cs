using m039.Common.StateMachine;
using System.Linq;
using UnityEngine;

namespace Game.StateMachineSample
{
    public class BotBrain : CoreBotBrain
    {
        #region Inspector

        [SerializeField] IdleBotState _IdleState;

        [SerializeField] WanderBotState _WanderState;

        [SerializeField] PatrolBotState _PatrolState;

        [SerializeField]
        TMPro.TMP_Text _Info;

        #endregion

        StateMachine _stateMachine { get; } = new();

        bool _idleClicked;

        bool _patrolClicked;

        bool _wanderClicked;

        public override void Init(CoreBotController botController)
        {
            base.Init(botController);

            foreach (var botState in GetComponentsInChildren<CoreBotState>())
            {
                botState.Init(botController);
            }

            var idleState = _IdleState;
            var patrolState = _PatrolState;
            var wanderState = _WanderState;

            _stateMachine.AddAnyTransition(idleState, () => Input.GetKeyDown(KeyCode.R) || _idleClicked);
            _stateMachine.AddAnyTransition(patrolState, () => Input.GetKeyDown(KeyCode.P) || _patrolClicked);
            _stateMachine.AddAnyTransition(wanderState, () => Input.GetKeyDown(KeyCode.W) || _wanderClicked);
            _stateMachine.SetState(idleState);
        }

        public void OnIdleClicked()
        {
            _idleClicked = true;
        }

        public void OnPatrolClicked()
        {
            _patrolClicked = true;
        }

        public void OnWanderClicked()
        {
            _wanderClicked = true;
        }

        public override void Think()
        {
            _stateMachine.Update();
            UpdateInfo();

            _idleClicked = false;
            _patrolClicked = false;
            _wanderClicked = false;
        }

        public override void FixedThink()
        {
            base.FixedThink();

            _stateMachine.FixedUpdate();
        }

        void UpdateInfo()
        {
            if (_stateMachine.CurrentState is MonoBehaviourState state)
            {
                string getName(IState state)
                {
                    if (state is MonoBehaviourState mbs)
                    {
                        return mbs.gameObject.name;
                    }
                    else
                    {
                        return state.GetType().Name;
                    }
                }

                _Info.text = "State: " + string.Join(" => ", state.GetHierarchicalStates().Select(getName));
                _Info.gameObject.SetActive(true);
            } else
            {
                _Info.gameObject.SetActive(false);
            }
        }
    }
}
