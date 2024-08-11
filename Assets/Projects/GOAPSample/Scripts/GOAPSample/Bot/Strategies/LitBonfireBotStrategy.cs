using Game.BehaviourTreeSample;

namespace Game.GOAPSample
{
    public class LitBonfireBotStrategy : BotStrategy
    {
        public LitBonfireBotStrategy(CoreBotController botController) : base(botController)
        {
        }

        public override bool canPerform => false;

        public override bool complete => true;

        public override void Start()
        {
            base.Start();

            if (!botController.Blackboard.TryGetValue(BlackboardKeys.Target, out var target) ||
                target.type != GameEntityType.Bonfire)
            {
                return;
            }

            target.GetBlackboard().SetValue(BlackboardKeys.IsLit, true);

            botController.Blackboard.Remove(BlackboardKeys.HasTree);
        }

    }
}
