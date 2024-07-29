using m039.Common.BehaviourTrees;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.BehaviourTreeSample
{
    public class BotBrain : CoreBotBrain
    {
        #region Inspector

        [SerializeField]
        BlinkBotNode _BlinkNode;

        #endregion

        BehaviourTree BehaviourTree { get; } = new();

        public override void Init(CoreBotController botController)
        {
            base.Init(botController);

            _BlinkNode.Init(botController);

            BehaviourTree.AddChild(_BlinkNode);
        }

        public override void Think()
        {
            BehaviourTree.Update();
        }
    }
}
