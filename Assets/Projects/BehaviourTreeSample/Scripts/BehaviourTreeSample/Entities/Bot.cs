using m039.Common;
using m039.Common.Blackboard;
using m039.Common.Events;
using UnityEngine;

namespace Game.BehaviourTreeSample
{
    public enum BotClass
    {
        Blue = 0,
        Yellow = 1,
        Green = 2,
        Purple = 3
    }

    public class Bot : GameEntity
    {
        #region Inspector

        [SerializeField]
        BotClass _TypeClass;

        #endregion

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

        protected override EventBusByInterface EventBus => botController.EventBus;

        public override GameEntityType type => GameEntityType.Bot;

        public override int typeClass => (int)_TypeClass;

        bool _created;

        protected override void OnCreateEntity(BlackboardBase blackboard)
        {
            base.OnCreateEntity(blackboard);

            if (blackboard.TryGetValue(BlackboardKeys.GroupBlackboard, out BlackboardBase _groupBlackboard))
            {
                Blackboard.SetValue(BlackboardKeys.GroupBlackboard, _groupBlackboard);
            }

            _created = true;
        }

        protected void Start()
        {
            if (!_created)
            {
                CoreGameController.Instance.ServiceLocator.Get<IGameEntityFactory>()?.CreateManually(this);
            }

            Blackboard.SetValue(BlackboardKeys.StartPosition, transform.position);
        }

        protected override void OnDestroyEntity()
        {
            base.OnDestroyEntity();

            botController.EventBus.Raise<IOnDestoyEntityEvent>(a => a.OnDestroyEntity());
            _created = false;
        }
    }
}
