using Game.StateMachineSample;
using m039.Common.BehaviourTrees.Nodes;
using m039.Common.GOAP;
using m039.Common.StateMachine;
using UnityEngine;

namespace Game.GOAPSample
{
    public class BotBrain : CoreBotBrain
    {
        #region Inspector

        [SerializeField]
        CoreBotState _IdleState;

        [SerializeField]
        CoreBotState _MoveState;

        #endregion

        AgentAction _action;

        StateMachine StateMachine { get; } = new();

        CoreBotState[] _botStates;

        public override void Init(CoreBotController botController)
        {
            base.Init(botController);

            botController.ServiceLocator.Register(StateMachine);

            // State Machine.

            _botStates = GetComponentsInChildren<CoreBotState>();

            foreach (var botState in _botStates)
            {
                botState.Init(botController);
            }

            StateMachine.AddTransition(_IdleState, _MoveState, () =>
            {
                return botController.Blackboard.GetValue(BlackboardKeys.IsMoving);
            });

            StateMachine.AddTransition(_MoveState, _IdleState, () =>
            {
                return !botController.Blackboard.GetValue(BlackboardKeys.IsMoving);
            });

            StateMachine.SetState(_IdleState);

            SetupGOAP();
        }

        void SetupGOAP()
        {
            var entityFactory = CoreGameController.Instance.ServiceLocator.Get<IGameEntityFactory>();

            _action = new AgentAction.Builder("GoToFirstTree")
                .WithStrategy(new MoveBotStrategy(botController, () =>
                {
                    var trees = entityFactory.GetEnteties(BehaviourTreeSample.GameEntityType.Tree);
                    if (trees.Count <= 0)
                        return Vector2.zero;

                    return trees[0].position;
                }))
                .Build();
            _action.Start();
        }

        public override void Deinit()
        {
            base.Deinit();

            botController.ServiceLocator.Unregister(StateMachine);

            foreach (var botState in _botStates)
            {
                botState.Deinit();
            }
        }

        public override void Think()
        {
            _action.Update(Time.deltaTime);

            StateMachine.Update();
        }

        public override void FixedThink()
        {
            base.FixedThink();

            StateMachine.FixedUpdate();
        }
    }
}
