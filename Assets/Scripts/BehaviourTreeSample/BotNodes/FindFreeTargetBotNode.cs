using Game.BehaviourTreeSample;
using m039.Common.BehaviourTrees;
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

        #endregion

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
                    botController.Blackboard.SetValue(BlackboardKeys.Target, gameEntity);
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
