using UnityEngine;

namespace Game
{
    public class WanderBotState : BotState
    {
        #region Inspector

        [SerializeField] IdleBotState _IdleState;

        [SerializeField] MoveBotState _MoveState;

        #endregion

        protected override void OnInit(BotController botController)
        {
            base.OnInit(botController);

            AddTransition(_IdleState, _MoveState, () => _IdleState.IsComplete);
            AddTransition(_MoveState, _IdleState, () => _MoveState.IsComplete);
            SetState(_IdleState);
        }
    }
}
