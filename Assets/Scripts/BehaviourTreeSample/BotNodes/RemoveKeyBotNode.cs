using m039.Common.BehaviourTrees;
using m039.Common.Blackboard;
using UnityEngine;
using UnityEngine.Assertions;

namespace Game.BehaviourTreeSample
{
    public class RemoveKeyBotNode : CoreBotNode
    {
        #region Inspector

        [SerializeField]
        string _Key;

        #endregion

        BlackboardKey<object> _key;

        public override void Init(CoreBotController botController)
        {
            base.Init(botController);

            Assert.IsNotNull(_Key);
            _key = new(_Key);
        }

        public override Status Process()
        {
            botController.Blackboard.Remove(_key);
            return Status.Success;
        }
    }
}
