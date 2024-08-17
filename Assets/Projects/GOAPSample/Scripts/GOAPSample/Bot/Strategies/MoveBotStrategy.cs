using Game.BehaviourTreeSample;
using Game.GOAPSample;
using System;
using UnityEngine;

namespace Game.GOAPSample
{
    public class MoveBotStrategy : BotStrategy
    {
        readonly Func<Vector2> _destination;
        readonly IGameEntity _gameEntity;
        readonly Func<float> _inRangeOf;
        readonly float _moveSpeedMultiplier;

        public MoveBotStrategy(
            CoreBotController botController,
            Func<Vector2> destination,
            Func<float> inRangeOf = null,
            float moveSpeedMultiplier = 1f
        ) : base(botController)
        {
            _destination = destination;
            _gameEntity = botController.ServiceLocator.Get<IGameEntity>();
            _moveSpeedMultiplier = moveSpeedMultiplier;
            if (inRangeOf == null)
            {
                _inRangeOf = () => 0.01f;
            } else
            {
                _inRangeOf = inRangeOf;
            }
        }

        public override bool canPerform => !complete;

        public override bool complete => !botController.Blackboard.ContainsKey(BlackboardKeys.Destination);

        public override void Start()
        {
            base.Start();

            botController.Blackboard.SetValue(BlackboardKeys.Destination, _destination());
            botController.Blackboard.SetValue(BlackboardKeys.DestinationThreshold, _inRangeOf());
            botController.Blackboard.SetValue(BlackboardKeys.MoveSpeedMultiplier, _moveSpeedMultiplier);
        }

        public override void Stop()
        {
            base.Stop();

            botController.Blackboard.Remove(BlackboardKeys.Destination);
        }
    }
}
