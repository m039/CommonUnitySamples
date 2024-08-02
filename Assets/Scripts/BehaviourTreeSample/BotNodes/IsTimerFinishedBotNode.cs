using m039.Common.BehaviourTrees;
using m039.Common.Blackboard;
using UnityEngine;
using UnityEngine.Assertions;

namespace Game.BehaviourTreeSample
{
    public class IsTimerFinishedBotNode : CoreBotNode
    {
        #region Inspector

        [SerializeField]
        string _TimerKey;

        #endregion

        BlackboardKey _timerKey;

        public override void Init(CoreBotController botController)
        {
            base.Init(botController);

            Assert.IsNotNull(_TimerKey);

            _timerKey = new(_TimerKey);
        }

        public override Status Process()
        {
            if (botController.Blackboard.TryGetValue(_timerKey, out float timer))
            {
                if (timer > Time.realtimeSinceStartup)
                {
                    return Status.Failure;
                }
            }

            return Status.Success;
        }

    }
}
