
using Game.BehaviourTreeSample;
using UnityEngine;

namespace Game.GOAPSample
{
    public class WanderBotStrategy : BotStrategy
    {
        readonly float _wanderRadius;

        public WanderBotStrategy(CoreBotController botController, float wanderRadius) : base(botController)
        {
            _wanderRadius = wanderRadius;
        }

        public override bool canPerform => !complete;

        public override bool complete => !botController.Blackboard.ContainsKey(BlackboardKeys.Destination);

        public override void Start()
        {
            base.Start();

            var gameEntity = botController.ServiceLocator.Get<IGameEntity>();

            for (int i = 0; i < 10; i++)
            {
                var position = gameEntity.position + Random.insideUnitCircle * _wanderRadius;
                if (!CameraUtils.PointInScreen(position))
                    continue;

                botController.Blackboard.SetValue(BlackboardKeys.Destination, position);
                break;
            }
        }
    }
}
