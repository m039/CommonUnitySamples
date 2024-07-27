using UnityEngine;

namespace Game.StateMachine
{
    public class IdleBotState : BotState
    {
        #region Inspector

        [SerializeField] float _Timer = 1f;

        #endregion

        float _timer;

        public bool IsComplete => _timer <= 0;

        public override void OnEnter()
        {
            base.OnEnter();

            _timer = _Timer;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            _timer -= Time.deltaTime;
        }
    }
}
