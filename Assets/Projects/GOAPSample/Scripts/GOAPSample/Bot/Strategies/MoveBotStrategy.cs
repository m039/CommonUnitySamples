using Game.BehaviourTreeSample;
using m039.Common.Pathfindig;
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

        public override bool complete => Vector2.Distance(_gameEntity.position, _destination()) < _inRangeOf();

        Path _path;

        int _pathIndex;

        public override void Start()
        {
            base.Start();

            var seeker = CoreGameController.Instance.ServiceLocator.Get<Seeker>();

            var destination = _destination();
            var inRangeOf = _inRangeOf();
            var vector = (destination - _gameEntity.position);
            var distance = vector.magnitude - inRangeOf;

            if (distance < 0)
            {
                return;
            }

            _path = seeker.Search(_gameEntity.position, destination);
            _pathIndex = 1;

            if (_path == null)
            {
                // Calculate new path with destination minus the range distance.
                _path = seeker.Search(_gameEntity.position, _gameEntity.position + vector.normalized * distance);
            }

            botController.ServiceLocator.Get<DebugBotSystem>().DebugPath(_path);

            if (_path != null && _path.vectorPath.Count > 1)
            {
                botController.Blackboard.SetValue(BlackboardKeys.Destination, _path.vectorPath[_pathIndex]);
                botController.Blackboard.SetValue(BlackboardKeys.DestinationThreshold, 0.01f);
            } else
            {
                botController.Blackboard.SetValue(BlackboardKeys.Destination, destination);
                botController.Blackboard.SetValue(BlackboardKeys.DestinationThreshold, inRangeOf);
            }

            botController.Blackboard.SetValue(BlackboardKeys.MoveSpeedMultiplier, _moveSpeedMultiplier);
            botController.Blackboard.SetValue(BlackboardKeys.MoveAnimationSpeed, _moveSpeedMultiplier);
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            if (_path == null)
                return;

            if (Vector2.Distance(_gameEntity.position, _path.vectorPath[_pathIndex]) < 0.01f) {
                _pathIndex++;

                if (_pathIndex < _path.vectorPath.Count)
                {
                    botController.Blackboard.SetValue(BlackboardKeys.Destination, _path.vectorPath[_pathIndex]);
                } else
                {
                    botController.Blackboard.SetValue(BlackboardKeys.Destination, _destination());
                    _path = null;
                }
            }
        }

        public override void Stop()
        {
            base.Stop();

            botController.Blackboard.Remove(BlackboardKeys.Destination);
        }
    }
}
