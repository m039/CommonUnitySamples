using m039.Common.BehaviourTrees;
using m039.Common.StateMachine;
using UnityEngine;

namespace Game.BehaviourTreeSample
{
    public class SetStateBotNode : CoreBotNode
    {
        #region Inspector

        [SerializeField]
        MonoBehaviourState _State;

        #endregion

        public override Status Process()
        {
            botController.ServiceLocator.Get<StateMachine>().SetState(_State);
            return Status.Success;
        }
    }
}
