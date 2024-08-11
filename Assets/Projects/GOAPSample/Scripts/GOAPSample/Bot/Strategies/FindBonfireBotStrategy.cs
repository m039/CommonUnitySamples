using m039.Common.Blackboard;

namespace Game.GOAPSample
{
    public class FindBonfireBotStrategy : BotStrategy
    {
        public FindBonfireBotStrategy(CoreBotController botController) : base(botController)
        {
        }

        public override bool canPerform => false;

        public override bool complete => true;

        public override void Start()
        {
            base.Start();

            if (!botController.Blackboard.TryGetValue(BlackboardKeys.House, out var house))
            {
                return;
            }

            if (!house.locator.Get<BlackboardBase>().TryGetValue(BlackboardKeys.Bonfire, out var bonfire))
            {
                return;
            }

            botController.Blackboard.SetValue(BlackboardKeys.Target, bonfire);
        }
    }
}
