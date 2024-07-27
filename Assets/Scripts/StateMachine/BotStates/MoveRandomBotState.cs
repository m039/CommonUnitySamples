using UnityEngine;

namespace Game
{
    public class MoveRandomBotState : MoveBotState
    {
        #region Inspector

        [SerializeField] float _Radius = 1;

        #endregion

        Vector2 _target;

        public override Vector2? Target => _target;

        public override void OnEnter()
        {
            base.OnEnter();

            Vector2 p = botController.StartPosition;
            _target = p + Random.insideUnitCircle * _Radius;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _Radius);
        }
    }
}
