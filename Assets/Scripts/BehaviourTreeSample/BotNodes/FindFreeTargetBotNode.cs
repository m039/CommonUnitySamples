using m039.Common.BehaviourTrees;
using m039.Common.Blackboard;
using System.Collections.Generic;
using UnityEngine;

namespace Game.BehaviourTreeSample
{
    public class FindFreeTargetBotNode : CoreBotNode
    {
        #region Inspector

        [SerializeField]
        float _Radius = 1;

        [SerializeField]
        GameEntityType _TargetType = GameEntityType.Food;

        [SerializeField]
        int _MaxTaken = 1;

        #endregion

        BlackboardKey _takenKey;

        public override void Init(CoreBotController botController)
        {
            base.Init(botController);

            _takenKey = new(nameof(FindFreeTargetBotNode) + ".takenKey");
        }

        static Collider2D[] s_Buffer = new Collider2D[16];

        public override Status Process()
        {
            if (!botController.Blackboard.TryGetValue(BlackboardKeys.StartPosition, out Vector3 startPosition))
            {
                return Status.Failure;
            }

            var count = Physics2D.OverlapCircleNonAlloc(startPosition, _Radius, s_Buffer);
            for (int i = 0; i < count; i++)
            {
                if (s_Buffer[i].GetComponentInParent<IGameEntity>() is IGameEntity gameEntity &&
                    gameEntity.type == _TargetType)
                {
                    if (!botController.ServiceLocator.TryGet(out IGameEntity botGameEntity))
                    {
                        botController.Blackboard.SetValue(BlackboardKeys.Target, gameEntity);
                        return Status.Success;
                    }

                    if (!gameEntity.locator.TryGet(out Blackboard blackboard))
                    {
                        botController.Blackboard.SetValue(BlackboardKeys.Target, gameEntity);
                        return Status.Success;
                    }

                    // Check if the target can be taken.
                    if (blackboard.TryGetValue(_takenKey, out HashSet<int> taken) &&
                        (taken.Contains(botGameEntity.id) || taken.Count >= _MaxTaken))
                    {
                        continue;
                    }

                    void action()
                    {
                        if (botController.Blackboard.ContainsKey(BlackboardKeys.Target))
                            return;

                        // Set target if it is not already taken or there are not many takers.
                        if (!blackboard.TryGetValue(_takenKey, out HashSet<int> taken))
                        {
                            botController.Blackboard.SetValue(BlackboardKeys.Target, gameEntity);
                            blackboard.SetValue(_takenKey, new HashSet<int> { botGameEntity.id });
                        }
                        else
                        {
                            if (!taken.Contains(botGameEntity.id) && taken.Count < _MaxTaken)
                            {
                                taken.Add(botGameEntity.id);
                                botController.Blackboard.SetValue(BlackboardKeys.Target, gameEntity);
                            }
                        }
                    }

                    if (botController.Blackboard.TryGetValue(BlackboardKeys.ExpertActions, out List<System.Action> actions))
                    {
                        actions.Add(action);
                    }
                    else
                    {
                        action();
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
    }
}
