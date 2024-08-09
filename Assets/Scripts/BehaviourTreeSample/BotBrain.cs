using Game.StateMachineSample;
using m039.Common.AI;
using m039.Common.BehaviourTrees;
using m039.Common.BehaviourTrees.Nodes;
using m039.Common.StateMachine;
using System.Collections.Generic;
using UnityEngine;

namespace Game.BehaviourTreeSample
{
    public class BotBrain : CoreBotBrain, IExpert
    {
        #region Inspector

        [SerializeField]
        NodeBase _StartNode;

        [SerializeField]
        CoreBotState _IdleState;

        [SerializeField]
        CoreBotState _MoveState;

        #endregion

        BehaviourTree BehaviourTree { get; } = new();

        StateMachine StateMachine { get; } = new();

        readonly Queue<System.Action> _expertActions = new();

        readonly Queue<System.Action> _expertLateActions = new();

        public override void Init(CoreBotController botController)
        {
            base.Init(botController);

            botController.ServiceLocator.Register(StateMachine);
            botController.ServiceLocator.Register(BehaviourTree);

            foreach (var botState in GetComponentsInChildren<CoreBotState>())
            {
                botState.Init(botController);
            }

            foreach (var botNode in GetComponentsInChildren<CoreBotNode>())
            {
                botNode.Init(botController);
            }

            BehaviourTree.AddChild(_StartNode);            

            StateMachine.AddTransition(_IdleState, _MoveState, () =>
            {
                return botController.Blackboard.ContainsKey(BlackboardKeys.Destination);
            });

            StateMachine.AddTransition(_MoveState, _IdleState, () =>
            {
                return !botController.Blackboard.ContainsKey(BlackboardKeys.Destination);
            });

            StateMachine.SetState(_IdleState);

            botController.Blackboard.SetValue(BlackboardKeys.ExpertActions, _expertActions);
            botController.Blackboard.SetValue(BlackboardKeys.ExpertLateActions, _expertLateActions);

            if (CoreGameController.Instance.ServiceLocator.TryGet(out Arbiter arbiter))
            {
                arbiter.Register(this);
            }
        }

        public override void Deinit()
        {
            base.Deinit();

            botController.ServiceLocator.Unregister(StateMachine);
            botController.ServiceLocator.Unregister(BehaviourTree);

            if (CoreGameController.Instance != null && CoreGameController.Instance.ServiceLocator.TryGet(out Arbiter arbiter))
            {
                arbiter.Unregister(this);
            }
        }

        public override void Think()
        {
            _expertActions.Clear();
            _expertLateActions.Clear();

            BehaviourTree.Update();
            StateMachine.Update();
        }

        public override void FixedThink()
        {
            base.FixedThink();

            StateMachine.FixedUpdate();
        }

        public int GetInsistence()
        {
            if (botController == null || _expertActions.Count <= 0)
            {
                return 0;
            }

            if (!botController.Blackboard.TryGetValue(BlackboardKeys.EatenFood, out int eatenFood))
            {
                return int.MaxValue;
            }

            return int.MaxValue - eatenFood;
        }

        public void Execute()
        {
            foreach (var action in _expertActions)
            {
                action?.Invoke();
            }
        }

        public void LateExecute()
        {
            foreach (var action in _expertLateActions)
            {
                action?.Invoke();
            }
        }
    }
}
