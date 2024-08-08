using m039.Common;
using m039.Common.BehaviourTrees;
using m039.Common.Blackboard;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Game.BehaviourTreeSample
{
    public class SetTimerBotNode : CoreBotNode
    {
        #region Inspector

        [SerializeField]
        string _TimerKey;

        [SerializeField]
        MinMaxFloat _TimerDuration;

        #endregion

        BlackboardKey<float> _timerKey;

        public override void Init(CoreBotController botController)
        {
            base.Init(botController);

            Assert.IsNotNull(_TimerKey);

            _timerKey = new(_TimerKey);
        }

        protected override Status OnProcess()
        {
            botController.Blackboard.SetValue(_timerKey, Time.realtimeSinceStartup + _TimerDuration.Random());
            return Status.Success;
        }
    }
}
