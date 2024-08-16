using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GOAPSample
{
    public class EatBotStrategy : BotStrategy
    {
        public EatBotStrategy(CoreBotController botController) : base(botController)
        {
        }

        public override bool canPerform => true;

        public override bool complete => true;

        public override void Start()
        {
            base.Start();

            if (!botController.Blackboard.ContainsKey(BlackboardKeys.HasFood))
                return;

            botController.Blackboard.Remove(BlackboardKeys.HasFood);
            botController.Blackboard.Remove(BlackboardKeys.Hunger);
        }
    }
}
