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
        }

        void OnValidate()
        {
            if (!Application.isPlaying)
                return;

            Blackboard.SetValue(BlackboardKeys.MoveSpeed, _MoveSpeed);
        }
    }
}
