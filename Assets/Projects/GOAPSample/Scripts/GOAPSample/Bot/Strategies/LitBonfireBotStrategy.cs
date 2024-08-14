using Game.BehaviourTreeSample;
using UnityEngine;

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

            var targetBlackboard = target.GetBlackboard();

            if (targetBlackboard.GetValue(BlackboardKeys.IsLit) ||
                !botController.Blackboard.GetValue(BlackboardKeys.HasWood))
                return;

            targetBlackboard.SetValue(BlackboardKeys.IsLit, true);

            botController.Blackboard.Remove(BlackboardKeys.HasWood);
        }

    }
}
