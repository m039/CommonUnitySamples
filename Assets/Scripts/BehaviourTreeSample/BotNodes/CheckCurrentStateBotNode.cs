using Game.StateMachineSample;
using m039.Common.BehaviourTrees;
using m039.Common.StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.BehaviourTreeSample
{
    public class CheckCurrentStateBotNode : CoreBotNode
    {
        #region Inspector

        [SerializeField]
        CoreBotState _State;

        #endregion

        public override Status Process()
        {
            if (botController.ServiceLocator.TryGet(out StateMachine sm) &&
                sm.CurrentState is CoreBotState cs &&
                cs == _State)
            {
                return Status.Success;
            }

            return Status.Failure;
        }
    }
}
