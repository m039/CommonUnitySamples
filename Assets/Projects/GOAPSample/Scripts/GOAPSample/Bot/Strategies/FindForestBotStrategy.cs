namespace Game.GOAPSample
{
    public class FindForestBotStrategy : BotStrategy
    {
        public FindForestBotStrategy(CoreBotController botController) : base(botController)
        {
        }

        public override bool canPerform => true;

        public override bool complete => true;

        public override void Start()
        {
            base.Start();

            var entityFactory = CoreGameController.Instance.ServiceLocator.Get<IGameEntityFactory>();
            var forest = entityFactory.GetEnteties(BehaviourTreeSample.GameEntityType.Forest);

            if (forest.Count > 0)
            {
                botController.Blackboard.SetValue(BlackboardKeys.Target, forest[UnityEngine.Random.Range(0, forest.Count)]);
            }
        }
    }
}
