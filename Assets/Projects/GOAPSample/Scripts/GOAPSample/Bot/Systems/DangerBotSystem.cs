using Game.BehaviourTreeSample;
using UnityEngine;

namespace Game
{
    public class DangerBotSystem : BotSystem
    {

        #region Inspector

        [SerializeField]
        float _SensorRadius = 2f;

        #endregion

        static Collider2D[] s_Buffer = new Collider2D[16];

        public override void Process(float deltaTime)
        {
            base.Process(deltaTime);

            if (botController.Blackboard.ContainsKey(BlackboardKeys.InDanger) ||
                botController.Blackboard.ContainsKey(BlackboardKeys.IsInvisible))
            {
                return;
            }

            var gameEntity = botController.ServiceLocator.Get<IGameEntity>();
            var count = Physics2D.OverlapCircleNonAlloc(gameEntity.position, _SensorRadius, s_Buffer);

            for (int i = 0; i < count; i++)
            {
                if (s_Buffer[i].GetComponentInParent<IGameEntity>() is IGameEntity target &&
                    target.type == GameEntityType.Troll)
                {
                    botController.Blackboard.SetValue(BlackboardKeys.InDanger, true);
                    break;
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _SensorRadius);
        }

    }
}
