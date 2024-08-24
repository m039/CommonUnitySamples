using Game.BehaviourTreeSample;
using m039.Common.Pathfindig;
using System;
using System.Collections;
using System.Collections.Generic;
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

        IList<Vector3> _vectorPath;

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

            var path = seeker.Search(_gameEntity.position, destination);
            _vectorPath = null;
            _pathIndex = 1;

            if (path == null)
            {
                // Calculate new path with destination minus the range distance.
                path = seeker.Search(_gameEntity.position, _gameEntity.position + vector.normalized * distance);
            }

            if (path != null && path.vectorPath.Count > 1)
            {
                if (CoreGameController.Instance.ServiceLocator.TryGet(out PathSmoother pathSmooter))
                {
                    _vectorPath = pathSmooter.GetSmoothPath(path.vectorPath);
                } else
                {
                    _vectorPath = path.vectorPath;
                }
            }

            botController.ServiceLocator.Get<DebugBotSystem>().DebugPath(_vectorPath);

            if (_vectorPath != null && _vectorPath.Count > 1)
            {
                botController.Blackboard.SetValue(BlackboardKeys.Destination, _vectorPath[_pathIndex]);
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

            if (_vectorPath == null)
                return;

            if (Vector2.Distance(_gameEntity.position, _vectorPath[_pathIndex]) < 0.01f) {
                _pathIndex++;

                if (_pathIndex < _vectorPath.Count)
                {
                    botController.Blackboard.SetValue(BlackboardKeys.Destination, _vectorPath[_pathIndex]);
                } else
                {
                    botController.Blackboard.SetValue(BlackboardKeys.Destination, _destination());
                    _vectorPath = null;
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
