using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GOAPSample
{
    public class FindGladeBotStrategy : BotStrategy
    {
        public FindGladeBotStrategy(CoreBotController botController) : base(botController)
        {
        }

        public override bool canPerform => true;

        public override bool complete => true;

        public override void Start()
        {
            base.Start();

            var entityFactory = CoreGameController.Instance.ServiceLocator.Get<IGameEntityFactory>();
            var glades = entityFactory.GetEnteties(BehaviourTreeSample.GameEntityType.Glade);

            if (glades.Count > 0)
            {
                botController.Blackboard.SetValue(BlackboardKeys.Target, glades[UnityEngine.Random.Range(0, glades.Count)]);
            }
        }
    }
}
