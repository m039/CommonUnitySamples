using m039.Common.BehaviourTrees.Nodes;

namespace Game.BehaviourTreeSample
{
    public abstract class CoreBotNode : NodeBase
    {
        protected CoreBotController botController { get; private set; }

        public virtual void Init(CoreBotController botController)
        {
            this.botController = botController;
        }
    }
}
