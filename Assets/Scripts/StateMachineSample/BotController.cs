using UnityEngine;

namespace Game
{
    public class BotController : CoreBotController
    {
        #region Inspector

        [SerializeField] float _MoveSpeed = 10f;

        #endregion

        protected override void Awake()
        {
            base.Awake();

            Blackboard.SetValue(BlackboardKeys.MoveSpeed, _MoveSpeed);
            Blackboard.SetValue(BlackboardKeys.StartPosition, transform.position);
        }

        void OnValidate()
        {
            Blackboard.SetValue(BlackboardKeys.MoveSpeed, _MoveSpeed);
        }
    }
}
