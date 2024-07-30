using Game.StateMachineSample;
using m039.Common.BehaviourTrees;
using m039.Common.BehaviourTrees.Nodes;
using m039.Common.StateMachine;
using UnityEngine;

namespace Game.BehaviourTreeSample
{
    public class BotBrain : CoreBotBrain
    {
        #region Inspector

        [SerializeField]
        NodeBase _StartNode;

        #endregion

        BehaviourTree BehaviourTree { get; } = new();

        StateMachine StateMachine { get; } = new();

        public override void Init(CoreBotController botController)
        {
            base.Init(botController);

            botController.ServiceLocator.Register(StateMachine);

            foreach (var botState in GetComponentsInChildren<CoreBotState>())
            {
                botState.Init(botController);
            }

            foreach (var botNode in GetComponentsInChildren<CoreBotNode>())
            {
                botNode.Init(botController);
            }

            BehaviourTree.AddChild(_StartNode);
        }

        public override void Think()
        {
            BehaviourTree.Update();
            StateMachine.Update();
        }

        public override void FixedThink()
        {
            base.FixedThink();

            StateMachine.FixedUpdate();
        }
    }
}
