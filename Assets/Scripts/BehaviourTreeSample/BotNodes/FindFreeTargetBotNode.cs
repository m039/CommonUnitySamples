using m039.Common.BehaviourTrees;
using m039.Common.Blackboard;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Profiling;

namespace Game.BehaviourTreeSample
{
    public class FindFreeTargetBotNode : CoreBotNode
    {
        public enum PositionType
        {
            Current, Start
        }

        readonly static Queue<ActionInternal> s_ActionPool = new();

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

        BlackboardKey<Taken> _takenKey;

        LayerMask _layerMask;

        public override void Init(CoreBotController botController)
        {
            base.Init(botController);

            _takenKey = new(nameof(FindFreeTargetBotNode) + ".takenKey");
            _layerMask = LayerMask.GetMask(_TargetType.ToString());
        }

        static Collider2D[] s_Buffer = new Collider2D[16];

        protected override Status OnProcess()
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
                var gameEntity = s_Buffer[i].GetComponentInParent<IGameEntity>();

                if (gameEntity != null &&
                    gameEntity.type == _TargetType &&
                    gameEntity.IsAlive &&
                    Vector2.Distance(gameEntity.position, pivotPosition) < _Radius)
                {
                    if (!gameEntity.locator.TryGet(out BlackboardBase blackboard))
                    {
                        botController.Blackboard.SetValue(BlackboardKeys.Target, gameEntity);
                        return Status.Success;
                    }

                    // Check if the target can be taken.
                    if (blackboard.TryGetValue(_takenKey, out var taken) &&
                        (taken.ids.Contains(botGameEntity.id) || taken.ids.Count >= _MaxTaken))
                    {
                        continue;
                    }

                    var action = GetAction(this, botGameEntity, gameEntity, blackboard);

                    if (botController.Blackboard.TryGetValue(BlackboardKeys.ExpertActions, out var actions))
                    {
                        actions.Enqueue(action.action);

                        if (botController.Blackboard.TryGetValue(BlackboardKeys.ExpertAfterAllActions, out var afterAllActions)) {
                            afterAllActions.Enqueue(action.afterAllAction);
                        }
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

        static ActionInternal GetAction(FindFreeTargetBotNode node, IGameEntity botGameEntity, IGameEntity gameEntity, BlackboardBase gameEntityBlackboard)
        {
            ActionInternal action;

            if (s_ActionPool.Count <= 0)
            {
                action = new ActionInternal();
            } else
            {
                action = s_ActionPool.Dequeue();
            }

            action.node = node;
            action.botGameEntity = botGameEntity;
            action.gameEntity = gameEntity;
            action.gameEntityBlackboard = gameEntityBlackboard;

            return action;
        }

        static void ReleaseAction(ActionInternal action)
        {
            action.node = null;
            action.gameEntity = null;
            action.botGameEntity = null;
            action.gameEntityBlackboard = null;
            s_ActionPool.Enqueue(action);
        }

        // This is cache for HashSet<int>.
        class Taken : IReleasable
        {
            static readonly Queue<Taken> s_Cache = new();

            public readonly HashSet<int> ids = new();

            private Taken()
            {
            }

            public static Taken Get(int id)
            {
                Taken taken;
                if (s_Cache.Count > 0)
                {
                    taken = s_Cache.Dequeue();
                }else
                {
                    taken = new Taken();
                }

                taken.ids.Clear();
                taken.ids.Add(id);
                return taken;
            }

            public void Release()
            {
                s_Cache.Enqueue(this);
            }
        }

        // This action executed by Arbiter so a bot with higher insistence can choose what to do first.
        class ActionInternal
        {
            public FindFreeTargetBotNode node;
            public IGameEntity gameEntity;
            public IGameEntity botGameEntity;
            public BlackboardBase gameEntityBlackboard;

            public readonly System.Action action;

            public readonly System.Action afterAllAction;

            public ActionInternal()
            {
                action = new System.Action(Run);
                afterAllAction = new System.Action(Release);
            }

            void Release()
            {
                ReleaseAction(this);
            }

            public void Run()
            {
                if (gameEntity == null || botGameEntity == null || gameEntityBlackboard == null || node == null)
                {
                    Debug.LogError("The action is disposed.");
                    return;
                }

                if (node.botController.Blackboard.ContainsKey(BlackboardKeys.Target))
                {
                    ReleaseAction(this);
                    return;
                }

                // Set target if it is not already taken or there are not many takers.
                if (!gameEntityBlackboard.TryGetValue(node._takenKey, out var taken))
                {
                    node.botController.Blackboard.SetValue(BlackboardKeys.Target, gameEntity);
                    gameEntityBlackboard.SetValue(node._takenKey, Taken.Get(botGameEntity.id));
                }
                else
                {
                    if (!taken.ids.Contains(botGameEntity.id) && taken.ids.Count < node._MaxTaken)
                    {
                        taken.ids.Add(botGameEntity.id);
                        node.botController.Blackboard.SetValue(BlackboardKeys.Target, gameEntity);
                    }
                }

                ReleaseAction(this);
            }
        }
    }
}
