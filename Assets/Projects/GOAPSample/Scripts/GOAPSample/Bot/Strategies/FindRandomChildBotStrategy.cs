using m039.Common.Blackboard;

namespace Game.GOAPSample
{
    public class FindRandomChildBotStrategy : BotStrategy
    {
        public FindRandomChildBotStrategy(CoreBotController botController) : base(botController)
        {
        }

        public override bool canPerform => true;

        public override bool complete => true;

        public override void Start()
        {
            base.Start();

            if (!botController.Blackboard.TryGetValue(BlackboardKeys.Target, out var target))
            {
                return;
            }

            if (!target.locator.Get<BlackboardBase>().TryGetValue(BlackboardKeys.Childs, out var childs))
            {
                return;
            }

            if (childs.Count > 0)
            {
                botController.Blackboard.SetValue(BlackboardKeys.Target, childs[UnityEngine.Random.Range(0, childs.Count)]);
            } else
            {
                botController.Blackboard.Remove(BlackboardKeys.Target);
            }
        }
    }
}
