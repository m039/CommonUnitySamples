using m039.Common.BehaviourTrees;
using m039.Common.Blackboard;
using System.Collections.Generic;
using UnityEngine;

namespace Game.BehaviourTreeSample
{
    public class FindFreeTargetBotNode : CoreBotNode
    {
        public enum PositionType
        {
            Current, Start
        }

        #region Inspector

        [SerializeField]
        float _Radius = 1;

        [SerializeField]
        GameEntityType _TargetType = GameEntityType.Food;

        [SerializeField]
        int _MaxTaken = 1;

        [SerializeField]
        PositionType _PositionType;

        #endregion

        BlackboardKey<HashSet<int>> _takenKey;

        readonly Queue<ActionInternal> _actionPool = new();

        LayerMask _layerMask;

        public override void Init(CoreBotController botController)
        {
            base.Init(botController);

            _takenKey = new(nameof(FindFreeTargetBotNode) + ".takenKey");
            _layerMask = LayerMask.GetMask(_TargetType.ToString());
        }

        static Collider2D[] s_Buffer = new Collider2D[16];

        public override Status Process()
        {
            if (!botController.ServiceLocator.TryGet(out IGameEntity botGameEntity))
            {
                return Status.Failure;
            } 

            Vector2 pivotPosition;

            if (_PositionType == PositionType.Current)
            {
                pivotPosition = botGameEntity.position;
            } else if (_PositionType == PositionType.Start)
            {
                if (!botController.Blackboard.TryGetValue(BlackboardKeys.StartPosition, out Vector3 startPosition))
                {
                    return Status.Failure;
                }
                pivotPosition = startPosition;
            } else
            {
                return Status.Failure;
            }

            var count = Physics2D.OverlapCircleNonAlloc(pivotPosition, _Radius, s_Buffer, _layerMask);
            for (int i = 0; i < count; i++)
            {
                if (s_Buffer[i].GetComponentInParent<IGameEntity>() is IGameEntity gameEntity &&
                    gameEntity.type == _TargetType)
                {
                    if (!gameEntity.locator.TryGet(out Blackboard blackboard))
                    {
                        botController.Blackboard.SetValue(BlackboardKeys.Target, gameEntity);
                        return Status.Success;
                    }

                    // Check if the target can be taken.
                    if (blackboard.TryGetValue(_takenKey, out var taken) &&
                        (taken.Contains(botGameEntity.id) || taken.Count >= _MaxTaken))
                    {
                        continue;
                    }

                    var action = GetAction(botGameEntity, gameEntity, blackboard);

                    if (botController.Blackboard.TryGetValue(BlackboardKeys.ExpertActions, out List<System.Action> actions))
                    {
                        actions.Add(action.Run);
                    }
                    else
                    {
                        action.Run();
                    }

                    return Status.Success;
                }
            }

            return Status.Failure;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, _Radius);
        }

        ActionInternal GetAction(IGameEntity botGameEntity, IGameEntity gameEntity, Blackboard gameEntityBlackboard)
        {
            ActionInternal action;

            if (_actionPool.Count <= 0)
            {
                action = new ActionInternal(this);
            } else
            {
                action = _actionPool.Dequeue();
            }

            action.botGameEntity = botGameEntity;
            action.gameEntity = gameEntity;
            action.gameEntityBlackboard = gameEntityBlackboard;

            return action;
        }

        void ReleaseAction(ActionInternal action)
        {
            action.gameEntity = null;
            action.botGameEntity = null;
            action.gameEntityBlackboard = null;
            _actionPool.Enqueue(action);
        }

        // This action executed by Arbiter so a bot with higher insistence can choose what to do first.
        class ActionInternal
        {
            public IGameEntity gameEntity;
            public IGameEntity botGameEntity;
            public Blackboard gameEntityBlackboard;

            readonly FindFreeTargetBotNode _parent;

            public ActionInternal(FindFreeTargetBotNode parent)
            {
                _parent = parent;
            }

            public void Run()
            {
                if (gameEntity == null || botGameEntity == null || gameEntityBlackboard == null)
                {
                    Debug.LogError("The action is disposed.");
                    return;
                }

                if (_parent.botController.Blackboard.ContainsKey(BlackboardKeys.Target))
                {
                    _parent.ReleaseAction(this);
                    return;
                }

                // Set target if it is not already taken or there are not many takers.
                if (!gameEntityBlackboard.TryGetValue(_parent._takenKey, out var taken))
                {
                    _parent.botController.Blackboard.SetValue(BlackboardKeys.Target, gameEntity);
                    gameEntityBlackboard.SetValue(_parent._takenKey, new HashSet<int> { botGameEntity.id });
                }
                else
                {
                    if (!taken.Contains(botGameEntity.id) && taken.Count < _parent._MaxTaken)
                    {
                        taken.Add(botGameEntity.id);
                        _parent.botController.Blackboard.SetValue(BlackboardKeys.Target, gameEntity);
                    }
                }

                _parent.ReleaseAction(this);
            }
        }
    }
}
