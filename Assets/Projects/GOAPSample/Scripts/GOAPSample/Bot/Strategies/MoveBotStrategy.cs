using Game.BehaviourTreeSample;
using Game.GOAPSample;
using System;
using UnityEngine;

namespace Game
{
    public class MoveBotStrategy : BotStrategy
    {
        readonly Func<Vector2> _destination;
        readonly IGameEntity _gameEntity;
        Vector2 _target;

        public MoveBotStrategy(CoreBotController botController, Func<Vector2> destination) : base(botController)
        {
            _destination = destination;
            _gameEntity = botController.ServiceLocator.Get<IGameEntity>();
        }

        public override bool canPerform => !complete;

        public override bool complete => Vector2.Distance(_gameEntity.position, _target) < 0.01f;

        public override void Start()
        {
            base.Start();
            _target = _destination();
            botController.Blackboard.SetValue(BlackboardKeys.IsMoving, true);
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            var direction = (_target - _gameEntity.position).normalized;
            botController.Blackboard.SetValue(BlackboardKeys.IsFacingLeft, direction.x < 0);
            _gameEntity.position += botController.Blackboard.GetValue(BlackboardKeys.MoveSpeed) * deltaTime * direction;
        }

        public override void Stop()
        {
            base.Stop();

            botController.Blackboard.Remove(BlackboardKeys.IsMoving);
        }
    }
}
