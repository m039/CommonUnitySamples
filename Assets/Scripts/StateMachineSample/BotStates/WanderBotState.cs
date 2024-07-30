using UnityEngine;

namespace Game.StateMachineSample
{
    public class WanderBotState : CoreBotState
    {
        #region Inspector

        [SerializeField] IdleBotState _IdleState;

        [SerializeField] MoveBotState _MoveState;

        #endregion

        protected override void OnInit(CoreBotController botController)
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
