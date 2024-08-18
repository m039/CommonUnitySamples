using Game.BehaviourTreeSample;
using m039.Common;
using m039.Common.Blackboard;
using m039.Common.Events;

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
                return _botController;
            }
        }

        public override ServiceLocator locator => botController.ServiceLocator;

        protected override BlackboardBase Blackboard => botController.Blackboard;

        public override GameEntityType type => GameEntityType.Bot;

        protected override void OnCreateEntity(BlackboardBase blackboard)
        {
            base.OnCreateEntity(blackboard);

            Blackboard.SetValue(BlackboardKeys.House, blackboard.GetValue(BlackboardKeys.House));

            botController.EventBus.Raise<IOnCreateEntityEvent>(a => a.OnCreateEntity());
        }

        protected override void OnDestroyEntity()
        {
            base.OnDestroyEntity();

            botController.EventBus.Raise<IOnDestoyEntityEvent>(a => a.OnDestroyEntity());
        }
    }
}
