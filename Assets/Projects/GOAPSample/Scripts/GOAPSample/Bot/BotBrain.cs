using Game.StateMachineSample;
using m039.Common.BehaviourTrees.Nodes;
using m039.Common.GOAP;
using m039.Common.StateMachine;
using System.Collections.Generic;
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

        [SerializeField]
        CoreBotState _ChopTreeState;

        #endregion

        ActionPlan _actionPlan;

        AgentAction _currentAction;

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

            StateMachine.AddAnyTransition(
                _ChopTreeState,
                () => botController.Blackboard.GetValue(BlackboardKeys.IsChoping)
            );

            StateMachine.AddTransition(
                _ChopTreeState,
                _IdleState,
                () => !botController.Blackboard.GetValue(BlackboardKeys.IsChoping)
            );

            StateMachine.SetState(_IdleState);

            SetupGOAP();
        }

        void SetupGOAP()
        {
            // noop
        }

        ActionPlan CreatePlan()
        {
            var entityFactory = CoreGameController.Instance.ServiceLocator.Get<IGameEntityFactory>();
            Stack<AgentAction> actions = new();

            actions.Push(new AgentAction.Builder("ChopTree")
                .WithStrategy(new ChopTreeBotStrategy(botController))
                .Build());

            actions.Push(new AgentAction.Builder("GoToFirstTree")
                .WithStrategy(new MoveBotStrategy(botController, () =>
                {
                    return botController.Blackboard.GetValue(BlackboardKeys.Target).position;
                }))
                .Build());

            actions.Push(new AgentAction.Builder("FindTree")
                .WithStrategy(new FindTreeBotStrategy(botController))
                .Build());

            actions.Push(new AgentAction.Builder("GoToFirstForest")
                .WithStrategy(new MoveBotStrategy(botController, () =>
                {
                    return botController.Blackboard.GetValue(BlackboardKeys.Target).position;
                }))
                .Build());

            actions.Push(new AgentAction.Builder("FindForest")
                .WithStrategy(new FindForestBotStrategy(botController))
                .Build());

            return new ActionPlan(null, actions, 0);
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
            if (_currentAction != null)
            {
                if (_currentAction.complete)
                {
                    _currentAction.Stop();
                    _currentAction = null;

                    if (_actionPlan.actions.Count <= 0)
                    {
                        _actionPlan = null;
                    }
                } else
                {
                    _currentAction.Update(Time.deltaTime);
                }
            }

            if (_actionPlan != null && _actionPlan.actions.Count > 0 && _currentAction == null)
            {
                _currentAction = _actionPlan.actions.Pop();
                _currentAction.Start();
            }

            if (_actionPlan == null)
            {
                _actionPlan = CreatePlan();
            }

            StateMachine.Update();
        }

        public override void FixedThink()
        {
            base.FixedThink();

            StateMachine.FixedUpdate();
        }
    }
}
