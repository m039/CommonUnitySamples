using UnityEditor;
using UnityEngine;

namespace Game
{
    public class MoveBotState : BotState
    {

        #region Inspector

        [SerializeField] float _SpeedMultiplier = 1f;

        [SerializeField] Transform _Target;

        #endregion

        public bool IsComplete
        {
            get
            {
                if (!Target.HasValue)
                    return false;

                return Vector2.Distance(Target.Value, (Vector2)botController.transform.position) < 0.1f;
            }
        }

        public virtual Vector2? Target
        {
            get
            {
                if (_Target == null)
                    return null;

                return _Target.position;
            }
        }

        void Move(Vector2 to) {
            var p = botController.transform.position;
            var delta = (to - (Vector2)p).normalized * botController.MoveSpeed * _SpeedMultiplier * Time.deltaTime;
            botController.transform.position += (Vector3)delta;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (Target.HasValue)
            {
                Move(Target.Value);
            }
        }
    }
}
