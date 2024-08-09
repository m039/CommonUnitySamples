using UnityEngine;

namespace Game.StateMachineSample
{
    public class MoveRandomBotState : MoveBotState
    {
        #region Inspector

        [SerializeField] float _Radius = 1;

        #endregion

        Vector2 _target;

        public override Vector2? Target => _noStartPosition? null : _target;

        bool _noStartPosition;

        public override void OnEnter()
        {
            base.OnEnter();

            if (botController.Blackboard.TryGetValue(BlackboardKeys.StartPosition, out Vector3 p))
            {
                _target = (Vector2)p + Random.insideUnitCircle * _Radius;
                _noStartPosition = false;
            } else
            {
                _noStartPosition = true;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _Radius);
        }
    }
}
