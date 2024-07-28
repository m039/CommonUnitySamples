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

        #endregion

        StateMachine StateMachine { get; } = new();

        public override void Init(CoreBotController botController)
        {
            base.Init(botController);

            foreach (var botState in GetComponentsInChildren<BotState>())
            {
                botState.Init(botController);
            }

            var idleState = _IdleState;
            var patrolState = _PatrolState;
            var wanderState = _WanderState;

            StateMachine.AddAnyTransition(idleState, () => Input.GetKeyDown(KeyCode.R));
            StateMachine.AddAnyTransition(patrolState, () => Input.GetKeyDown(KeyCode.P));
            StateMachine.AddAnyTransition(wanderState, () => Input.GetKeyDown(KeyCode.W));
            StateMachine.SetState(idleState);
        }

        void OnGUI()
        {
            if (StateMachine.CurrentState is MonoBehaviourState state)
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

                GUI.Label(new Rect(10, 10, 1000, 200), "State: " + string.Join(" => ", state.GetHierarchicalStates().Select(getName)));
            }
        }

        public override void Think()
        {
            StateMachine.Update();
        }

        public override void FixedThink()
        {
            base.FixedThink();

            StateMachine.FixedUpdate();
        }
    }
}
