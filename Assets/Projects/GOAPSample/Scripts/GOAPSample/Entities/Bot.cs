using Game.BehaviourTreeSample;
using m039.Common;
using m039.Common.Blackboard;

namespace Game.GOAPSample
{
    public class Bot : GameEntity
    {
        CoreBotController _botController;

        CoreBotController botController
        {
            get
            {
                if (_botController == null)
                {
                    _botController = GetComponent<CoreBotController>();
                }
                return botController;
            }
        }

        public override ServiceLocator locator => botController.ServiceLocator;

        protected override BlackboardBase Blackboard => botController.Blackboard;

        public override GameEntityType type => GameEntityType.Bot;

    }
}
