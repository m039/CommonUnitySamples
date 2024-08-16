using Game.BehaviourTreeSample;

namespace Game.GOAPSample
{
    public class TakeMushroomBotStrategy : BotStrategy
    {
        public TakeMushroomBotStrategy(CoreBotController botController) : base(botController)
        {
        }

        public override bool canPerform => true;

        public override bool complete => true;

        public override void Start()
        {
            base.Start();

            if (botController.Blackboard.ContainsKey(BlackboardKeys.HasFood))
                return;

            if (botController.Blackboard.TryGetValue(BlackboardKeys.Target, out var target) &&
                target.type == GameEntityType.Mushroom)
            {
                botController.Blackboard.SetValue(BlackboardKeys.HasFood, true);
                target.Destroy();
            }
        }
    }
}
