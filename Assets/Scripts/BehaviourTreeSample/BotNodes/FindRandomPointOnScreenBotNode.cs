using m039.Common.BehaviourTrees;
using m039.Common.Blackboard;
using UnityEngine;
using UnityEngine.Assertions;

namespace Game.BehaviourTreeSample
{
    public class FindRandomPointOnScreenBotNode : CoreBotNode
    {
        #region Inspector

        [SerializeField]
        string _ResultKey;

        #endregion

        BlackboardKey _resultKey;

        public override void Init(CoreBotController botController)
        {
            base.Init(botController);

            Assert.IsNotNull(_ResultKey);

            _resultKey = new(_ResultKey);
        }

        public override Status Process()
        {
            botController.Blackboard.SetValue(_resultKey, (Vector3)CameraUtils.RandomPositionOnScreen());
            return Status.Success;
        }
    }
}
