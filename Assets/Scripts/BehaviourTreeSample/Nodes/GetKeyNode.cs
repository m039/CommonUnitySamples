using m039.Common.BehaviourTrees;
using m039.Common.BehaviourTrees.Nodes;
using UnityEngine;

namespace Game.BehaviourTreeSample
{
    public class GetKeyNode : NodeBase
    {
        #region Inspector

        [SerializeField]
        KeyCode _Key = KeyCode.T;

        #endregion

        public override Status Process()
        {
            if (Input.GetKey(_Key))
            {
                return Status.Success;
            } else
            {
                return Status.Failure;
            }
        }
    }
}
