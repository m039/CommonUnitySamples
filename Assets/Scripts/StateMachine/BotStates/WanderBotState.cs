using m039.Common.StateMachine;
using UnityEngine;

namespace Game.StateMachine
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

            var idleState = _IdleState;
            var moveState = _MoveState;

            AddTransition(idleState, moveState, () => _IdleState.IsComplete);
            AddTransition(moveState, idleState, () => _MoveState.IsComplete);
            SetState(idleState);
        }
    }
}
