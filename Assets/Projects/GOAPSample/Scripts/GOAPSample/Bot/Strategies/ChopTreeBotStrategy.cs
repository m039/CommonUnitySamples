using Game.BehaviourTreeSample;

namespace Game.GOAPSample
{
    public class ChopTreeBotStrategy : BotStrategy
    {
        public ChopTreeBotStrategy(CoreBotController botController) : base(botController)
        {
        }

        public override bool canPerform => false;

        public override bool complete => _tree == null || !_tree.IsAlive;

        IGameEntity _tree;

        public override void Start()
        {
            base.Start();

            if (botController.Blackboard.TryGetValue(BlackboardKeys.Target, out var target) && target.type == GameEntityType.Tree)
            {
                _tree = target;
            }

            botController.Blackboard.SetValue(BlackboardKeys.IsChoping, true);
        }

        public override void Stop()
        {
            base.Stop();

            _tree = null;
            botController.Blackboard.Remove(BlackboardKeys.IsChoping);
        }
    }
}
